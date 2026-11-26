using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// The closed loop belt (patri). It is a rectangle walked clockwise from the top-left corner.
    /// A position on the belt is a distance "s" from 0 to Length. Slots are evenly spaced and all move together.
    /// </summary>
    public class Belt : MonoBehaviour
    {
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Length { get; private set; }
        public int SlotCount { get; private set; }
        public float Speed { get; private set; }

        /// <summary>Speed multiplier (rush hour). 1 = normal.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>True while the Reverse booster runs the belt backwards.</summary>
        public bool Reversed => _reverseLeft > 0f;
        public float Spacing => Length / SlotCount;

        /// <summary>Distance on the belt where the godown loads goods (bottom middle).</summary>
        public float GateS => Width + Height + Width * 0.5f;

        public BeltItem[] Slots { get; private set; }

        float _offset;
        float _reverseLeft;
        Transform[] _slotMarkers;

        public void Init(Vector2 center, float width, float height, int slotCount, float speed)
        {
            transform.position = center;
            Width = width;
            Height = height;
            Length = 2f * (width + height);
            SlotCount = slotCount;
            Speed = speed;
            Slots = new BeltItem[slotCount];
            _offset = 0f;

            const float thick = 0.9f;
            float hw = width * 0.5f, hh = height * 0.5f;
            var dark = GameColors.BeltDark;
            SpriteFactory.Create("Top", transform, SpriteFactory.Square, dark, new Vector3(0, hh), new Vector2(width + thick, thick), 0);
            SpriteFactory.Create("Bottom", transform, SpriteFactory.Square, dark, new Vector3(0, -hh), new Vector2(width + thick, thick), 0);
            SpriteFactory.Create("Left", transform, SpriteFactory.Square, dark, new Vector3(-hw, 0), new Vector2(thick, height + thick), 0);
            SpriteFactory.Create("Right", transform, SpriteFactory.Square, dark, new Vector3(hw, 0), new Vector2(thick, height + thick), 0);

            _slotMarkers = new Transform[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                var m = SpriteFactory.Create("Slot" + i, transform, SpriteFactory.Rounded, GameColors.BeltSlot,
                    Vector3.zero, new Vector2(0.62f, 0.62f), 1);
                _slotMarkers[i] = m.transform;
            }
            UpdateVisuals();
        }

        public float SlotS(int i) => Mod(i * Spacing + _offset, Length);

        /// <summary>Moves the belt. Returns how far it moved this frame (negative while reversed).</summary>
        public float Advance(float dt)
        {
            float moved = Speed * SpeedMultiplier * dt;
            if (_reverseLeft > 0f)
            {
                _reverseLeft -= moved;
                moved = -moved;
            }
            _offset = Mod(_offset + moved, Length);
            return moved;
        }

        /// <summary>Reverse booster: the belt runs backwards for one full lap.</summary>
        public void ReverseOneLap() => _reverseLeft = Length;

        /// <summary>
        /// Extra Belt booster: adds slots. Goods keep their place on the loop as closely as possible.
        /// </summary>
        public void AddSlots(int extra)
        {
            if (extra <= 0) return;
            var oldItems = new List<(float s, BeltItem item)>();
            for (int i = 0; i < SlotCount; i++)
                if (Slots[i] != null) oldItems.Add((SlotS(i), Slots[i]));

            int newCount = SlotCount + extra;
            var newMarkers = new Transform[newCount];
            for (int i = 0; i < newCount; i++)
            {
                if (i < _slotMarkers.Length) newMarkers[i] = _slotMarkers[i];
                else
                {
                    // Copy the look of an existing marker so art that re-skinned the slots still matches.
                    var like = _slotMarkers[0].GetComponent<SpriteRenderer>();
                    var sr = SpriteFactory.Create("Slot" + i, transform, like.sprite, like.color,
                        Vector3.zero, _slotMarkers[0].localScale, like.sortingOrder);
                    newMarkers[i] = sr.transform;
                }
            }
            _slotMarkers = newMarkers;
            SlotCount = newCount;
            Slots = new BeltItem[newCount];

            foreach (var entry in oldItems)
            {
                var item = entry.item;
                int best = Mathf.RoundToInt(Mod(entry.s - _offset, Length) / Spacing) % newCount;
                for (int k = 0; k < newCount; k++)
                {
                    int idx = (best + k) % newCount;
                    if (Slots[idx] == null) { Slots[idx] = item; break; }
                }
            }
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                Vector3 p = PointAt(SlotS(i));
                _slotMarkers[i].position = p;
                if (Slots[i] != null && !Slots[i].Leaving)
                    Slots[i].transform.position = p + new Vector3(0, 0, -0.1f);
            }
        }

        /// <summary>World position of distance s on the loop.</summary>
        public Vector3 PointAt(float s)
        {
            s = Mod(s, Length);
            float hw = Width * 0.5f, hh = Height * 0.5f;
            Vector3 c = transform.position;
            if (s < Width) return c + new Vector3(-hw + s, hh);                         // top, going right
            s -= Width;
            if (s < Height) return c + new Vector3(hw, hh - s);                         // right, going down
            s -= Height;
            if (s < Width) return c + new Vector3(hw - s, -hh);                         // bottom, going left
            s -= Width;
            return c + new Vector3(-hw, -hh + s);                                       // left, going up
        }

        /// <summary>Unit vector from the belt at distance s toward the inside of the loop.</summary>
        public Vector3 InwardAt(float s)
        {
            s = Mod(s, Length);
            if (s < Width) return Vector3.down;
            if (s < Width + Height) return Vector3.left;
            if (s < 2 * Width + Height) return Vector3.up;
            return Vector3.right;
        }

        /// <summary>True when a point that was at prevS and then moved forward by "moved" passed over target.</summary>
        public bool Crossed(float prevS, float moved, float target)
        {
            if (moved >= 0f) return Mod(target - prevS, Length) < moved;
            return Mod(prevS - target, Length) < -moved;
        }

        public int CountFree()
        {
            int n = 0;
            foreach (var s in Slots) if (s == null) n++;
            return n;
        }

        static float Mod(float a, float m) => ((a % m) + m) % m;
    }
}
