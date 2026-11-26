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
        public float Spacing => Length / SlotCount;

        /// <summary>Distance on the belt where the godown loads goods (bottom middle).</summary>
        public float GateS => Width + Height + Width * 0.5f;

        public BeltItem[] Slots { get; private set; }

        float _offset;
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

        /// <summary>Moves the belt. Returns how far it moved this frame.</summary>
        public float Advance(float dt)
        {
            float moved = Speed * dt;
            _offset = Mod(_offset + moved, Length);
            return moved;
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
            float d = Mod(target - prevS, Length);
            return d < moved;
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
