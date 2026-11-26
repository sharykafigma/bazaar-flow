using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BazaarFlow
{
    public enum GameState { Playing, Won, Jammed }

    /// <summary>
    /// Runs one level: builds the board, moves the belt, lets shops grab goods, handles taps, boosters,
    /// win and "Jammed!". Other systems (UI, art, sound, ads) listen to the events and public state below.
    /// The OnGUI HUD is a fallback for running gameplay alone; the uGUI layer turns it off.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>Set to false by the UI layer: hides the OnGUI top bar and win/jam panels (world labels stay).</summary>
        public static bool DrawPrototypeHud = true;

        /// <summary>Old OnGUI counts on shops and crates. Off: WorldLabels draws them in the world instead.</summary>
        public static bool DrawWorldLabels = false;

        public const int ExtraBeltSlots = 4;

        // Board layout in world units (portrait camera, about 9 x 16 units visible).
        static readonly Vector2 BeltCenter = new Vector2(0f, 1.6f);
        const float BeltWidth = 7.2f;
        const float BeltHeight = 7.0f;
        const float ShopInset = 1.05f;

        const float RushEvery = 16f;
        const float RushLength = 5f;
        const float RushMultiplier = 1.7f;

        /// <summary>Where shops sit, as a fraction along each belt edge. Order = order shops are filled in.</summary>
        static readonly (int edge, float t)[] ShopSpots =
        {
            (0, 0.35f), (0, 0.65f), // top
            (1, 0.35f), (3, 0.65f), // right upper, left upper
            (1, 0.75f), (3, 0.25f), // right lower, left lower
        };

        // ---------------- Events ----------------

        public static event Action<int> LevelStarted;
        /// <summary>(level, coin reward, stars 1-3)</summary>
        public static event Action<int, int, int> LevelWon;
        public static event Action<int> LevelJammed;
        public static event Action<int> LevelContinued;
        /// <summary>A crate was tapped: (colour, goods count).</summary>
        public static event Action<ItemColor, int> CrateTapped;
        public static event Action<Shop, ItemColor> GoodDelivered;
        public static event Action<Shop> ShopCompleted;
        public static event Action<BoosterType> BoosterUsed;
        /// <summary>Rush hour started (true) or ended (false).</summary>
        public static event Action<bool> RushChanged;

        // ---------------- State ----------------

        public GameState State { get; private set; }
        public int LevelNumber { get; private set; }
        public int Coins => Economy.Coins;
        public LevelData Level => _level;
        public Belt Belt => _belt;
        public Godown Godown => _godown;
        public IReadOnlyList<Shop> Shops => _shops;
        public int GateQueue => _gateQueue.Count;

        public int BeltCapacity => _belt != null ? _belt.SlotCount : 0;
        public int BeltFree => _belt != null ? _belt.CountFree() : 0;
        /// <summary>0 = empty belt, 1 = full.</summary>
        public float BeltFill => BeltCapacity > 0 ? 1f - (float)BeltFree / BeltCapacity : 0f;

        public bool RushActive { get; private set; }
        public bool ContinueUsed => _continueUsed;
        public int BoostersUsedThisLevel => _boostersUsed;
        /// <summary>Coin reward of the last win (before any doubling).</summary>
        public int LastReward => _lastReward;
        /// <summary>Stars of the last win.</summary>
        public int LastStars { get; private set; }

        LevelData _level;
        Transform _root;
        Belt _belt;
        Godown _godown;
        readonly List<Shop> _shops = new List<Shop>();
        readonly Queue<ItemColor> _gateQueue = new Queue<ItemColor>();
        Camera _cam;
        int _deliveries;
        bool _continueUsed;
        int _boostersUsed;
        int _lastReward;
        bool _rewardDoubled;
        bool _waitingForAd;
        float _rushClock;
        float[] _prevS = new float[0];

        void Awake()
        {
            if (GetComponent<WorldLabels>() == null) gameObject.AddComponent<WorldLabels>();
        }

        void Start()
        {
            _cam = Camera.main;
            if (LevelNumber == 0) LoadLevel(Progress.CurrentLevel);
        }

        // ---------------- Level setup ----------------

        public void LoadLevel(int number)
        {
            if (_cam == null) _cam = Camera.main;
            if (_root != null) Destroy(_root.gameObject);
            _shops.Clear();
            _gateQueue.Clear();
            _deliveries = 0;
            _continueUsed = false;
            _boostersUsed = 0;
            _rewardDoubled = false;
            _waitingForAd = false;
            _rushClock = 0f;
            RushActive = false;
            AdsService.SetBannerVisible(false);

            LevelNumber = Mathf.Max(1, number);
            _level = LevelLibrary.Get(LevelNumber);
            if (!_level.IsBalanced(out var problem))
                Debug.LogError($"Level {LevelNumber} is not balanced: {problem}");

            _root = new GameObject("Level " + LevelNumber).transform;

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
                shop.Delivered += (sh, c) => GoodDelivered?.Invoke(sh, c);
                _shops.Add(shop);
            }

            _godown = new GameObject("Godown").AddComponent<Godown>();
            _godown.transform.SetParent(_root, false);
            _godown.Init(_level, new Vector2(0f, BeltCenter.y - BeltHeight * 0.5f - 1.75f), 6.5f);

            State = GameState.Playing;
            LevelStarted?.Invoke(LevelNumber);
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
            if (AdsService.AdShowing || ShopUI.IsOpen) return; // pause under ads and the OnGUI shop
            if (State == GameState.Playing) HandleInput();
            if (State != GameState.Jammed)
            {
                UpdateRush(Time.deltaTime);
                Simulate(Time.deltaTime);
            }
            if (State == GameState.Playing) CheckEnd();
        }

        void UpdateRush(float dt)
        {
            if (!_level.rushHour || State != GameState.Playing)
            {
                if (RushActive) SetRush(false);
                return;
            }
            _rushClock += dt;
            float phase = _rushClock % (RushEvery + RushLength);
            SetRush(phase >= RushEvery);
        }

        void SetRush(bool on)
        {
            if (RushActive == on) return;
            RushActive = on;
            _belt.SpeedMultiplier = on ? RushMultiplier : 1f;
            RushChanged?.Invoke(on);
        }

        void Simulate(float dt)
        {
            var slots = _belt.Slots;
            int n = slots.Length;
            if (_prevS.Length < n) _prevS = new float[n];
            for (int i = 0; i < n; i++) _prevS[i] = _belt.SlotS(i);
            float moved = _belt.Advance(dt);

            for (int i = 0; i < n; i++)
            {
                var item = slots[i];
                if (item != null)
                {
                    foreach (var shop in _shops)
                    {
                        if (shop.CanTakeNow(item.Color) && _belt.Crossed(_prevS[i], moved, shop.PathS))
                        {
                            slots[i] = null;
                            shop.Take(item);
                            _deliveries++;
                            break;
                        }
                    }
                }
                else if (_gateQueue.Count > 0 && _belt.Crossed(_prevS[i], moved, _belt.GateS))
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
            if (PointerOverUI()) return;          // taps on buttons must not hit crates
            if (_gateQueue.Count > 0) return;     // one crate at a time through the gate

            Vector2 world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_cam.transform.position.z));
            var crate = _godown.FrontCrateAt(world, out int column);
            if (crate == null) return;

            var color = crate.Color;
            int count = crate.Count;
            for (int i = 0; i < count; i++) _gateQueue.Enqueue(color);
            _godown.RemoveFront(column);
            CrateTapped?.Invoke(color, count);
        }

        static bool PointerOverUI()
        {
            var es = EventSystem.current;
            if (es == null) return false;
            if (es.IsPointerOverGameObject()) return true;
#if !ENABLE_INPUT_SYSTEM
            for (int i = 0; i < Input.touchCount; i++)
                if (es.IsPointerOverGameObject(Input.GetTouch(i).fingerId)) return true;
#endif
            return false;
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
            ShopCompleted?.Invoke(done);
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

            // Jammed: every slot is full and nothing on the belt is wanted by any open shop.
            if (_belt.CountFree() > 0) return;
            foreach (var item in _belt.Slots)
                foreach (var shop in _shops)
                    if (shop.WillNeed(item.Color)) return;
            Jam();
        }

        void Jam()
        {
            State = GameState.Jammed;
            SetRush(false);
            Lives.LoseLife();
            Progress.OnLevelLost();
            AdsService.SetBannerVisible(true);
            LevelJammed?.Invoke(LevelNumber);
        }

        void Win()
        {
            State = GameState.Won;
            SetRush(false);
            LastStars = Mathf.Clamp(3 - (_continueUsed ? 1 : 0) - (_boostersUsed > 0 ? 1 : 0), 1, 3);
            _lastReward = RewardFor(_level);
            Economy.AddCoins(_lastReward);
            Progress.OnLevelWon(LevelNumber, LastStars);
            AdsService.SetBannerVisible(true);
            LevelWon?.Invoke(LevelNumber, _lastReward, LastStars);
        }

        public static int RewardFor(LevelData level)
        {
            int baseReward = 10 + level.number;
            switch (level.difficulty)
            {
                case Difficulty.Hard: return Mathf.RoundToInt(baseReward * 1.5f);
                case Difficulty.VeryHard: return baseReward * 2;
                default: return baseReward;
            }
        }

        // ---------------- Monetization hooks (used by the OnGUI fallback; the uGUI layer has its own) ----------------

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

        /// <summary>Jammed "continue": rewarded ad, then 6 goods go back to the godown.</summary>
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

        /// <summary>Jammed "try again".</summary>
        public void RetryAfterJam()
        {
            AdsService.OnLevelLost();
            LoadLevel(LevelNumber);
        }

        // ---------------- Boosters ----------------

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
                case BoosterType.ExtraPatri: return _belt.SlotCount < _level.beltSlots + ExtraBeltSlots * 2;
                case BoosterType.UltiBelt: return State == GameState.Playing && !_belt.Reversed;
                default: return false;
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
                case BoosterType.ExtraPatri: _belt.AddSlots(ExtraBeltSlots); break;
                case BoosterType.UltiBelt: _belt.ReverseOneLap(); break;
            }
            _boostersUsed++;
            if (State == GameState.Jammed && _belt.CountFree() > 0)
            {
                State = GameState.Playing;
                Lives.RefundLife();
                AdsService.SetBannerVisible(false);
                LevelContinued?.Invoke(LevelNumber);
            }
            BoosterUsed?.Invoke(b);
            return true;
        }

        /// <summary>Sweep: every good of the most common colour on the belt goes back to the godown.</summary>
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

        /// <summary>Reveal: shows the colour of every hidden crate.</summary>
        void RevealAllCrates()
        {
            foreach (var col in _godown.Columns)
                foreach (var c in col) c.Reveal();
        }

        /// <summary>
        /// Continue after a jam (the UI pays for it with a rewarded ad or coins first):
        /// sends up to 6 goods back to the godown as crates and refunds the life.
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
            Lives.RefundLife();
            AdsService.SetBannerVisible(false);
            LevelContinued?.Invoke(LevelNumber);
        }

        // ---------------- Prototype HUD (OnGUI fallback) ----------------

        GUIStyle _label, _big, _button, _small, _hud;

        void OnGUI()
        {
            if (_belt == null || _cam == null) return;
            float u = Screen.height / 100f; // 1% of screen height
            EnsureStyles(u);

            if (DrawWorldLabels)
            {
                foreach (var s in _shops)
                {
                    string text = s.IsDone ? "Done" : s.IsLocked ? "Locked" : s.ShutterClosed ? "Closed" : $"{s.Remaining}";
                    if (!s.IsDone && !s.IsLocked && s.OrdersLeft > 1) text += $"\n+{s.OrdersLeft - 1}";
                    WorldLabel(s.transform.position + new Vector3(0, -0.05f, 0), text, _label, u);
                }
                foreach (var col in _godown.Columns)
                    foreach (var crate in col)
                        WorldLabel(crate.transform.position, crate.Hidden ? "?" : crate.Count.ToString(), _label, u);
                if (_gateQueue.Count > 0)
                    WorldLabel(_belt.PointAt(_belt.GateS) + new Vector3(0, -0.75f, 0), _gateQueue.Count.ToString(), _label, u);
            }

            if (!DrawPrototypeHud) return;

            GUI.Label(new Rect(u * 2, u * 2, Screen.width * 0.5f, u * 5), $"Level {LevelNumber}", _hud);
            var coinsStyle = new GUIStyle(_hud) { alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width * 0.5f, u * 2, Screen.width * 0.5f - u * 2, u * 5), $"Coins {Coins}", coinsStyle);
            GUI.Label(new Rect(u * 2, u * 6, Screen.width, u * 4),
                $"Free slots: {BeltFree}/{BeltCapacity}   Lives: {Lives.Count}{(RushActive ? "   RUSH HOUR!" : "")}", _small);
            BoosterBar(u);

            if (LevelNumber == 1 && State == GameState.Playing && _deliveries == 0 && _gateQueue.Count == 0)
                CenterText(u, "Tap a crate below!", Screen.height * 0.47f);
            else if (State == GameState.Playing && _level.intro != null && _deliveries < 3)
                CenterText(u, _level.intro, Screen.height * 0.47f);

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
            Panel(u, "Jammed!", "No shop wants the goods on the belt");
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
