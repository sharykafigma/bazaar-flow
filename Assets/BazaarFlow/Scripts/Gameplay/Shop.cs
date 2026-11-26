using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// A bazaar stall next to the belt. It has a line of orders; the front order says which colour(s) it takes.
    /// It grabs a matching item when that item passes its pickup point on the belt.
    /// Twists: locked (opens when another shop finishes), twin orders (two colours at once), shutters (closes for a
    /// few seconds now and then).
    /// </summary>
    public class Shop : MonoBehaviour
    {
        public const float ShutterOpenSeconds = 9f;
        public const float ShutterClosedSeconds = 3.5f;

        public int Index { get; private set; }
        public float PathS { get; private set; }
        public bool IsDone { get; private set; }
        public bool IsLocked => _lockedUntil >= 0;
        public int LockedUntil => _lockedUntil;

        /// <summary>The order being filled now (for a twin order, Current.color is the first colour).</summary>
        public OrderDef Current => _orders.Count > 0 ? _orders[0] : null;

        /// <summary>Goods still needed for the current order (both colours together for a twin order).</summary>
        public int Remaining => _remaining1 + _remaining2;
        public int RemainingFirst => _remaining1;
        public int RemainingSecond => _remaining2;
        public int OrdersLeft => _orders.Count;

        public bool HasShutters { get; private set; }
        /// <summary>True while the shutter is down: the shop takes nothing.</summary>
        public bool ShutterClosed { get; private set; }
        /// <summary>Seconds until the shutter changes (for a warning blink).</summary>
        public float ShutterTimer => _shutterTimer;

        public event System.Action<Shop> Completed;
        /// <summary>Fires when a good lands in the shop.</summary>
        public event System.Action<Shop, ItemColor> Delivered;
        /// <summary>Fires when the shop moves to its next order.</summary>
        public event System.Action<Shop> OrderChanged;

        readonly List<OrderDef> _orders = new List<OrderDef>();
        int _lockedUntil;
        int _remaining1, _remaining2;
        float _cooldown;
        float _shutterTimer;
        SpriteRenderer _body, _patch, _patch2, _lockIcon, _shutter;
        Vector3 _baseScale;

        public void Init(int index, ShopDef def, float pathS, Vector3 position)
        {
            Index = index;
            PathS = pathS;
            transform.position = position;
            _lockedUntil = def.lockedUntilShop;
            HasShutters = def.shutters;
            // Stagger shutters so shops don't all close together.
            _shutterTimer = ShutterOpenSeconds * (0.45f + 0.18f * index);
            foreach (var o in def.orders) _orders.Add(new OrderDef(o.color, o.count, o.color2, o.count2));
            LoadCurrent();

            _body = SpriteFactory.Create("Body", transform, SpriteFactory.Rounded, GameColors.ShopBody,
                Vector3.zero, new Vector2(1.15f, 1.15f), 2);
            _patch = SpriteFactory.Create("Order", transform, SpriteFactory.Circle, Color.white,
                new Vector3(0, 0.1f, -0.01f), new Vector2(0.62f, 0.62f), 3);
            _patch2 = SpriteFactory.Create("Order2", transform, SpriteFactory.Circle, Color.white,
                new Vector3(0.22f, 0.1f, -0.01f), new Vector2(0.42f, 0.42f), 3);
            _shutter = SpriteFactory.Create("Shutter", transform, SpriteFactory.Rounded, new Color(0.35f, 0.38f, 0.45f, 0.9f),
                new Vector3(0, 0, -0.02f), new Vector2(1.15f, 1.15f), 4);
            _lockIcon = SpriteFactory.Create("Lock", transform, SpriteFactory.Rounded, new Color(0.15f, 0.15f, 0.15f, 0.75f),
                new Vector3(0, 0, -0.03f), new Vector2(1.15f, 1.15f), 5);
            _baseScale = transform.localScale;
            Refresh();
        }

        void LoadCurrent()
        {
            var o = Current;
            _remaining1 = o?.count ?? 0;
            _remaining2 = o != null && o.IsTwin ? o.count2 : 0;
        }

        bool Wants(ItemColor c) =>
            (_remaining1 > 0 && Current.color == c) || (_remaining2 > 0 && Current.color2 == c);

        /// <summary>Can this shop take this colour (ignores the short pause between orders and the shutter)?</summary>
        public bool Needs(ItemColor c) => !IsDone && !IsLocked && Current != null && Wants(c);

        public bool CanTakeNow(ItemColor c) => Needs(c) && _cooldown <= 0f && !ShutterClosed;

        /// <summary>For the jam check: also counts the next order while the shop pauses, and closed shutters.</summary>
        public bool WillNeed(ItemColor c)
        {
            if (IsDone || IsLocked || Current == null) return false;
            if (Remaining > 0) return Wants(c);
            if (_orders.Count < 2) return false;
            var next = _orders[1];
            return next.color == c || (next.IsTwin && next.color2 == c);
        }

        /// <summary>Reserve one unit now; the item is counted as delivered when it lands.</summary>
        public void Take(BeltItem item)
        {
            var color = item.Color;
            if (_remaining1 > 0 && Current.color == color) _remaining1--;
            else _remaining2--;

            bool orderDone = Remaining <= 0;
            if (orderDone) _cooldown = 0.35f;
            item.FlyTo(transform.position, () =>
            {
                StartCoroutine(Punch());
                Delivered?.Invoke(this, color);
            });
            if (orderDone) StartCoroutine(NextOrderAfter(0.35f));
            Refresh();
        }

        public void Unlock()
        {
            _lockedUntil = -1;
            Refresh();
        }

        void Update()
        {
            if (_cooldown > 0f) _cooldown -= Time.deltaTime;
            if (HasShutters && !IsDone && !IsLocked)
            {
                _shutterTimer -= Time.deltaTime;
                if (_shutterTimer <= 0f)
                {
                    ShutterClosed = !ShutterClosed;
                    _shutterTimer = ShutterClosed ? ShutterClosedSeconds : ShutterOpenSeconds;
                    Refresh();
                }
            }
            else if (ShutterClosed)
            {
                ShutterClosed = false;
                Refresh();
            }
            // Blink the shutter half a second before it drops.
            if (_shutter != null && HasShutters && !ShutterClosed && !IsDone)
                _shutter.enabled = _shutterTimer < 0.8f && Mathf.Repeat(_shutterTimer, 0.25f) < 0.12f;
        }

        IEnumerator NextOrderAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            _orders.RemoveAt(0);
            if (_orders.Count == 0)
            {
                IsDone = true;
                ShutterClosed = false;
                Refresh();
                Completed?.Invoke(this);
            }
            else
            {
                LoadCurrent();
                Refresh();
                OrderChanged?.Invoke(this);
            }
        }

        IEnumerator Punch()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.18f;
                float k = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f;
                transform.localScale = _baseScale * k;
                yield return null;
            }
            transform.localScale = _baseScale;
        }

        void Refresh()
        {
            _lockIcon.enabled = IsLocked;
            _shutter.enabled = ShutterClosed;
            if (IsDone)
            {
                _body.color = GameColors.ShopDone;
                _patch.enabled = false;
                _patch2.enabled = false;
                return;
            }
            var o = Current;
            bool twin = o != null && o.IsTwin;
            _patch.enabled = true;
            _patch.color = o != null ? GameColors.Get(o.color) : Color.white;
            _patch.transform.localPosition = new Vector3(twin ? -0.2f : 0f, 0.1f, -0.01f);
            _patch.transform.localScale = twin ? new Vector3(0.46f, 0.46f, 1f) : new Vector3(0.62f, 0.62f, 1f);
            _patch2.enabled = twin;
            if (twin) _patch2.color = GameColors.Get(o.color2);
        }
    }
}
