using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow.Visuals
{
    /// <summary>
    /// Loads the bazaar sprites from Resources/BazaarArt (made by Tools/ArtGen) and builds Sprites
    /// with an exact world size, so code never depends on texture import settings.
    /// Returns null when a texture is missing; callers then keep the placeholder look.
    /// </summary>
    public static class BazaarArt
    {
        const string Folder = "BazaarArt/";
        static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        static readonly string[] GoodsKeys = { "chai", "choori", "aam", "patang", "lattoo", "diya" };

        /// <summary>chai, choori, aam, patang, lattoo, diya: one goods type per colour (shape helps colour-blind players).</summary>
        public static string Goods(ItemColor c) => GoodsKeys[(int)c];

        public static bool Ready => Texture("tray") != null;

        public static Texture2D Texture(string name)
        {
            if (Textures.TryGetValue(name, out var t) && t != null) return t;
            t = Resources.Load<Texture2D>(Folder + name);
            if (t != null)
            {
                t.wrapMode = TextureWrapMode.Clamp;
                t.filterMode = t.mipmapCount > 1 ? FilterMode.Trilinear : FilterMode.Bilinear;
            }
            Textures[name] = t;
            return t;
        }

        /// <summary>A sprite that is worldWidth units wide. Pivot is 0..1 from the bottom-left.</summary>
        public static Sprite Get(string name, float worldWidth, Vector2 pivot, bool fullRect = false)
        {
            string key = $"{name}|{worldWidth:0.###}|{pivot.x:0.###},{pivot.y:0.###}|{fullRect}";
            if (Sprites.TryGetValue(key, out var s) && s != null) return s;
            var tex = Texture(name);
            if (tex == null) return null;
            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, tex.width / worldWidth, 0,
                fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight);
            s.name = name;
            Sprites[key] = s;
            return s;
        }

        public static Sprite Get(string name, float worldWidth) => Get(name, worldWidth, new Vector2(0.5f, 0.5f));

        /// <summary>Adds a child sprite renderer. Returns null (and adds nothing) if the art is missing.</summary>
        public static SpriteRenderer Add(Transform parent, string name, Sprite sprite, int order,
            Vector3 localPos = default, Color? color = null)
        {
            if (sprite == null) return null;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.color = color ?? Color.white;
            return sr;
        }

        /// <summary>Hides a placeholder renderer without touching the state gameplay code drives (enabled / color).</summary>
        public static void HidePlaceholder(SpriteRenderer sr)
        {
            if (sr != null) sr.sprite = null;
        }

        public static void HidePlaceholder(Transform parent, string childName)
        {
            var t = parent.Find(childName);
            if (t != null) HidePlaceholder(t.GetComponent<SpriteRenderer>());
        }
    }

    /// <summary>Small easing helpers for the juice code.</summary>
    public static class Ease
    {
        public static float OutBack(float t, float s = 1.70158f)
        {
            t = Mathf.Clamp01(t) - 1f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }

        public static float OutCubic(float t)
        {
            t = 1f - Mathf.Clamp01(t);
            return 1f - t * t * t;
        }

        public static float OutElastic(float t)
        {
            t = Mathf.Clamp01(t);
            if (t <= 0f || t >= 1f) return t;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI / 3f)) + 1f;
        }

        /// <summary>0 -> 1 -> 0 bump.</summary>
        public static float Bump(float t) => Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
    }
}
