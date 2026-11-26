using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Numbers drawn in the world (not on top of the UI): goods left on each stall's order, goods in each crate
    /// ("?" when hidden) and goods waiting at the gate. Uses the game font with a dark drop shadow.
    /// </summary>
    public class WorldLabels : MonoBehaviour
    {
        const int SortingOrder = 20;
        const int FontSize = 64;
        const float CharacterSize = 0.1f;
        /// <summary>Approximate line height of a TextMesh at this size, in world units.</summary>
        const float TextHeight = CharacterSize * FontSize / 10f;

        GameManager _game;
        Font _font;
        readonly Dictionary<Object, Label> _labels = new Dictionary<Object, Label>();
        readonly HashSet<Object> _seen = new HashSet<Object>();
        readonly List<Object> _gone = new List<Object>();
        Label _gate;

        class Label
        {
            public Transform Root;
            public TextMesh Text, Shadow;
            public string Value;

            /// <summary>height = digit height in world units.</summary>
            public void Set(string value, Vector3 pos, float height)
            {
                Root.position = pos;
                float k = height / TextHeight;
                Root.localScale = new Vector3(k, k, 1f);
                if (Value == value) return;
                Value = value;
                Text.text = value;
                Shadow.text = value;
            }

            public void SetActive(bool on)
            {
                if (Root.gameObject.activeSelf != on) Root.gameObject.SetActive(on);
            }
        }

        void Awake()
        {
            _game = GetComponent<GameManager>();
            _font = Resources.Load<Font>("Fonts/BazaarFont");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        void LateUpdate()
        {
            if (_game == null || _game.Belt == null) return;
            _seen.Clear();

            foreach (var shop in _game.Shops)
            {
                if (shop == null) continue;
                string text;
                if (shop.IsDone) text = "";
                else if (shop.IsLocked) text = "";
                else
                {
                    text = shop.Remaining.ToString();
                    if (shop.OrdersLeft > 1) text += "  +" + (shop.OrdersLeft - 1);
                }
                Show(shop, text, shop.transform.position + new Vector3(0f, -0.72f, -0.5f), 0.42f);
            }

            foreach (var col in _game.Godown.Columns)
                foreach (var crate in col)
                    if (crate != null)
                        Show(crate, crate.Hidden ? "?" : crate.Count.ToString(), crate.transform.position + new Vector3(0f, 0f, -0.5f), 0.45f);

            if (_gate == null) _gate = Make("Gate");
            int q = _game.GateQueue;
            _gate.SetActive(q > 0);
            if (q > 0) _gate.Set(q.ToString(), _game.Belt.PointAt(_game.Belt.GateS) + new Vector3(0f, -0.75f, -0.5f), 0.38f);

            // Remove labels whose object is gone (level reloaded, crate tapped).
            _gone.Clear();
            foreach (var kv in _labels) if (kv.Key == null || !_seen.Contains(kv.Key)) _gone.Add(kv.Key);
            foreach (var k in _gone)
            {
                if (_labels[k].Root != null) Destroy(_labels[k].Root.gameObject);
                _labels.Remove(k);
            }
        }

        void Show(Object owner, string text, Vector3 pos, float size)
        {
            _seen.Add(owner);
            if (!_labels.TryGetValue(owner, out var label))
            {
                label = Make("Label");
                _labels[owner] = label;
            }
            label.SetActive(!string.IsNullOrEmpty(text));
            label.Set(text, pos, size);
        }

        Label Make(string name)
        {
            var root = new GameObject(name).transform;
            root.SetParent(transform, false);
            var shadow = MakeText(root, new Vector3(0.045f, -0.06f, 0.01f), new Color(0.23f, 0.14f, 0.09f, 0.95f), SortingOrder);
            var text = MakeText(root, Vector3.zero, Color.white, SortingOrder + 1);
            return new Label { Root = root, Text = text, Shadow = shadow };
        }

        TextMesh MakeText(Transform parent, Vector3 offset, Color color, int order)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = offset;
            var tm = go.AddComponent<TextMesh>();
            tm.font = _font;
            tm.fontSize = FontSize;
            tm.characterSize = CharacterSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _font.material;
            mr.sortingOrder = order;
            return tm;
        }
    }
}
