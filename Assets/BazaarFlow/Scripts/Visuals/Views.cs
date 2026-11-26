using UnityEngine;

namespace BazaarFlow.Visuals
{
    // Each view sits on a gameplay object, hides its placeholder sprite and draws the bazaar art as children.
    // Views only READ gameplay state, so gameplay scripts stay untouched.

    /// <summary>Belt look: tiled conveyor edges, corner caps with turning brass hubs, brass thaali slots.</summary>
    public class BeltView : MonoBehaviour
    {
        public const float Thick = 0.9f;
        Belt _belt;
        Transform[] _hubs;

        public void Init(Belt belt)
        {
            _belt = belt;
            var t = belt.transform;
            foreach (var n in new[] { "Top", "Bottom", "Left", "Right" }) BazaarArt.HidePlaceholder(t, n);

            float hw = belt.Width * 0.5f, hh = belt.Height * 0.5f;
            Strip("belt_h", new Vector3(0, hh), new Vector2(belt.Width, Thick));
            Strip("belt_h", new Vector3(0, -hh), new Vector2(belt.Width, Thick));
            Strip("belt_v", new Vector3(-hw, 0), new Vector2(Thick, belt.Height));
            Strip("belt_v", new Vector3(hw, 0), new Vector2(Thick, belt.Height));

            _hubs = new Transform[4];
            var corners = new[] { new Vector3(-hw, hh), new Vector3(hw, hh), new Vector3(hw, -hh), new Vector3(-hw, -hh) };
            for (int i = 0; i < 4; i++)
            {
                BazaarArt.Add(t, "Corner", BazaarArt.Get("belt_corner", Thick * 1.1f), 1, corners[i]);
                var hub = BazaarArt.Add(t, "Hub", BazaarArt.Get("belt_hub", Thick * 0.92f), 1, corners[i] + new Vector3(0, 0, -0.01f));
                if (hub != null) _hubs[i] = hub.transform;
            }

            // Slot markers become brass thaalis (gameplay moves them; we only change their look).
            var tray = BazaarArt.Get("tray", 1.12f);
            for (int i = 0; i < belt.SlotCount; i++)
            {
                var m = t.Find("Slot" + i);
                if (m == null || tray == null) continue;
                var sr = m.GetComponent<SpriteRenderer>();
                sr.sprite = tray;
                sr.color = Color.white;
            }
        }

        void Strip(string sprite, Vector3 pos, Vector2 size)
        {
            var s = BazaarArt.Get(sprite, Thick, new Vector2(0.5f, 0.5f), true);
            var sr = BazaarArt.Add(_belt.transform, "Strip", s, 0, pos);
            if (sr == null) return;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = size;
        }

        void LateUpdate()
        {
            if (_belt == null || _hubs == null) return;
            // Hubs turn with the belt (clockwise). Radius ~0.4 units.
            float paused = BazaarVisuals.BeltRunning ? 1f : 0f;
            float deg = -_belt.Speed / 0.4f * Mathf.Rad2Deg * Time.deltaTime * paused;
            foreach (var h in _hubs) if (h != null) h.Rotate(0, 0, deg);
        }
    }

    /// <summary>Goods on the belt: icon art, soft shadow, bob and wobble, sparkle trail while flying into a shop.</summary>
    public class ItemView : MonoBehaviour
    {
        BeltItem _item;
        Transform _art;
        SpriteRenderer _artSr, _shadow;
        float _phase, _trail;
        public Vector3 LastPos { get; private set; }
        public bool WasLeaving { get; private set; }
        public ItemColor GoodsColor { get; private set; }

        public void Init(BeltItem item)
        {
            _item = item;
            GoodsColor = item.Color;
            _phase = Random.value * 10f;
            var root = item.GetComponent<SpriteRenderer>();
            BazaarArt.HidePlaceholder(root);

            // Item root scale is animated by gameplay (0.1 -> 0.5 pop, 0.5 -> 0.25 fly), so sizes here are "at scale 1".
            _shadow = BazaarArt.Add(transform, "Shadow", BazaarArt.Get("fx_shadow", 1.2f), 2, new Vector3(0, -0.42f, 0.01f),
                new Color(1, 1, 1, 0.55f));
            _artSr = BazaarArt.Add(transform, "Art", BazaarArt.Get("item_" + BazaarArt.Goods(item.Color), 1.45f), 3, new Vector3(0, 0.08f, 0));
            if (_artSr != null) _art = _artSr.transform;
            LastPos = transform.position;
        }

        void LateUpdate()
        {
            if (_item == null) return;
            LastPos = transform.position;
            if (_art == null) return;
            float t = Time.time * 5.5f + _phase;
            if (_item.Leaving)
            {
                if (!WasLeaving)
                {
                    WasLeaving = true;
                    if (_shadow != null) _shadow.enabled = false;
                    _artSr.sortingOrder = 7; // fly over stalls
                }
                _art.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 30f) * 12f);
                _trail -= Time.deltaTime;
                if (_trail <= 0f)
                {
                    _trail = 0.04f;
                    Fx.Instance.Emit("fx_sparkle", transform.position + (Vector3)Random.insideUnitCircle * 0.12f, Vector3.zero,
                        Color.Lerp(Color.white, GameColors.Get(GoodsColor), 0.35f), 0.3f, 0.22f, 0.02f, 0f, 0f, 180f);
                }
                return;
            }
            float bob = BazaarVisuals.BeltRunning ? Mathf.Abs(Mathf.Sin(t)) * 0.07f : 0f;
            _art.localPosition = new Vector3(0, 0.08f + bob, 0);
            _art.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.5f) * 5f);
            if (_shadow != null) _shadow.transform.localScale = Vector3.one * (1f - bob * 1.5f);
        }
    }

    /// <summary>Crates: wooden crate with goods peeking out, sack cover for hidden crates, glow on tappable front crates.</summary>
    public class CrateView : MonoBehaviour
    {
        Crate _crate;
        SpriteRenderer _art, _glow;
        bool _hidden;
        float _revealT = 1f, _bornT;
        public bool IsFront;
        public Vector3 LastPos { get; private set; }
        public Sprite CurrentSprite => _art != null ? _art.sprite : null;
        public ItemColor GoodsColor => _crate != null ? _crate.Color : default;

        public void Init(Crate crate)
        {
            _crate = crate;
            BazaarArt.HidePlaceholder(crate.GetComponent<SpriteRenderer>());
            var ls = crate.transform.localScale;
            var inv = new Vector3(1f / Mathf.Max(0.01f, ls.x), 1f / Mathf.Max(0.01f, ls.y), 1f); // cancel the hitbox scale

            _glow = BazaarArt.Add(transform, "Glow", BazaarArt.Get("fx_glow", 1.9f), 1, new Vector3(0, 0.05f, 0.02f),
                new Color(1f, 0.93f, 0.55f, 0f));
            if (_glow != null) _glow.transform.localScale = inv;
            var go = new GameObject("Art");
            go.transform.SetParent(transform, false);
            go.transform.localScale = inv;
            _art = go.AddComponent<SpriteRenderer>();
            _art.sortingOrder = 2;
            _hidden = crate.Hidden;
            _art.sprite = SpriteFor(_hidden);
            if (_art.sprite == null) Destroy(go);
            LastPos = transform.position;
        }

        public static Sprite SpriteFor(bool hidden, ItemColor color) =>
            BazaarArt.Get(hidden ? "crate_hidden" : "crate_" + BazaarArt.Goods(color), 1.12f, new Vector2(0.5f, 0.4f));

        Sprite SpriteFor(bool hidden) => SpriteFor(hidden, _crate.Color);

        public void PlayDrop(float delay)
        {
            _bornT = -delay;
        }

        void LateUpdate()
        {
            if (_crate == null || _art == null) return;
            LastPos = transform.position;
            if (_hidden && !_crate.Hidden)
            {
                _hidden = false;
                _art.sprite = SpriteFor(false);
                _revealT = 0f;
                Fx.Puff(transform.position, 7, 1.6f, new Color(0.85f, 0.72f, 0.5f, 0.9f));
                Fx.Sparkles(transform.position, GameColors.Get(_crate.Color), 6, 2f);
            }

            float s = 1f;
            if (_revealT < 1f)
            {
                _revealT += Time.deltaTime / 0.35f;
                s = Ease.OutBack(_revealT, 3f);
            }
            if (_bornT < 1f)
            {
                _bornT += Time.deltaTime / 0.45f;
                s *= _bornT <= 0f ? 0f : Ease.OutBack(_bornT, 2.2f);
            }

            float pulse = IsFront && BazaarVisuals.Playing ? 0.5f + 0.5f * Mathf.Sin(Time.time * 4f) : 0f;
            if (IsFront) s *= 1f + pulse * 0.035f;
            _art.transform.localScale = InvScale() * s;
            _art.color = IsFront ? Color.white : new Color(0.84f, 0.8f, 0.78f);
            if (_glow != null)
            {
                var c = _glow.color;
                c.a = Mathf.MoveTowards(c.a, IsFront ? 0.35f + pulse * 0.35f : 0f, Time.deltaTime * 4f);
                _glow.color = c;
            }
        }

        Vector3 InvScale()
        {
            var ls = transform.localScale;
            return new Vector3(1f / Mathf.Max(0.01f, ls.x), 1f / Mathf.Max(0.01f, ls.y), 1f);
        }
    }

    /// <summary>Shops: a bazaar stall with an order bubble, shutter when done, chains and padlock when locked.</summary>
    public class ShopView : MonoBehaviour
    {
        public const float StallSize = 1.45f;
        static readonly Vector2 StallPivot = new Vector2(0.5f, 0.48f);

        Shop _shop;
        Transform _stall, _bubble;
        SpriteRenderer _bubbleSr, _closed, _lock;
        int _remaining, _ordersLeft;
        ItemColor? _color;
        bool _done, _locked;
        float _introT = 1f, _introDelay, _popT = 1f, _flipT = 1f, _closeT = 1f;
        SpriteRenderer _bubble2, _shutterSr;

        public void Init(Shop shop, int variant, float introDelay)
        {
            _shop = shop;
            var t = shop.transform;
            BazaarArt.HidePlaceholder(t, "Body");
            BazaarArt.HidePlaceholder(t, "Order");
            BazaarArt.HidePlaceholder(t, "Lock");
            BazaarArt.HidePlaceholder(t, "Order2");
            BazaarArt.HidePlaceholder(t, "Shutter");

            BazaarArt.Add(t, "Shadow", BazaarArt.Get("fx_shadow", 1.5f), 1, new Vector3(0, -0.66f, 0.02f), new Color(1, 1, 1, 0.6f));
            var stall = BazaarArt.Add(t, "Stall", BazaarArt.Get("stall_" + variant, StallSize, StallPivot), 2);
            if (stall != null) _stall = stall.transform;
            _bubbleSr = BazaarArt.Add(t, "Bubble", BazaarArt.Get("bubble_chai", 0.76f), 3, new Vector3(0, 0, -0.01f));
            if (_bubbleSr != null) _bubble = _bubbleSr.transform;
            _closed = BazaarArt.Add(t, "Closed", BazaarArt.Get("stall_closed", StallSize, StallPivot), 4, new Vector3(0, 0, -0.02f));
            _lock = BazaarArt.Add(t, "Chains", BazaarArt.Get("stall_lock", StallSize, StallPivot), 5, new Vector3(0, 0, -0.03f));
            // Twin orders: a smaller second bubble. Monsoon: the closed shutter, tinted rain-grey.
            _bubble2 = BazaarArt.Add(t, "Bubble2", BazaarArt.Get("bubble_chai", 0.5f), 4, new Vector3(0.36f, 0.3f, -0.015f));
            _shutterSr = BazaarArt.Add(t, "Monsoon", BazaarArt.Get("stall_closed", StallSize, StallPivot), 4, new Vector3(0, 0, -0.025f));
            if (_shutterSr != null) _shutterSr.enabled = false;

            _done = shop.IsDone;
            _locked = shop.IsLocked;
            _remaining = shop.Remaining;
            _ordersLeft = shop.OrdersLeft;
            _color = shop.Current?.color;
            _introDelay = introDelay;
            _introT = 0f;
            RefreshBubble();
            if (_closed != null) _closed.enabled = _done;
            if (_lock != null) _lock.enabled = _locked;
            ApplyIntro();
        }

        void RefreshBubble()
        {
            if (_bubbleSr == null) return;
            bool show = !_shop.IsDone && _shop.Current != null;
            _bubbleSr.enabled = show;
            if (show) _bubbleSr.sprite = BazaarArt.Get("bubble_" + BazaarArt.Goods(_shop.Current.color), 0.76f);
            if (_bubble2 != null)
            {
                bool twin = show && _shop.Current.IsTwin && _shop.RemainingSecond > 0;
                _bubble2.enabled = twin;
                if (twin) _bubble2.sprite = BazaarArt.Get("bubble_" + BazaarArt.Goods(_shop.Current.color2), 0.5f);
            }
            if (_shutterSr != null)
            {
                bool warn = _shop.HasShutters && !_shop.ShutterClosed && !_shop.IsDone && _shop.ShutterTimer < 0.8f;
                bool on = (_shop.ShutterClosed && !_shop.IsDone) || (warn && Mathf.Repeat(_shop.ShutterTimer, 0.25f) < 0.12f);
                _shutterSr.enabled = on;
                if (on) _shutterSr.color = new Color(0.7f, 0.78f, 0.9f, _shop.ShutterClosed ? 1f : 0.45f);
            }
        }

        void ApplyIntro()
        {
            if (_stall == null) return;
            float t = _introDelay > 0 ? 0f : _introT;
            float s = Ease.OutElastic(t);
            _stall.localScale = new Vector3(s, s, 1);
            if (_bubble != null && _popT >= 1f && _flipT >= 1f) _bubble.localScale = Vector3.one * Ease.OutBack(Mathf.Clamp01(t * 1.3f - 0.3f));
        }

        void LateUpdate()
        {
            if (_shop == null) return;
            float dt = Time.deltaTime;

            if (_introT < 1f)
            {
                if (_introDelay > 0) _introDelay -= dt;
                else _introT += dt / 0.7f;
                ApplyIntro();
            }

            // Delivery landed or reserved: Remaining goes down -> bubble pop.
            if (_shop.Remaining < _remaining && !_shop.IsDone)
            {
                _popT = 0f;
            }
            // New order: colour changes or order count drops -> bubble flips to the new goods.
            var col = _shop.Current?.color;
            if (_shop.OrdersLeft != _ordersLeft || col != _color)
            {
                if (!_shop.IsDone && _shop.OrdersLeft < _ordersLeft)
                {
                    _flipT = 0f;
                    Fx.Sparkles(transform.position, col.HasValue ? GameColors.Get(col.Value) : Color.white, 8, 2.2f);
                }
                _ordersLeft = _shop.OrdersLeft;
                _color = col;
            }
            _remaining = _shop.Remaining;

            if (_shop.IsDone && !_done)
            {
                _done = true;
                _closeT = 0f;
                if (_closed != null) _closed.enabled = true;
                Fx.PetalBurst(transform.position + new Vector3(0, 0.3f, 0), 16);
                Fx.Ring(transform.position, new Color(1f, 0.85f, 0.35f), 1.8f, 0.4f);
                Fx.Sparkles(transform.position, new Color(1f, 0.85f, 0.35f), 10, 3f);
            }
            if (_locked && !_shop.IsLocked)
            {
                _locked = false;
                if (_lock != null)
                {
                    _lock.enabled = false;
                    FallingGhost.Spawn(_lock, Vector3.zero);
                }
                Fx.Sparkles(transform.position, new Color(1f, 0.85f, 0.35f), 12, 3f);
                Fx.Ring(transform.position, Color.white, 1.6f);
                _popT = 0f;
            }

            RefreshBubble();
            if (_bubble != null && _introT >= 1f)
            {
                float s = 1f;
                if (_popT < 1f)
                {
                    _popT += dt / 0.25f;
                    s += Ease.Bump(_popT) * 0.22f;
                }
                float sx = s;
                if (_flipT < 1f)
                {
                    _flipT += dt / 0.3f;
                    sx *= Mathf.Abs(Mathf.Cos(_flipT * Mathf.PI)) * 0.9f + 0.1f;
                }
                // gentle idle breathing so the board feels alive
                float idle = 1f + Mathf.Sin(Time.time * 2.4f + transform.position.x) * 0.025f;
                _bubble.localScale = new Vector3(sx * idle, s * idle, 1);
            }

            if (_closed != null && _closeT < 1f)
            {
                _closeT += dt / 0.4f;
                float e = Ease.OutBack(_closeT, 2f);
                _closed.transform.localPosition = new Vector3(0, Mathf.Lerp(0.5f, 0f, e), -0.02f);
                var c = Color.white;
                c.a = Mathf.Clamp01(_closeT * 3f);
                _closed.color = c;
                if (_stall != null) _stall.localScale = Vector3.one * (1f + Ease.Bump(_closeT) * 0.08f);
            }
        }
    }

    /// <summary>Gate (godown arch): bumps whenever goods leave it.</summary>
    public class GateView : MonoBehaviour
    {
        float _t = 1f;
        Vector3 _base = Vector3.one;

        public void Bump() => _t = 0f;

        void Awake() => _base = transform.localScale;

        void LateUpdate()
        {
            if (_t >= 1f) return;
            _t += Time.deltaTime / 0.22f;
            float k = Ease.Bump(_t);
            transform.localScale = new Vector3(_base.x * (1f + k * 0.08f), _base.y * (1f - k * 0.06f), 1f);
        }
    }

    /// <summary>A copy of a sprite that falls and spins away (broken lock).</summary>
    public class FallingGhost : MonoBehaviour
    {
        SpriteRenderer _sr;
        Vector3 _vel;
        float _spin, _age;

        public static void Spawn(SpriteRenderer src, Vector3 vel)
        {
            var go = new GameObject("Ghost");
            go.transform.position = src.transform.position;
            go.transform.rotation = src.transform.rotation;
            go.transform.localScale = src.transform.lossyScale;
            var g = go.AddComponent<FallingGhost>();
            g._sr = go.AddComponent<SpriteRenderer>();
            g._sr.sprite = src.sprite;
            g._sr.sortingOrder = src.sortingOrder + 2;
            g._vel = vel + new Vector3(Random.Range(-1.2f, 1.2f), 2.5f, 0);
            g._spin = Random.Range(-240f, 240f);
        }

        void Update()
        {
            _age += Time.deltaTime;
            _vel.y -= 14f * Time.deltaTime;
            transform.position += _vel * Time.deltaTime;
            transform.Rotate(0, 0, _spin * Time.deltaTime);
            var c = _sr.color;
            c.a = 1f - Mathf.Clamp01((_age - 0.4f) / 0.4f);
            _sr.color = c;
            if (_age > 0.85f) Destroy(gameObject);
        }
    }

    /// <summary>A crate that was tapped: squashes, then flies into the gate and shrinks.</summary>
    public class CrateGhost : MonoBehaviour
    {
        SpriteRenderer _sr;
        Vector3 _from, _to;
        float _t;
        System.Action _onArrive;

        public static void Spawn(Sprite sprite, Vector3 from, Vector3 to, System.Action onArrive)
        {
            if (sprite == null) return;
            var go = new GameObject("CrateGhost");
            go.transform.position = from;
            var g = go.AddComponent<CrateGhost>();
            g._sr = go.AddComponent<SpriteRenderer>();
            g._sr.sprite = sprite;
            g._sr.sortingOrder = 8;
            g._from = from;
            g._to = to;
            g._onArrive = onArrive;
        }

        void Update()
        {
            _t += Time.deltaTime / 0.32f;
            if (_t < 0.25f)
            {
                float k = Ease.Bump(_t / 0.25f);
                transform.localScale = new Vector3(1f + k * 0.18f, 1f - k * 0.15f, 1f);
                return;
            }
            float e = Ease.OutCubic((_t - 0.25f) / 0.75f);
            var p = Vector3.Lerp(_from, _to, e);
            p.y += Ease.Bump(e) * 0.5f;
            transform.position = p;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.25f, e);
            if (_t >= 1f)
            {
                _onArrive?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
