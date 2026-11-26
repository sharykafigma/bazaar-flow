using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BazaarFlow
{
    public enum GameState { Playing, Won, Jammed }

    /// <summary>
    /// Runs one level: builds the board, moves the belt, lets shops grab goods, handles taps and win / Belt Jam.
    /// The HUD is plain OnGUI for the prototype; it becomes real UI in the art milestone.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        const string LevelKey = "bf_level";

        // Board layout in world units (portrait camera, about 9 x 16 units visible).
        static readonly Vector2 BeltCenter = new Vector2(0f, 1.6f);
        const float BeltWidth = 7.2f;
        const float BeltHeight = 7.0f;
        const float ShopInset = 1.05f;

        /// <summary>Where shops sit, as a fraction along each belt edge. Order = order shops are filled in.</summary>
        static readonly (int edge, float t)[] ShopSpots =
        {
            (0, 0.35f), (0, 0.65f), // top
            (1, 0.35f), (3, 0.65f), // right upper, left upper
            (1, 0.75f), (3, 0.25f), // right lower, left lower
        };

        public GameState State { get; private set; }
        public int LevelNumber { get; private set; }
        public int Coins => Economy.Coins;

        LevelData _level;
        Transform _root;
        Belt _belt;
        Godown _godown;
        readonly List<Shop> _shops = new List<Shop>();
        readonly Queue<ItemColor> _gateQueue = new Queue<ItemColor>();
        Camera _cam;
        int _deliveries;
        bool _continueUsed;
        int _lastReward;
        bool _rewardDoubled;
        bool _waitingForAd;

        void Start()
        {
            _cam = Camera.main;
            LoadLevel(Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, 1)));
        }

        // ---------------- Level setup ----------------

        public void LoadLevel(int number)
        {
            if (_root != null) Destroy(_root.gameObject);
            _shops.Clear();
            _gateQueue.Clear();
            _deliveries = 0;
            _continueUsed = false;
            _rewardDoubled = false;
            _waitingForAd = false;
            AdsService.SetBannerVisible(false);

            LevelNumber = number;
            _level = LevelLibrary.Get(number);
            if (!_level.IsBalanced(out var problem))
                Debug.LogError($"Level {number} is not balanced: {problem}");

            _root = new GameObject("Level " + number).transform;

            _belt = new GameObject("Belt").AddComponent<Belt>();
            _belt.transform.SetParent(_root, false);
            _belt.Init(BeltCenter, BeltWidth, BeltHeight, _level.beltSlots, _level.beltSpeed);

            // Gate between the belt and the godown.
            Vector3 gatePos = _belt.PointAt(_belt.GateS);
            SpriteFactory.Create("Gate", _root, SpriteFactory.Square, GameColors.Gate,
                gatePos + new Vector3(0, -0.75f, 0.05f), new Vector2(0.9f, 0.7f), 1);

            for (int i = 0; i < _level.shops.Count && i < ShopSpots.Length; i++)
            {
                var (edge, t) = ShopSpots[i];
                float s = EdgeToS(edge, t);
                Vector3 pos = _belt.PointAt(s) + _belt.InwardAt(s) * ShopInset;
                var shop = new GameObject("Shop" + i).AddComponent<Shop>();
                shop.transform.SetParent(_root, false);
                shop.Init(i, _level.shops[i], s, pos);
                shop.Completed += OnShopCompleted;
                _shops.Add(shop);
            }

            _godown = new GameObject("Godown").AddComponent<Godown>();
            _godown.transform.SetParent(_root, false);
            _godown.Init(_level, new Vector2(0f, BeltCenter.y - BeltHeight * 0.5f - 1.75f), 6.5f);

            State = GameState.Playing;
        }

        float EdgeToS(int edge, float t)
        {
            switch (edge)
            {
                case 0: return BeltWidth * t;
                case 1: return BeltWidth + BeltHeight * t;
                case 2: return BeltWidth + BeltHeight + BeltWidth * t;
                default: return 2 * BeltWidth + BeltHeight + BeltHeight * t;
            }
        }

        // ---------------- Loop ----------------

        void Update()
        {
            if (_belt == null) return;
            if (AdsService.AdShowing || ShopUI.IsOpen) return; // pause under ads and the shop
            if (State == GameState.Playing) HandleInput();
            if (State != GameState.Jammed) Simulate(Time.deltaTime);
            if (State == GameState.Playing) CheckEnd();
        }

        void Simulate(float dt)
        {
            var slots = _belt.Slots;
            int n = slots.Length;
            var prev = new float[n];
            for (int i = 0; i < n; i++) prev[i] = _belt.SlotS(i);
            float moved = _belt.Advance(dt);

            for (int i = 0; i < n; i++)
            {
                var item = slots[i];
                if (item != null)
                {
                    foreach (var shop in _shops)
                    {
                        if (shop.CanTakeNow(item.Color) && _belt.Crossed(prev[i], moved, shop.PathS))
                        {
                            slots[i] = null;
                            shop.Take(item);
                            _deliveries++;
                            break;
                        }
                    }
                }
                else if (_gateQueue.Count > 0 && _belt.Crossed(prev[i], moved, _belt.GateS))
                {
                    var color = _gateQueue.Dequeue();
                    slots[i] = BeltItem.Spawn(color, _belt.PointAt(_belt.SlotS(i)), _root);
                }
            }
            _belt.UpdateVisuals();
        }

        void HandleInput()
        {
            if (!PointerDown(out Vector2 screen)) return;
            if (_gateQueue.Count > 0) return; // one crate at a time through the gate

            Vector2 world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_cam.transform.position.z));
            var crate = _godown.FrontCrateAt(world, out int column);
            if (crate == null) return;

            for (int i = 0; i < crate.Count; i++) _gateQueue.Enqueue(crate.Color);
            _godown.RemoveFront(column);
        }

        static bool PointerDown(out Vector2 pos)
        {
#if ENABLE_INPUT_SYSTEM
            var p = Pointer.current;
            if (p != null && p.press.wasPressedThisFrame)
            {
                pos = p.position.ReadValue();
                return true;
            }
            pos = default;
            return false;
#else
            if (Input.GetMouseButtonDown(0))
            {
                pos = Input.mousePosition;
                return true;
            }
            pos = default;
            return false;
#endif
        }

        void OnShopCompleted(Shop done)
        {
            foreach (var s in _shops)
                if (s.IsLocked && s.LockedUntil == done.Index) s.Unlock();
        }

        void CheckEnd()
        {
            bool allDone = true;
            foreach (var s in _shops) if (!s.IsDone) { allDone = false; break; }
            if (allDone)
            {
                Win();
                return;
            }

            // Belt Jam: every slot is full and nothing on the belt is wanted by any open shop.
            if (_belt.CountFree() > 0) return;
            foreach (var item in _belt.Slots)
                foreach (var shop in _shops)
                    if (shop.WillNeed(item.Color)) return;
            State = GameState.Jammed;
            AdsService.SetBannerVisible(true);
        }

        void Win()
        {
            State = GameState.Won;
            _lastReward = 10 + LevelNumber;
            Economy.AddCoins(_lastReward);
            PlayerPrefs.SetInt(LevelKey, LevelNumber + 1);
            PlayerPrefs.Save();
            AdsService.SetBannerVisible(true);
        }

        // ---------------- Monetization hooks ----------------

        /// <summary>Rewarded ad on the win screen: gives the level reward a second time.</summary>
        public void DoubleCoinsWithAd()
        {
            if (_rewardDoubled || _waitingForAd) return;
            _waitingForAd = true;
            AdsService.ShowRewarded("double_coins", ok =>
            {
                _waitingForAd = false;
                if (!ok) return;
                _rewardDoubled = true;
                Economy.AddCoins(_lastReward);
            });
        }

        /// <summary>Win screen "next": may show an interstitial (pacing rules in MonetizationConfig) first.</summary>
        public void NextLevel()
        {
            if (_waitingForAd) return;
            _waitingForAd = true;
            int next = LevelNumber + 1;
            AdsService.OnLevelWon(LevelNumber, () => LoadLevel(next));
        }

        /// <summary>Belt Jam "continue": rewarded ad, then 6 goods go back to the godown.</summary>
        public void ContinueWithAd()
        {
            if (_continueUsed || _waitingForAd) return;
            _waitingForAd = true;
            AdsService.ShowRewarded("continue", ok =>
            {
                _waitingForAd = false;
                if (ok && State == GameState.Jammed) ContinueAfterJam();
            });
        }

        /// <summary>Belt Jam "try again".</summary>
        public void RetryAfterJam()
        {
            AdsService.OnLevelLost();
            LoadLevel(LevelNumber);
        }

        public bool CanUseBooster(BoosterType b)
        {
            if (_godown == null || State == GameState.Won) return false;
            switch (b)
            {
                case BoosterType.Jhaaru: return _belt.CountFree() < _belt.SlotCount;
                case BoosterType.Chashma:
                    foreach (var col in _godown.Columns)
                        foreach (var c in col) if (c.Hidden) return true;
                    return false;
                default: return false; // ExtraPatri / UltiBelt: not built yet
            }
        }

        /// <summary>Uses one booster from the wallet. Returns false (and keeps the booster) if it would do nothing.</summary>
        public bool UseBooster(BoosterType b)
        {
            if (!CanUseBooster(b) || !Economy.TryUseBooster(b)) return false;
            switch (b)
            {
                case BoosterType.Jhaaru: SweepMostCommonColour(); break;
                case BoosterType.Chashma: RevealAllCrates(); break;
            }
            if (State == GameState.Jammed && _belt.CountFree() > 0)
            {
                State = GameState.Playing;
                AdsService.SetBannerVisible(false);
            }
            return true;
        }

        /// <summary>Jhaaru: every good of the most common colour on the belt goes back to the godown.</summary>
        void SweepMostCommonColour()
        {
            var slots = _belt.Slots;
            var counts = new int[GameColors.Count];
            foreach (var it in slots) if (it != null) counts[(int)it.Color]++;
            int best = 0;
            for (int i = 1; i < counts.Length; i++) if (counts[i] > counts[best]) best = i;
            var color = (ItemColor)best;
            int removed = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null || slots[i].Color != color) continue;
                Destroy(slots[i].gameObject);
                slots[i] = null;
                removed++;
            }
            while (removed > 0)
            {
                int n = Mathf.Min(6, removed);
                _godown.PushFront(new CrateDef(color, n));
                removed -= n;
            }
        }

        /// <summary>Chashma: shows the colour of every hidden crate.</summary>
        void RevealAllCrates()
        {
            foreach (var col in _godown.Columns)
                foreach (var c in col) c.Reveal();
        }

        /// <summary>
        /// Rewarded-ad continue (no ad SDK yet, so it is free in the prototype):
        /// sends up to 6 unwanted goods back to the godown as crates.
        /// </summary>
        public void ContinueAfterJam()
        {
            var slots = _belt.Slots;
            var counts = new Dictionary<ItemColor, int>();
            int removed = 0;
            for (int i = 0; i < slots.Length && removed < 6; i++)
            {
                var item = slots[i];
                if (item == null) continue;
                counts.TryGetValue(item.Color, out int c);
                counts[item.Color] = c + 1;
                Destroy(item.gameObject);
                slots[i] = null;
                removed++;
            }
            foreach (var kv in counts) _godown.PushFront(new CrateDef(kv.Key, kv.Value));
            _continueUsed = true;
            State = GameState.Playing;
        }

        // ---------------- Prototype HUD ----------------

        GUIStyle _label, _big, _button, _small, _hud;

        void OnGUI()
        {
            if (_belt == null) return;
            float u = Screen.height / 100f; // 1% of screen height
            EnsureStyles(u);

            // Top bar
            GUI.Label(new Rect(u * 2, u * 2, Screen.width * 0.5f, u * 5), $"Level {LevelNumber}", _hud);
            var coinsStyle = new GUIStyle(_hud) { alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width * 0.5f, u * 2, Screen.width * 0.5f - u * 2, u * 5), $"Coins {Coins}", coinsStyle);
            int free = _belt.CountFree();
            GUI.Label(new Rect(u * 2, u * 6, Screen.width, u * 4), $"Free slots: {free}/{_belt.SlotCount}", _small);
            BoosterBar(u);

            // Shop order labels
            foreach (var s in _shops)
            {
                string text = s.IsDone ? "Done" : s.IsLocked ? "Locked" : $"{s.Remaining}";
                if (!s.IsDone && !s.IsLocked && s.OrdersLeft > 1) text += $"\n+{s.OrdersLeft - 1}";
                WorldLabel(s.transform.position + new Vector3(0, -0.05f, 0), text, _label, u);
            }

            // Crate counts
            foreach (var col in _godown.Columns)
                foreach (var crate in col)
                    WorldLabel(crate.transform.position, crate.Hidden ? "?" : crate.Count.ToString(), _label, u);

            // Gate queue
            if (_gateQueue.Count > 0)
                WorldLabel(_belt.PointAt(_belt.GateS) + new Vector3(0, -0.75f, 0), _gateQueue.Count.ToString(), _label, u);

            if (LevelNumber == 1 && State == GameState.Playing && _deliveries == 0 && _gateQueue.Count == 0)
                CenterText(u, "Tap a crate below!", Screen.height * 0.47f);

            if (State == GameState.Won) WinPanel(u);
            else if (State == GameState.Jammed) JamPanel(u);
        }

        void BoosterBar(float u)
        {
            float bw = u * 11, bh = u * 5, y = u * 7;
            float x = Screen.width - u * 2 - bw;
            if (GUI.Button(new Rect(x, y, bw, bh), "Shop", _button)) ShopUI.Open();
            if (State != GameState.Playing) return;
            foreach (var b in Economy.LiveBoosters)
            {
                x -= bw + u;
                int n = Economy.BoosterCount(b);
                GUI.enabled = CanUseBooster(b) || n == 0;
                if (GUI.Button(new Rect(x, y, bw, bh), $"{Boosters.Name(b)} {(n > 0 ? n.ToString() : "+")}", _button))
                {
                    if (n > 0) UseBooster(b);
                    else ShopUI.Open();
                }
                GUI.enabled = true;
            }
        }

        void WinPanel(float u)
        {
            Panel(u, "Well done!", _rewardDoubled ? $"+{_lastReward * 2} coins (doubled!)" : $"+{_lastReward} coins");
            int row = 0;
            if (!_rewardDoubled && GUI.Button(ButtonRect(u, row++), "Double coins (ad)", _button)) DoubleCoinsWithAd();
            if (GUI.Button(ButtonRect(u, row), "Next level", _button)) NextLevel();
        }

        void JamPanel(float u)
        {
            Panel(u, "Belt Jam!", "No shop wants the goods on the belt");
            int row = 0;
            if (!_continueUsed && GUI.Button(ButtonRect(u, row++), "Continue (ad)", _button)) ContinueWithAd();
            if (Economy.BoosterCount(BoosterType.Jhaaru) > 0 && CanUseBooster(BoosterType.Jhaaru)
                && GUI.Button(ButtonRect(u, row++), $"Use {Boosters.Name(BoosterType.Jhaaru)} ({Economy.BoosterCount(BoosterType.Jhaaru)})", _button))
                UseBooster(BoosterType.Jhaaru);
            if (GUI.Button(ButtonRect(u, row++), "Try again", _button)) RetryAfterJam();
            if (Economy.StarterOfferActive && GUI.Button(ButtonRect(u, row), "Starter Pack offer!", _button)) ShopUI.Open();
        }

        void Panel(float u, string title, string sub)
        {
            var r = new Rect(Screen.width * 0.1f, Screen.height * 0.3f, Screen.width * 0.8f, Screen.height * 0.34f);
            GUI.Box(r, GUIContent.none);
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x, r.y + u * 2, r.width, u * 7), title, _big);
            GUI.Label(new Rect(r.x, r.y + u * 9, r.width, u * 4), sub, new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter });
        }

        Rect ButtonRect(float u, int index)
        {
            float w = Screen.width * 0.6f;
            return new Rect((Screen.width - w) * 0.5f, Screen.height * 0.3f + u * 15 + index * u * 8, w, u * 6.5f);
        }

        void CenterText(float u, string text, float y)
        {
            GUI.Label(new Rect(0, y, Screen.width, u * 5), text, new GUIStyle(_hud) { alignment = TextAnchor.MiddleCenter });
        }

        void WorldLabel(Vector3 world, string text, GUIStyle style, float u)
        {
            Vector3 sp = _cam.WorldToScreenPoint(world);
            var r = new Rect(sp.x - u * 5, Screen.height - sp.y - u * 3, u * 10, u * 6);
            GUI.Label(r, text, style);
        }

        void EnsureStyles(float u)
        {
            if (_label != null && _label.fontSize == Mathf.RoundToInt(u * 2.6f)) return;
            _label = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(u * 2.6f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            _small = new GUIStyle(_label) { fontSize = Mathf.RoundToInt(u * 1.9f), fontStyle = FontStyle.Normal, alignment = TextAnchor.UpperLeft };
            _small.normal.textColor = new Color(0.25f, 0.2f, 0.18f);
            _big = new GUIStyle(_label) { fontSize = Mathf.RoundToInt(u * 5f) };
            _hud = new GUIStyle(_label) { alignment = TextAnchor.UpperLeft };
            _hud.normal.textColor = new Color(0.25f, 0.2f, 0.18f);
            _button = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(u * 2.6f), fontStyle = FontStyle.Bold };
        }
    }
}
