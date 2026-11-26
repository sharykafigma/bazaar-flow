using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// A bazaar stall next to the belt. It has a line of orders; the front order says which colour it takes.
    /// It grabs a matching item when that item passes its pickup point on the belt.
    /// </summary>
    public class Shop : MonoBehaviour
    {
        public int Index { get; private set; }
        public float PathS { get; private set; }
        public bool IsDone { get; private set; }
        public bool IsLocked => _lockedUntil >= 0;
        public int LockedUntil => _lockedUntil;

        public OrderDef Current => _orders.Count > 0 ? _orders[0] : null;
        public int Remaining { get; private set; }
        public int OrdersLeft => _orders.Count;

        public event System.Action<Shop> Completed;

        readonly List<OrderDef> _orders = new List<OrderDef>();
        int _lockedUntil;
        float _cooldown;
        SpriteRenderer _body, _patch, _lockIcon;
        Vector3 _baseScale;

        public void Init(int index, ShopDef def, float pathS, Vector3 position)
        {
            Index = index;
            PathS = pathS;
            transform.position = position;
            _lockedUntil = def.lockedUntilShop;
            foreach (var o in def.orders) _orders.Add(new OrderDef(o.color, o.count));
            Remaining = Current?.count ?? 0;

            _body = SpriteFactory.Create("Body", transform, SpriteFactory.Rounded, GameColors.ShopBody,
                Vector3.zero, new Vector2(1.15f, 1.15f), 2);
            _patch = SpriteFactory.Create("Order", transform, SpriteFactory.Circle, Color.white,
                new Vector3(0, 0.1f, -0.01f), new Vector2(0.62f, 0.62f), 3);
            _lockIcon = SpriteFactory.Create("Lock", transform, SpriteFactory.Rounded, new Color(0.15f, 0.15f, 0.15f, 0.75f),
                new Vector3(0, 0, -0.02f), new Vector2(1.15f, 1.15f), 4);
            _baseScale = transform.localScale;
            Refresh();
        }

        /// <summary>Can this shop take this colour right now (ignores the short pause between orders)?</summary>
        public bool Needs(ItemColor c) => !IsDone && !IsLocked && Current != null && Current.color == c && Remaining > 0;

        public bool CanTakeNow(ItemColor c) => Needs(c) && _cooldown <= 0f;

        /// <summary>For the Belt Jam check: also counts the next order while the shop pauses between orders.</summary>
        public bool WillNeed(ItemColor c)
        {
            if (IsDone || IsLocked || Current == null) return false;
            if (Remaining > 0) return Current.color == c;
            return _orders.Count > 1 && _orders[1].color == c;
        }

        /// <summary>Reserve one unit now; the item is counted when it lands.</summary>
        public void Take(BeltItem item)
        {
            Remaining--;
            if (Remaining <= 0) _cooldown = 0.35f;
            item.FlyTo(transform.position, () => StartCoroutine(Punch()));
            if (Remaining <= 0) StartCoroutine(NextOrderAfter(0.35f));
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
        }

        IEnumerator NextOrderAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            _orders.RemoveAt(0);
            if (_orders.Count == 0)
            {
                IsDone = true;
                Refresh();
                Completed?.Invoke(this);
            }
            else
            {
                Remaining = Current.count;
                Refresh();
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
            if (IsDone)
            {
                _body.color = GameColors.ShopDone;
                _patch.enabled = false;
                return;
            }
            _patch.enabled = true;
            _patch.color = Current != null ? GameColors.Get(Current.color) : Color.white;
        }
    }
}
