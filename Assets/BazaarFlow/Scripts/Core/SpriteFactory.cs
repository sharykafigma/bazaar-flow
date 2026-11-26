using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Makes simple placeholder sprites at runtime so the prototype needs no art files.
    /// Every sprite is 1 world unit wide (64 px at 64 pixels per unit).
    /// </summary>
    public static class SpriteFactory
    {
        const int Size = 64;
        static Sprite _square, _circle, _rounded;

        // Explicit null checks: "??=" skips Unity's own null check on destroyed objects.
        public static Sprite Square { get { if (_square == null) _square = Make((x, y) => 1f); return _square; } }
        public static Sprite Circle { get { if (_circle == null) _circle = Make(CircleAlpha); return _circle; } }
        public static Sprite Rounded { get { if (_rounded == null) _rounded = Make(RoundedAlpha); return _rounded; } }

        static float CircleAlpha(float x, float y)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f));
            return Mathf.Clamp01((0.5f - d) * Size);
        }

        static float RoundedAlpha(float x, float y)
        {
            const float r = 0.18f;
            float dx = Mathf.Max(Mathf.Abs(x - 0.5f) - (0.5f - r), 0f);
            float dy = Mathf.Max(Mathf.Abs(y - 0.5f) - (0.5f - r), 0f);
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01((r - d) * Size);
        }

        static Sprite Make(System.Func<float, float, float> alpha)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var px = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float a = alpha((x + 0.5f) / Size, (y + 0.5f) / Size);
                px[y * Size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        /// <summary>Creates a child GameObject with a coloured sprite.</summary>
        public static SpriteRenderer Create(string name, Transform parent, Sprite sprite, Color color,
            Vector3 localPos, Vector2 scale, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
