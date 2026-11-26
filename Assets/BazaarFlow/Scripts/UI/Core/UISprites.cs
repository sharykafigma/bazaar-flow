using System;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow.UI
{
    public enum UIIcon
    {
        Play, Pause, Close, Gear, Lock, Star, Home, Retry, Check, Bag, Speaker, Note, Vibrate,
        Video, Plus, Back, Pin, Pennant, Coin, Gift, Map, Shield
    }

    /// <summary>
    /// Builds every UI sprite at runtime from signed distance shapes, so the UI looks clean at any size
    /// and needs no image files. The art thread can drop real sprites in Resources/UI/&lt;name&gt; to override any of them.
    /// </summary>
    public static class UISprites
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Rounded rectangle for sliced Images (corner radius 40 px).</summary>
        public static Sprite Rounded => Sliced("rounded", 128, 40f);
        /// <summary>Small-corner rounded rectangle (corner radius 20 px).</summary>
        public static Sprite RoundedSmall => Sliced("rounded_s", 96, 20f);
        /// <summary>Full pill (corner radius = half height).</summary>
        public static Sprite Pill => Sliced("pill", 128, 63f);
        /// <summary>Blurred rounded rectangle for soft drop shadows.</summary>
        public static Sprite SoftShadow => Get("shadow", () => BuildSoftShadow(160, 44f, 28f));
        public static Sprite Square => Get("square", () => Build(8, p => -1f, new Vector4()));
        public static Sprite Circle => Get("circle", () => Build(128, p => Disc(p, 0.98f), new Vector4()));
        public static Sprite Ring => Get("ring", () => Build(128, p => Mathf.Abs(Len(p) - 0.86f) - 0.1f, new Vector4()));
        public static Sprite Glow => Get("glow", () => BuildGlow(128));
        public static Sprite Rays => Get("rays", () => BuildRays(256, 14));
        /// <summary>Triangle pointing down, for bunting flags.</summary>
        public static Sprite Pennant => Icon(UIIcon.Pennant);

        public static Sprite Icon(UIIcon icon) => Get("icon_" + icon, () => Build(128, IconShape(icon), new Vector4()));

        // ------------------------------------------------------------------ cache

        static Sprite Get(string key, Func<Sprite> make)
        {
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            s = Resources.Load<Sprite>("UI/" + key);
            if (s == null) s = make();
            s.name = "ui_" + key;
            Cache[key] = s;
            return s;
        }

        static Sprite Sliced(string key, int size, float radiusPx)
        {
            return Get(key, () =>
            {
                float r = radiusPx / size * 2f; // radius in -1..1 space
                float border = radiusPx + 2f;
                return Build(size, p => RoundBox(p, new Vector2(1f, 1f), r), new Vector4(border, border, border, border));
            });
        }

        // ------------------------------------------------------------------ rasterising

        /// <summary>Rasterises a signed distance function given in -1..1 space (negative = inside).</summary>
        static Sprite Build(int size, Func<Vector2, float> sdf, Vector4 border)
        {
            var tex = NewTexture(size);
            var px = new Color32[size * size];
            float pixel = 2f / size;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float a = Mathf.Clamp01(0.5f - sdf(p) / pixel);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            return Finish(tex, px, size, border);
        }

        static Sprite BuildSoftShadow(int size, float radiusPx, float blurPx)
        {
            var tex = NewTexture(size);
            var px = new Color32[size * size];
            float half = size * 0.5f;
            float inner = half - blurPx;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f - half, y + 0.5f - half);
                float d = RoundBox(p, new Vector2(inner, inner), Mathf.Min(radiusPx, inner));
                float a = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d + blurPx * 0.15f) / blurPx));
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            float b = radiusPx + blurPx;
            return Finish(tex, px, size, new Vector4(b, b, b, b));
        }

        static Sprite BuildGlow(int size)
        {
            var tex = NewTexture(size);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float a = Mathf.Clamp01(1f - Len(p));
                a = a * a;
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            return Finish(tex, px, size, new Vector4());
        }

        static Sprite BuildRays(int size, int count)
        {
            var tex = NewTexture(size);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float ang = Mathf.Atan2(p.y, p.x);
                float ray = Mathf.Cos(ang * count) * 0.5f + 0.5f;
                ray = Mathf.SmoothStep(0.45f, 0.75f, ray);
                float fade = Mathf.Clamp01(1f - Len(p));
                px[y * size + x] = new Color32(255, 255, 255, (byte)(ray * fade * 255f));
            }
            return Finish(tex, px, size, new Vector4());
        }

        static Texture2D NewTexture(int size) => new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        static Sprite Finish(Texture2D tex, Color32[] px, int size, Vector4 border)
        {
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, border);
        }

        // ------------------------------------------------------------------ shapes (-1..1 space)

        static Func<Vector2, float> IconShape(UIIcon icon)
        {
            switch (icon)
            {
                case UIIcon.Play:
                    return p => Polygon(p, new Vector2(-0.42f, -0.62f), new Vector2(0.68f, 0f), new Vector2(-0.42f, 0.62f)) - 0.1f;
                case UIIcon.Pause:
                    return p => Mathf.Min(RoundBox(p - new Vector2(-0.3f, 0), new Vector2(0.17f, 0.62f), 0.1f),
                        RoundBox(p - new Vector2(0.3f, 0), new Vector2(0.17f, 0.62f), 0.1f));
                case UIIcon.Close:
                    return p => Mathf.Min(Segment(p, new Vector2(-0.52f, -0.52f), new Vector2(0.52f, 0.52f)),
                        Segment(p, new Vector2(-0.52f, 0.52f), new Vector2(0.52f, -0.52f))) - 0.15f;
                case UIIcon.Check:
                    return p => Mathf.Min(Segment(p, new Vector2(-0.6f, 0.02f), new Vector2(-0.18f, -0.45f)),
                        Segment(p, new Vector2(-0.18f, -0.45f), new Vector2(0.62f, 0.5f))) - 0.15f;
                case UIIcon.Plus:
                    return p => Mathf.Min(RoundBox(p, new Vector2(0.62f, 0.16f), 0.12f), RoundBox(p, new Vector2(0.16f, 0.62f), 0.12f));
                case UIIcon.Back:
                    return p => Mathf.Min(Mathf.Min(Segment(p, new Vector2(-0.55f, 0f), new Vector2(0.6f, 0f)),
                        Segment(p, new Vector2(-0.55f, 0f), new Vector2(-0.1f, 0.48f))),
                        Segment(p, new Vector2(-0.55f, 0f), new Vector2(-0.1f, -0.48f))) - 0.13f;
                case UIIcon.Gear:
                    return Gear;
                case UIIcon.Lock:
                    return p =>
                    {
                        float body = RoundBox(p - new Vector2(0, -0.28f), new Vector2(0.6f, 0.46f), 0.14f);
                        var q = p - new Vector2(0, 0.2f);
                        float arc = Mathf.Abs(Len(q) - 0.36f) - 0.12f;
                        if (q.y < 0) arc = Mathf.Max(Mathf.Abs(Mathf.Abs(q.x) - 0.36f) - 0.12f, q.y - 0f) ;
                        float keyhole = Disc(p - new Vector2(0, -0.22f), 0.11f);
                        return Mathf.Max(Mathf.Min(body, arc), -keyhole);
                    };
                case UIIcon.Star:
                    return p => Star(p * 1.05f, 0.92f, 0.42f) - 0.04f;
                case UIIcon.Home:
                    return p =>
                    {
                        float roof = Polygon(p, new Vector2(-0.82f, 0.05f), new Vector2(0f, 0.78f), new Vector2(0.82f, 0.05f)) - 0.06f;
                        float body = RoundBox(p - new Vector2(0, -0.32f), new Vector2(0.55f, 0.42f), 0.08f);
                        float door = RoundBox(p - new Vector2(0, -0.48f), new Vector2(0.16f, 0.28f), 0.06f);
                        return Mathf.Max(Mathf.Min(roof, body), -door);
                    };
                case UIIcon.Retry:
                    return p =>
                    {
                        float ring = Mathf.Abs(Len(p) - 0.55f) - 0.14f;
                        // cut a wedge at the top right for the arrow gap
                        float ang = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg;
                        if (ang > 20f && ang < 80f) ring = Mathf.Max(ring, 0.2f);
                        float head = Polygon(p, new Vector2(0.18f, 0.48f), new Vector2(0.72f, 0.95f), new Vector2(0.78f, 0.28f)) - 0.02f;
                        return Mathf.Min(ring, head);
                    };
                case UIIcon.Bag:
                    return p =>
                    {
                        float body = RoundBox(p - new Vector2(0, -0.25f), new Vector2(0.66f, 0.55f), 0.16f);
                        var q = p - new Vector2(0, 0.3f);
                        float handle = Mathf.Abs(Len(q) - 0.32f) - 0.1f;
                        handle = Mathf.Max(handle, -q.y);
                        return Mathf.Min(body, handle);
                    };
                case UIIcon.Speaker:
                    return p =>
                    {
                        float box = RoundBox(p - new Vector2(-0.5f, 0), new Vector2(0.18f, 0.24f), 0.05f);
                        float cone = Polygon(p, new Vector2(-0.4f, -0.24f), new Vector2(0.05f, -0.62f), new Vector2(0.05f, 0.62f), new Vector2(-0.4f, 0.24f)) - 0.04f;
                        var q = p - new Vector2(0.05f, 0);
                        float wave = Mathf.Abs(Len(q) - 0.42f) - 0.07f;
                        wave = Mathf.Max(wave, -(q.x - Mathf.Abs(q.y) * 0.6f));
                        return Mathf.Min(Mathf.Min(box, cone), wave);
                    };
                case UIIcon.Note:
                    return p =>
                    {
                        float head1 = Ellipse(p - new Vector2(-0.38f, -0.48f), new Vector2(0.24f, 0.18f));
                        float head2 = Ellipse(p - new Vector2(0.42f, -0.34f), new Vector2(0.24f, 0.18f));
                        float s1 = Segment(p, new Vector2(-0.18f, -0.46f), new Vector2(-0.18f, 0.6f)) - 0.07f;
                        float s2 = Segment(p, new Vector2(0.62f, -0.32f), new Vector2(0.62f, 0.74f)) - 0.07f;
                        float bar = Segment(p, new Vector2(-0.18f, 0.6f), new Vector2(0.62f, 0.74f)) - 0.12f;
                        return Mathf.Min(Mathf.Min(Mathf.Min(head1, head2), Mathf.Min(s1, s2)), bar);
                    };
                case UIIcon.Vibrate:
                    return p =>
                    {
                        float phone = Mathf.Abs(RoundBox(p, new Vector2(0.34f, 0.62f), 0.12f)) - 0.08f;
                        float l = Mathf.Min(Segment(p, new Vector2(-0.62f, -0.3f), new Vector2(-0.62f, 0.3f)),
                            Segment(p, new Vector2(0.62f, -0.3f), new Vector2(0.62f, 0.3f))) - 0.07f;
                        return Mathf.Min(phone, l);
                    };
                case UIIcon.Video:
                    return p =>
                    {
                        float box = RoundBox(p, new Vector2(0.82f, 0.6f), 0.2f);
                        float tri = Polygon(p, new Vector2(-0.22f, -0.3f), new Vector2(0.36f, 0f), new Vector2(-0.22f, 0.3f)) - 0.04f;
                        return Mathf.Max(box, -tri);
                    };
                case UIIcon.Pin:
                    return p =>
                    {
                        float head = Disc(p - new Vector2(0, 0.2f), 0.55f);
                        float tip = Polygon(p, new Vector2(-0.42f, -0.02f), new Vector2(0.42f, -0.02f), new Vector2(0f, -0.9f));
                        float hole = Disc(p - new Vector2(0, 0.2f), 0.22f);
                        return Mathf.Max(Mathf.Min(head, tip), -hole);
                    };
                case UIIcon.Pennant:
                    return p => Polygon(p, new Vector2(-0.9f, 0.9f), new Vector2(0.9f, 0.9f), new Vector2(0f, -0.9f)) + 0.02f;
                case UIIcon.Coin:
                    return p => Disc(p, 0.96f);
                case UIIcon.Gift:
                    return p =>
                    {
                        float box = RoundBox(p - new Vector2(0, -0.3f), new Vector2(0.66f, 0.5f), 0.08f);
                        float lid = RoundBox(p - new Vector2(0, 0.28f), new Vector2(0.78f, 0.16f), 0.06f);
                        float gap = Mathf.Min(RoundBox(p, new Vector2(0.08f, 1f), 0f), RoundBox(p - new Vector2(0, 0.06f), new Vector2(1f, 0.05f), 0f));
                        float bowL = Ellipse(p - new Vector2(-0.22f, 0.6f), new Vector2(0.24f, 0.16f));
                        float bowR = Ellipse(p - new Vector2(0.22f, 0.6f), new Vector2(0.24f, 0.16f));
                        return Mathf.Max(Mathf.Min(Mathf.Min(box, lid), Mathf.Min(bowL, bowR)), -gap);
                    };
                case UIIcon.Map:
                    return p =>
                    {
                        float a = Polygon(p, new Vector2(-0.8f, -0.6f), new Vector2(-0.3f, -0.75f), new Vector2(-0.3f, 0.75f), new Vector2(-0.8f, 0.6f));
                        float b = Polygon(p, new Vector2(-0.22f, -0.75f), new Vector2(0.22f, -0.6f), new Vector2(0.22f, 0.6f), new Vector2(-0.22f, 0.75f));
                        float c = Polygon(p, new Vector2(0.3f, -0.6f), new Vector2(0.8f, -0.75f), new Vector2(0.8f, 0.75f), new Vector2(0.3f, 0.6f));
                        return Mathf.Min(Mathf.Min(a, b), c) - 0.02f;
                    };
                case UIIcon.Shield:
                    return p =>
                    {
                        float s = Polygon(p, new Vector2(-0.7f, 0.65f), new Vector2(0f, 0.85f), new Vector2(0.7f, 0.65f),
                            new Vector2(0.6f, -0.15f), new Vector2(0f, -0.85f), new Vector2(-0.6f, -0.15f)) - 0.04f;
                        return s;
                    };
                default:
                    return p => Disc(p, 0.8f);
            }
        }

        static float Gear(Vector2 p)
        {
            float d = Disc(p, 0.58f);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                var q = Rotate(p, -a);
                d = Mathf.Min(d, RoundBox(q - new Vector2(0.66f, 0f), new Vector2(0.2f, 0.15f), 0.05f));
            }
            return Mathf.Max(d, -Disc(p, 0.24f));
        }

        // ------------------------------------------------------------------ SDF primitives

        static float Len(Vector2 p) => Mathf.Sqrt(p.x * p.x + p.y * p.y);
        static float Disc(Vector2 p, float r) => Len(p) - r;

        static float Ellipse(Vector2 p, Vector2 r)
        {
            // cheap approximation, fine for small icon parts
            float k = Len(new Vector2(p.x / r.x, p.y / r.y));
            return (k - 1f) * Mathf.Min(r.x, r.y);
        }

        static float RoundBox(Vector2 p, Vector2 half, float r)
        {
            var q = new Vector2(Mathf.Abs(p.x) - half.x + r, Mathf.Abs(p.y) - half.y + r);
            var outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f));
            return Len(outside) + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - r;
        }

        static float Segment(Vector2 p, Vector2 a, Vector2 b)
        {
            var pa = p - a;
            var ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return Len(pa - ba * h);
        }

        static Vector2 Rotate(Vector2 p, float a)
        {
            float c = Mathf.Cos(a), s = Mathf.Sin(a);
            return new Vector2(c * p.x - s * p.y, s * p.x + c * p.y);
        }

        /// <summary>Exact signed distance to a closed polygon (Inigo Quilez).</summary>
        static float Polygon(Vector2 p, params Vector2[] v)
        {
            float d = Vector2.Dot(p - v[0], p - v[0]);
            float s = 1f;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                var e = v[j] - v[i];
                var w = p - v[i];
                var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
            }
            return s * Mathf.Sqrt(d);
        }

        static float Star(Vector2 p, float outer, float inner)
        {
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                float r = i % 2 == 0 ? outer : inner;
                pts[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }
            return Polygon(p, pts);
        }
    }
}
