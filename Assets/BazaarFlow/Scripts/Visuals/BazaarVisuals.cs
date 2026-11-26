using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow.Visuals
{
    /// <summary>
    /// Dresses the running game in the bazaar art and adds juice, without any code in gameplay scripts.
    /// It starts itself on Play, watches the board (GameManager, Belt, Shop, Godown, BeltItem public state),
    /// puts a view component on each gameplay object and fires effects when state changes.
    /// If the art folder is missing, it does nothing and the placeholder look stays.
    /// </summary>
    public class BazaarVisuals : MonoBehaviour
    {
        /// <summary>True while the belt is moving (not jammed). Views use it for bobbing / turning.</summary>
        public static bool BeltRunning { get; private set; } = true;
        /// <summary>True while the player can tap.</summary>
        public static bool Playing { get; private set; }

        public static BazaarVisuals Instance { get; private set; }

        static readonly Color SkyTop = new Color(1f, 0.81f, 0.62f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Instance != null) return;
            if (!BazaarArt.Ready)
            {
                Debug.LogWarning("Belt Jam: art not found in Resources/BazaarArt, keeping placeholder look.");
                return;
            }
            var go = new GameObject("BazaarVisuals");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<BazaarVisuals>();
        }

        struct ItemRec
        {
            public BeltItem item;
            public Vector3 pos;
            public bool leaving;
            public ItemColor color;
        }

        struct CrateRec
        {
            public Crate crate;
            public Vector3 pos;
            public Sprite sprite;
        }

        GameManager _game;
        Camera _cam;
        Belt _belt;
        Transform _root;
        Godown _godown;
        GateView _gate;
        Vector3 _gatePos;
        GameState _state;
        int _levelsDressed;

        readonly List<ItemRec> _items = new List<ItemRec>();
        readonly List<CrateRec> _crates = new List<CrateRec>();
        readonly HashSet<int> _knownItems = new HashSet<int>();

        SceneDecor _decor;
        Vector3 _camBase;
        bool _camBaseSet;
        float _shake, _zoomT = 1f, _flashT = 1f, _flashA;
        SpriteRenderer _flash;

        void LateUpdate()
        {
            if (_game == null)
            {
                _game = FindFirstObjectByType<GameManager>();
                if (_game == null) return;
            }
            if (_cam == null) _cam = Camera.main;
            if (_decor == null)
            {
                _decor = new GameObject("Decor").AddComponent<SceneDecor>();
                _decor.transform.SetParent(transform, false);
                _decor.Build();
                if (_cam != null) _cam.backgroundColor = SkyTop;
            }

            // A new level builds a new Belt: dress it the same frame so the placeholder never shows.
            foreach (var b in FindObjectsByType<Belt>(FindObjectsSortMode.None))
                if (b != null && b.Slots != null && b.GetComponent<BeltView>() == null)
                    DressLevel(b);

            if (_root == null) return;

            Playing = _game.State == GameState.Playing;
            BeltRunning = _game.State != GameState.Jammed;

            TrackItems();
            TrackCrates();
            TrackState();
            CameraJuice();
        }

        // ---------------------------------------------------------------- level

        void DressLevel(Belt belt)
        {
            _belt = belt;
            _root = belt.transform.parent;
            _items.Clear();
            _crates.Clear();
            _knownItems.Clear();
            _levelsDressed++;

            belt.gameObject.AddComponent<BeltView>().Init(belt);
            _decor.Place(belt);

            // Gate: the placeholder is scaled (0.9 x 0.7), so the arch goes next to it, not under it.
            var gate = _root.Find("Gate");
            _gatePos = belt.PointAt(belt.GateS) + new Vector3(0, -0.75f, 0);
            if (gate != null)
            {
                BazaarArt.HidePlaceholder(gate.GetComponent<SpriteRenderer>());
                _gatePos = gate.position;
            }
            var arch = BazaarArt.Add(_root, "GateArch", BazaarArt.Get("gate", 1.4f, new Vector2(0.5f, 0.42f)), 1,
                new Vector3(_gatePos.x, _gatePos.y, 0.04f));
            _gate = arch != null ? arch.gameObject.AddComponent<GateView>() : null;

            int level = _game.LevelNumber;
            foreach (var shop in _root.GetComponentsInChildren<Shop>())
            {
                int variant = (shop.Index + level) % 6;
                shop.gameObject.AddComponent<ShopView>().Init(shop, variant, 0.15f + shop.Index * 0.08f);
            }

            _godown = _root.GetComponentInChildren<Godown>();
            if (_godown != null)
            {
                for (int c = 0; c < _godown.Columns.Count; c++)
                {
                    var col = _godown.Columns[c];
                    for (int i = 0; i < col.Count; i++)
                    {
                        var v = AddCrateView(col[i]);
                        if (v != null) v.PlayDrop(0.25f + c * 0.06f + i * 0.05f);
                    }
                }
            }

            _state = _game.State;
            _shake = 0f;
            _zoomT = 0f; // small "board settles" zoom on every level start
        }

        // ---------------------------------------------------------------- goods

        void TrackItems()
        {
            // Old items: delivered (flew into a shop) or removed (continue after Belt Jam).
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var r = _items[i];
                if (r.item != null)
                {
                    r.pos = r.item.transform.position;
                    r.leaving = r.item.Leaving;
                    _items[i] = r;
                    continue;
                }
                var tint = GameColors.Get(r.color);
                if (r.leaving)
                {
                    Fx.Sparkles(r.pos, tint, 9, 2.8f);
                    Fx.Ring(r.pos, Color.Lerp(tint, Color.white, 0.3f), 1.1f);
                    Fx.CoinPop(r.pos + new Vector3(0, 0.2f, 0));
                }
                else
                {
                    Fx.Puff(r.pos, 5, 1.2f);
                }
                _items.RemoveAt(i);
            }

            // New items appear at the gate.
            for (int i = 0; i < _root.childCount; i++)
            {
                var child = _root.GetChild(i);
                var item = child.GetComponent<BeltItem>();
                if (item == null || !_knownItems.Add(item.GetInstanceID())) continue;
                child.gameObject.AddComponent<ItemView>().Init(item);
                _items.Add(new ItemRec { item = item, pos = child.position, color = item.Color });
                if (_gate != null) _gate.Bump();
                Fx.Puff(child.position, 3, 0.9f);
            }
        }

        // ---------------------------------------------------------------- crates

        CrateView AddCrateView(Crate crate)
        {
            var v = crate.GetComponent<CrateView>();
            if (v != null) return v;
            v = crate.gameObject.AddComponent<CrateView>();
            v.Init(crate);
            _crates.Add(new CrateRec { crate = crate, pos = crate.transform.position, sprite = v.CurrentSprite });
            return v;
        }

        void TrackCrates()
        {
            if (_godown == null) return;
            for (int c = 0; c < _godown.Columns.Count; c++)
            {
                var col = _godown.Columns[c];
                for (int i = 0; i < col.Count; i++)
                {
                    var crate = col[i];
                    if (crate == null) continue;
                    var v = crate.GetComponent<CrateView>();
                    if (v == null) v = AddCrateView(crate);
                    v.IsFront = i == 0;
                }
            }

            for (int i = _crates.Count - 1; i >= 0; i--)
            {
                var r = _crates[i];
                if (r.crate != null)
                {
                    r.pos = r.crate.transform.position;
                    var v = r.crate.GetComponent<CrateView>();
                    if (v != null && v.CurrentSprite != null) r.sprite = v.CurrentSprite;
                    _crates[i] = r;
                    continue;
                }
                // Tapped: the crate squashes and flies into the gate.
                var gate = _gate;
                var gatePos = _gatePos;
                CrateGhost.Spawn(r.sprite, r.pos, gatePos, () =>
                {
                    Fx.Puff(gatePos, 6, 1.4f);
                    if (gate != null) gate.Bump();
                });
                _crates.RemoveAt(i);
            }
        }

        // ---------------------------------------------------------------- win / jam

        void TrackState()
        {
            var s = _game.State;
            if (s == _state) return;
            if (s == GameState.Won)
            {
                Fx.Celebrate(_cam);
                _zoomT = 0f;
            }
            else if (s == GameState.Jammed)
            {
                _shake = 0.45f;
                Flash(new Color(0.9f, 0.15f, 0.1f, 0.28f));
            }
            else if (_state == GameState.Jammed && s == GameState.Playing)
            {
                Flash(new Color(1f, 0.95f, 0.7f, 0.25f));
                if (_belt != null) Fx.Sparkles(_belt.transform.position, Color.white, 14, 4f);
            }
            _state = s;
        }

        void Flash(Color c)
        {
            if (_cam == null) return;
            if (_flash == null)
            {
                _flash = BazaarArt.Add(transform, "Flash", SpriteFactory.Square, 200);
                if (_flash == null) return;
            }
            _flash.color = c;
            _flashA = c.a;
            _flashT = 0f;
        }

        void CameraJuice()
        {
            if (_cam == null) return;
            if (!_camBaseSet || _shake <= 0f && _cam.transform.position != _camBase)
            {
                _camBase = _cam.transform.position;
                _camBaseSet = true;
            }

            if (_shake > 0f)
            {
                _shake -= Time.deltaTime;
                float k = Mathf.Clamp01(_shake / 0.45f);
                _cam.transform.position = _camBase + (Vector3)(Random.insideUnitCircle * 0.18f * k);
                if (_shake <= 0f) _cam.transform.position = _camBase;
            }

            // CameraFit resets the size every Update, so a multiplier here is a temporary punch.
            if (_zoomT < 1f)
            {
                _zoomT += Time.deltaTime / 0.5f;
                _cam.orthographicSize *= 1f - Ease.Bump(_zoomT) * 0.035f;
            }

            if (_flash != null)
            {
                if (_flashT < 1f)
                {
                    _flashT += Time.deltaTime / 0.45f;
                    _flash.enabled = true;
                    float h = _cam.orthographicSize * 2.2f;
                    _flash.transform.position = new Vector3(_camBase.x, _camBase.y, 0f);
                    _flash.transform.localScale = new Vector3(h * _cam.aspect + 1f, h, 1f);
                    var c = _flash.color;
                    c.a = _flashA * (1f - Ease.OutCubic(_flashT));
                    _flash.color = c;
                }
                else _flash.enabled = false;
            }
        }
    }
}
