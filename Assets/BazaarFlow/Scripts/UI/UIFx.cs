using System;
using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>Top-layer effects: confetti bursts, coins flying to the counter, toasts.</summary>
    public class UIFx : MonoBehaviour
    {
        RectTransform _rt;

        void Awake() => _rt = (RectTransform)transform;

        void LateUpdate() => transform.SetAsLastSibling();

        /// <summary>Festive confetti from the top of the screen.</summary>
        public void Confetti(int count = 70)
        {
            float w = _rt.rect.width, h = _rt.rect.height;
            for (int i = 0; i < count; i++)
            {
                var img = UIFactory.Image(_rt, "Confetti", i % 3 == 0 ? UISprites.Circle : UISprites.RoundedSmall,
                    UITheme.Festive[i % UITheme.Festive.Length]);
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.sizeDelta = i % 3 == 0 ? new Vector2(22f, 22f) : new Vector2(18f, 34f);
                var start = new Vector2(UnityEngine.Random.Range(-w * 0.5f, w * 0.5f), UnityEngine.Random.Range(40f, 300f));
                var drift = UnityEngine.Random.Range(-160f, 160f);
                float fall = h + 200f;
                float spin = UnityEngine.Random.Range(-720f, 720f);
                float dur = UnityEngine.Random.Range(1.8f, 3.0f);
                float delay = UnityEngine.Random.Range(0f, 0.5f);
                float wobble = UnityEngine.Random.Range(3f, 7f);
                rt.anchoredPosition = start;
                Tween.Run(rt, "fall", dur, Ease.Linear, k =>
                {
                    rt.anchoredPosition = start + new Vector2(drift * k + Mathf.Sin(k * wobble * 6.28f) * 30f, -fall * k);
                    rt.localRotation = Quaternion.Euler(0, 0, spin * k);
                    rt.localScale = new Vector3(Mathf.Cos(k * wobble * 6.28f), 1f, 1f);
                }, delay, () => Destroy(rt.gameObject));
            }
        }

        public static int CoinCount(int coins) => Mathf.Clamp(coins, 3, 12);

        /// <summary>Spawns coins at 'from' that arc into 'to' one by one. onEach runs as each coin lands.</summary>
        public void CoinFly(RectTransform from, RectTransform to, int coins, Action onEach, Action onDone)
        {
            int n = CoinCount(coins);
            int landed = 0;
            for (int i = 0; i < n; i++)
            {
                var c = UIFactory.Coin(_rt, 76f);
                c.position = from.position;
                Vector3 start = from.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 90f * _rt.lossyScale.x * 1f);
                Vector3 end = to.position;
                Vector3 ctrl = Vector3.Lerp(start, end, 0.3f) + new Vector3(UnityEngine.Random.Range(-250f, 250f), 120f, 0f) * _rt.lossyScale.x;
                c.localScale = Vector3.zero;
                float delay = i * 0.06f;
                Tween.Run(c, "pop", 0.2f, Ease.OutBack, k =>
                {
                    c.position = Vector3.LerpUnclamped(from.position, start, k);
                    c.localScale = Vector3.one * k;
                }, delay);
                Tween.Run(c, "fly", 0.55f, Ease.InQuad, k =>
                {
                    Vector3 a = Vector3.Lerp(start, ctrl, k), b = Vector3.Lerp(ctrl, end, k);
                    c.position = Vector3.Lerp(a, b, k);
                    c.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, k);
                }, delay + 0.28f, () =>
                {
                    Destroy(c.gameObject);
                    landed++;
                    UIServices.Sfx("ui_coin");
                    onEach?.Invoke();
                    if (landed == n) onDone?.Invoke();
                });
            }
        }

        /// <summary>Short message pill near the top that slides in and fades out.</summary>
        public void Toast(string message)
        {
            var rt = UIFactory.Rect("Toast", _rt);
            rt.Place(new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(820f, 120f));
            var edge = UIFactory.Image(rt, "Edge", UISprites.Pill, UITheme.Ink);
            edge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var bg = UIFactory.Image(rt, "Bg", UISprites.Pill, Color.white);
            bg.rectTransform.Stretch();
            bg.Gradient(UITheme.Hex("#5A3A28"), UITheme.Hex("#2E1A10"));
            var label = UIFactory.Label(rt, message, UITheme.BodySize, Color.white);
            label.rectTransform.Stretch(30f, 0f, 30f, 0f);
            var g = rt.gameObject.AddComponent<CanvasGroup>();
            g.blocksRaycasts = false;
            rt.localScale = Vector3.one * 0.6f;
            Tween.Scale(rt, Vector3.one, 0.35f, Ease.OutBack);
            Tween.Fade(g, 0f, 0.4f, 1.6f, () => Destroy(rt.gameObject));
        }
    }
}
