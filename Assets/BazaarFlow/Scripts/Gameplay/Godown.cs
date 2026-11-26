using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>A crate of goods waiting in the godown.</summary>
    public class Crate : MonoBehaviour
    {
        public ItemColor Color { get; private set; }
        public int Count { get; private set; }
        public bool Hidden { get; private set; }

        SpriteRenderer _sr;
        Vector3 _target;

        public void Init(CrateDef def)
        {
            Color = def.color;
            Count = def.count;
            Hidden = def.hidden;
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = SpriteFactory.Rounded;
            _sr.sortingOrder = 2;
            transform.localScale = new Vector3(0.95f, 0.85f, 1f);
            Refresh();
        }

        public void Reveal()
        {
            if (!Hidden) return;
            Hidden = false;
            Refresh();
        }

        public void MoveTo(Vector3 pos, bool instant)
        {
            _target = pos;
            if (instant) transform.position = pos;
        }

        void Update()
        {
            transform.position = Vector3.Lerp(transform.position, _target, 1f - Mathf.Exp(-14f * Time.deltaTime));
        }

        void Refresh() => _sr.color = Hidden ? GameColors.Hidden : GameColors.Get(Color);

        public bool Contains(Vector2 world)
        {
            Vector2 half = transform.lossyScale * 0.5f;
            Vector2 d = world - (Vector2)transform.position;
            return Mathf.Abs(d.x) <= half.x && Mathf.Abs(d.y) <= half.y;
        }
    }

    /// <summary>
    /// The godown under the belt: a few columns of crates. Only the front crate of a column can be tapped.
    /// </summary>
    public class Godown : MonoBehaviour
    {
        public const float RowStep = 1.0f;
        public readonly List<List<Crate>> Columns = new List<List<Crate>>();
        readonly List<float> _columnX = new List<float>();
        float _topY;

        public void Init(LevelData level, Vector2 topCenter, float width)
        {
            _topY = topCenter.y;
            int n = level.columns.Count;
            float step = n > 1 ? Mathf.Min(1.25f, width / (n - 1)) : 0f;
            float startX = topCenter.x - step * (n - 1) * 0.5f;
            for (int c = 0; c < n; c++)
            {
                _columnX.Add(startX + c * step);
                var list = new List<Crate>();
                foreach (var def in level.columns[c].crates)
                    list.Add(MakeCrate(def));
                Columns.Add(list);
            }
            Layout(true);
        }

        Crate MakeCrate(CrateDef def)
        {
            var go = new GameObject("Crate_" + def.color);
            go.transform.SetParent(transform, false);
            var crate = go.AddComponent<Crate>();
            crate.Init(def);
            return crate;
        }

        public void Layout(bool instant)
        {
            for (int c = 0; c < Columns.Count; c++)
            {
                var col = Columns[c];
                for (int i = 0; i < col.Count; i++)
                {
                    col[i].MoveTo(new Vector3(_columnX[c], _topY - i * RowStep, 0), instant);
                    if (i == 0) col[i].Reveal();
                }
            }
        }

        /// <summary>The front crate under the finger, or null.</summary>
        public Crate FrontCrateAt(Vector2 world, out int column)
        {
            for (int c = 0; c < Columns.Count; c++)
            {
                if (Columns[c].Count > 0 && Columns[c][0].Contains(world))
                {
                    column = c;
                    return Columns[c][0];
                }
            }
            column = -1;
            return null;
        }

        public void RemoveFront(int column)
        {
            var crate = Columns[column][0];
            Columns[column].RemoveAt(0);
            Destroy(crate.gameObject);
            Layout(false);
        }

        /// <summary>Used by the "continue" reward: puts goods back as a new front crate in the shortest column.</summary>
        public void PushFront(CrateDef def)
        {
            int best = 0;
            for (int c = 1; c < Columns.Count; c++)
                if (Columns[c].Count < Columns[best].Count) best = c;
            var crate = MakeCrate(def);
            crate.MoveTo(new Vector3(_columnX[best], _topY + RowStep, 0), true);
            Columns[best].Insert(0, crate);
            Layout(false);
        }

        public bool IsEmpty
        {
            get
            {
                foreach (var col in Columns) if (col.Count > 0) return false;
                return true;
            }
        }
    }
}
