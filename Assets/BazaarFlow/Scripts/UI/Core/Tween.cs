using System;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow.UI
{
    public enum Ease { Linear, InQuad, OutQuad, InOutSine, OutCubic, OutBack, InBack, OutElastic, OutBounce }

    /// <summary>
    /// Tiny tween engine for UI juice (no DOTween dependency). Runs on unscaled time so it works while the game is paused.
    /// A tween is keyed by (target, channel); starting a new one on the same key replaces the old one.
    /// </summary>
    public class Tween : MonoBehaviour
    {
        class Item
        {
            public UnityEngine.Object Target;
            public string Channel;
            public float Delay, Duration, Time;
            public Ease Ease;
            public Action<float> Step;
            public Action Done;
            public bool Loop;
            public bool Dead;
        }

        static Tween _runner;
        readonly List<Item> _items = new List<Item>();
        readonly List<Item> _adding = new List<Item>();

        static Tween Runner
        {
            get
            {
                if (_runner == null)
                {
                    var go = new GameObject("UITween");
                    DontDestroyOnLoad(go);
                    _runner = go.AddComponent<Tween>();
                }
                return _runner;
            }
        }

        /// <summary>Runs step(eased 0..1) over duration seconds.</summary>
        public static void Run(UnityEngine.Object target, string channel, float duration, Ease ease, Action<float> step,
            float delay = 0f, Action done = null, bool loop = false)
        {
            Kill(target, channel);
            Runner._adding.Add(new Item
            {
                Target = target, Channel = channel, Duration = Mathf.Max(0.0001f, duration), Delay = delay,
                Ease = ease, Step = step, Done = done, Loop = loop
            });
        }

        public static void Delay(UnityEngine.Object target, float seconds, Action done) =>
            Run(target, "delay" + done.GetHashCode(), seconds, Ease.Linear, null, 0f, done);

        public static void Kill(UnityEngine.Object target, string channel = null)
        {
            if (_runner == null) return;
            foreach (var list in new[] { _runner._items, _runner._adding })
                foreach (var it in list)
                    if (it.Target == target && (channel == null || it.Channel == channel)) it.Dead = true;
        }

        void Update()
        {
            if (_adding.Count > 0)
            {
                _items.AddRange(_adding);
                _adding.Clear();
            }
            float dt = UnityEngine.Time.unscaledDeltaTime;
            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                if (it.Dead) continue;
                if (it.Target == null) { it.Dead = true; continue; } // destroyed UI object
                if (it.Delay > 0f) { it.Delay -= dt; continue; }
                it.Time += dt;
                float t = Mathf.Clamp01(it.Time / it.Duration);
                try { it.Step?.Invoke(Evaluate(it.Ease, t)); }
                catch (Exception e) { Debug.LogException(e); it.Dead = true; continue; }
                if (t >= 1f)
                {
                    if (it.Loop) { it.Time = 0f; continue; }
                    it.Dead = true;
                    try { it.Done?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
                }
            }
            _items.RemoveAll(x => x.Dead);
        }

        public static float Evaluate(Ease ease, float t)
        {
            switch (ease)
            {
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return 1f - (1f - t) * (1f - t);
                case Ease.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
                case Ease.OutCubic: { float u = 1f - t; return 1f - u * u * u; }
                case Ease.OutBack: { const float c1 = 1.70158f, c3 = c1 + 1f; float u = t - 1f; return 1f + c3 * u * u * u + c1 * u * u; }
                case Ease.InBack: { const float c1 = 1.70158f, c3 = c1 + 1f; return c3 * t * t * t - c1 * t * t; }
                case Ease.OutElastic:
                    if (t <= 0f || t >= 1f) return t;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI / 3f)) + 1f;
                case Ease.OutBounce:
                {
                    const float n1 = 7.5625f, d1 = 2.75f;
                    if (t < 1f / d1) return n1 * t * t;
                    if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
                    if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
                    t -= 2.625f / d1; return n1 * t * t + 0.984375f;
                }
                default: return t;
            }
        }

        // ------------------------------------------------------------ helpers

        public static void Scale(Transform tr, Vector3 to, float duration, Ease ease, float delay = 0f, Action done = null)
        {
            Vector3 from = tr.localScale;
            Run(tr, "scale", duration, ease, k => tr.localScale = Vector3.LerpUnclamped(from, to, k), delay, done);
        }

        public static void Move(RectTransform rt, Vector2 to, float duration, Ease ease, float delay = 0f, Action done = null)
        {
            Vector2 from = rt.anchoredPosition;
            Run(rt, "move", duration, ease, k => rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k), delay, done);
        }

        public static void Fade(CanvasGroup g, float to, float duration, float delay = 0f, Action done = null)
        {
            float from = g.alpha;
            Run(g, "fade", duration, Ease.OutQuad, k => g.alpha = Mathf.LerpUnclamped(from, to, k), delay, done);
        }

        /// <summary>Quick squash-and-stretch punch, e.g. when a counter changes.</summary>
        public static void Punch(Transform tr, float amount = 0.18f, float duration = 0.35f)
        {
            Run(tr, "scale", duration, Ease.Linear, k =>
            {
                float s = 1f + Mathf.Sin(k * Mathf.PI) * amount * (1f - k);
                tr.localScale = new Vector3(s, 2f - s, 1f);
            }, 0f, () => tr.localScale = Vector3.one);
        }

        /// <summary>Horizontal shake for errors and Belt Jam.</summary>
        public static void Shake(RectTransform rt, float strength = 18f, float duration = 0.45f)
        {
            Vector2 home = rt.anchoredPosition;
            Run(rt, "shake", duration, Ease.Linear,
                k => rt.anchoredPosition = home + new Vector2(Mathf.Sin(k * 40f) * strength * (1f - k), 0f),
                0f, () => rt.anchoredPosition = home);
        }

        /// <summary>Endless gentle breathing scale for call-to-action buttons.</summary>
        public static void Breathe(Transform tr, float amount = 0.04f, float period = 1.4f)
        {
            Run(tr, "breathe", period, Ease.Linear, k =>
            {
                float s = 1f + Mathf.Sin(k * Mathf.PI * 2f) * amount;
                tr.localScale = new Vector3(s, s, 1f);
            }, 0f, null, true);
        }
    }
}
