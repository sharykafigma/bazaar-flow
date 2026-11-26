using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow.Visuals
{
    /// <summary>
    /// Tiny pooled sprite-particle system for juice (sparkles, petals, puffs, rings).
    /// Cheaper and easier to tune from code than ParticleSystem for a handful of bursts.
    /// </summary>
    public class Fx : MonoBehaviour
    {
        public const int Order = 50;

        struct P
        {
            public SpriteRenderer sr;
            public Vector3 vel;
            public float gravity, drag, life, age, size0, size1, spin, wobble, phase;
            public Color color;
            public bool fadeIn;
        }

        readonly List<P> _live = new List<P>();
        readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();

        static Fx _instance;

        public static Fx Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("BazaarFx");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<Fx>();
                }
                return _instance;
            }
        }

        SpriteRenderer Rent(Sprite sprite, int order)
        {
            SpriteRenderer sr = null;
            while (_pool.Count > 0 && sr == null) sr = _pool.Pop();
            if (sr == null)
            {
                var go = new GameObject("p");
                go.transform.SetParent(transform, false);
                sr = go.AddComponent<SpriteRenderer>();
            }
            sr.gameObject.SetActive(true);
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        public void Emit(string sprite, Vector3 pos, Vector3 vel, Color color, float life, float size0, float size1,
            float gravity = 0f, float drag = 0f, float spin = 0f, float wobble = 0f, int order = Order, bool fadeIn = false)
        {
            var s = BazaarArt.Get(sprite, 1f);
            if (s == null) return;
            var sr = Rent(s, order);
            pos.z = -1f;
            sr.transform.position = pos;
            sr.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            sr.transform.localScale = Vector3.one * size0;
            sr.color = color;
            _live.Add(new P
            {
                sr = sr, vel = vel, gravity = gravity, drag = drag, life = life, age = 0f,
                size0 = size0, size1 = size1, spin = spin, wobble = wobble, phase = Random.value * 10f,
                color = color, fadeIn = fadeIn
            });
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var p = _live[i];
                p.age += dt;
                if (p.age >= p.life || p.sr == null)
                {
                    if (p.sr != null)
                    {
                        p.sr.gameObject.SetActive(false);
                        _pool.Push(p.sr);
                    }
                    _live.RemoveAt(i);
                    continue;
                }
                float t = p.age / p.life;
                p.vel.y -= p.gravity * dt;
                p.vel *= 1f / (1f + p.drag * dt);
                var tr = p.sr.transform;
                Vector3 wob = p.wobble != 0f ? new Vector3(Mathf.Sin(p.age * 5f + p.phase) * p.wobble * dt, 0, 0) : Vector3.zero;
                tr.position += p.vel * dt + wob;
                tr.Rotate(0, 0, p.spin * dt);
                tr.localScale = Vector3.one * Mathf.Lerp(p.size0, p.size1, t);
                float a = 1f - Mathf.SmoothStep(0.55f, 1f, t);
                if (p.fadeIn) a *= Mathf.Clamp01(t * 6f);
                var c = p.color;
                c.a *= a;
                p.sr.color = c;
                _live[i] = p;
            }
        }

        // ---------------- presets ----------------

        public static void Sparkles(Vector3 pos, Color tint, int count = 8, float speed = 2.6f)
        {
            var fx = Instance;
            for (int i = 0; i < count; i++)
            {
                float a = (i + Random.value * 0.6f) / count * Mathf.PI * 2f;
                var v = new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * speed * Random.Range(0.6f, 1.1f);
                var c = Color.Lerp(Color.white, tint, Random.Range(0.1f, 0.5f));
                fx.Emit("fx_sparkle", pos, v, c, Random.Range(0.35f, 0.55f), 0.32f, 0.05f, 0f, 4f, Random.Range(-200f, 200f));
            }
        }

        public static void Ring(Vector3 pos, Color color, float size = 1.2f, float life = 0.35f)
        {
            Instance.Emit("fx_ring", pos, Vector3.zero, color, life, 0.2f, size, order: Order - 1);
        }

        public static void Glow(Vector3 pos, Color color, float size = 1.4f, float life = 0.3f)
        {
            Instance.Emit("fx_glow", pos, Vector3.zero, color, life, size * 0.6f, size, order: Order - 2);
        }

        public static void Puff(Vector3 pos, int count = 6, float spread = 1.4f, Color? color = null)
        {
            var c = color ?? new Color(1f, 0.95f, 0.85f, 0.85f);
            for (int i = 0; i < count; i++)
            {
                float a = Random.value * Mathf.PI * 2f;
                var v = new Vector3(Mathf.Cos(a), Mathf.Sin(a) * 0.6f + 0.3f) * spread * Random.Range(0.5f, 1f);
                Instance.Emit("fx_puff", pos, v, c, Random.Range(0.35f, 0.6f), 0.35f, 0.8f, 0f, 3f);
            }
        }

        static readonly Color[] PetalColors =
        {
            new Color(1f, 0.55f, 0.1f), new Color(1f, 0.72f, 0.12f), new Color(1f, 0.85f, 0.25f),
            new Color(0.95f, 0.36f, 0.55f), new Color(1f, 0.45f, 0.2f)
        };

        /// <summary>Marigold petals bursting out of a point (shop done).</summary>
        public static void PetalBurst(Vector3 pos, int count = 14)
        {
            for (int i = 0; i < count; i++)
            {
                float a = Random.Range(0.15f, 0.85f) * Mathf.PI;
                var v = new Vector3(Mathf.Cos(a) * 2.6f, Mathf.Sin(a) * 4.5f) * Random.Range(0.6f, 1.1f);
                Instance.Emit("fx_petal", pos, v, PetalColors[Random.Range(0, PetalColors.Length)], Random.Range(0.9f, 1.4f),
                    0.26f, 0.2f, 9f, 1.2f, Random.Range(-360f, 360f), 6f);
            }
        }

        /// <summary>Petal + confetti rain over the whole screen (level won).</summary>
        public static void Celebrate(Camera cam, int count = 70)
        {
            if (cam == null) return;
            float h = cam.orthographicSize, w = h * cam.aspect;
            Vector3 c = cam.transform.position;
            for (int i = 0; i < count; i++)
            {
                var pos = new Vector3(c.x + Random.Range(-w, w), c.y + h + Random.Range(0.2f, 3f), 0);
                var v = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-2.5f, -1.2f));
                bool petal = Random.value < 0.6f;
                var col = petal ? PetalColors[Random.Range(0, PetalColors.Length)] : Color.HSVToRGB(Random.value, 0.65f, 1f);
                Instance.Emit(petal ? "fx_petal" : "fx_confetti", pos, v, col, Random.Range(3.5f, 5f),
                    petal ? 0.3f : 0.22f, petal ? 0.26f : 0.2f, 0.6f, 0.4f, Random.Range(-300f, 300f), 18f, Order + 5, true);
            }
        }

        /// <summary>A little coin that hops up from a point (delivery reward feel).</summary>
        public static void CoinPop(Vector3 pos)
        {
            Instance.Emit("fx_coin", pos, new Vector3(Random.Range(-0.4f, 0.4f), 3.2f), Color.white, 0.6f, 0.34f, 0.28f, 9f, 0f, 0f, 0f, Order + 2);
        }
    }
}
