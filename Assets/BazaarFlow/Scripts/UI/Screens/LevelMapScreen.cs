using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>
    /// Scrolling level map: a winding dotted road through bazaar districts (20 levels each).
    /// Done levels are green, the current one is big, orange and bouncing with a pin, future ones are locked.
    /// </summary>
    public class LevelMapScreen : UIScreen
    {
        static readonly string[] Districts =
        {
            "Chai Street", "Bangle Bazaar", "Fruit Market", "Kite Lane", "Toy Alley", "Lamp Square",
            "Spice Row", "Silk Market", "Sweet Street", "Grand Bazaar"
        };

        static readonly ButtonPalette[] DistrictColors =
        {
            UITheme.Maroon3D, UITheme.Teal3D, UITheme.Orange, UITheme.Green, UITheme.Purple, UITheme.Red
        };

        const float Step = 230f;
        const float Bottom = 320f;

        ScrollRect _scroll;
        RectTransform _content;
        JuicyButton _play;
        int _builtFor = -1;

        protected override void Build()
        {
            var bg = UIFactory.Image(Rt, "Ground", UISprites.Square, Color.white, true);
            bg.rectTransform.Stretch();
            bg.Gradient(UITheme.Hex("#FFD9A0"), UITheme.Hex("#F2BB7C"));
            bg.transform.SetAsFirstSibling();

            var view = UIFactory.Rect("Viewport", Safe).Stretch();
            view.gameObject.AddComponent<RectMask2D>();
            var hit = view.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            _content = UIFactory.Rect("Content", view);
            _content.anchorMin = new Vector2(0f, 0f);
            _content.anchorMax = new Vector2(1f, 0f);
            _content.pivot = new Vector2(0.5f, 0f);
            _scroll = view.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = view;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.movementType = ScrollRect.MovementType.Elastic;
            _scroll.decelerationRate = 0.12f;
            _scroll.scrollSensitivity = 30f;

            var back = UIFactory.IconButton(Safe, UIIcon.Back, UITheme.Cream3D, 130f);
            ((RectTransform)back.transform).Place(new Vector2(0f, 1f), new Vector2(100f, -100f), new Vector2(130f, 130f));
            back.ClickSound = "ui_back";
            back.Clicked += () => { Hide(); Root.Menu.Show(); };

            var coins = CoinCounter.Create(Safe, new Vector2(300f, 88f));
            ((RectTransform)coins.transform).Place(new Vector2(1f, 1f), new Vector2(-190f, -100f), new Vector2(300f, 88f));

            _play = UIFactory.Button(Safe, "PLAY", UITheme.Green, new Vector2(640f, 170f), UIIcon.Play, 70);
            ((RectTransform)_play.transform).Place(new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(640f, 170f));
            _play.Clicked += () => Root.Play(UIServices.GetUnlockedLevel());
        }

        protected override void OnShow()
        {
            int unlocked = UIServices.GetUnlockedLevel();
            _play.SetText("PLAY  " + unlocked);
            _play.MakeCallToAction();
            if (_builtFor != unlocked) Rebuild(unlocked);
            // scroll so the current level sits a little below the middle
            Canvas.ForceUpdateCanvases();
            float viewH = ((RectTransform)_scroll.viewport).rect.height;
            float target = Bottom + (unlocked - 1) * Step - viewH * 0.4f;
            float max = Mathf.Max(1f, _content.rect.height - viewH);
            float to = Mathf.Clamp01(target / max);
            float from = Mathf.Clamp01(to - 0.08f);
            Tween.Run(_content, "scroll", 0.6f, Ease.OutCubic, k => _scroll.verticalNormalizedPosition = Mathf.Lerp(from, to, k));
        }

        static float NodeX(int level) => Mathf.Sin(level * 0.9f) * 270f;

        void Rebuild(int unlocked)
        {
            _builtFor = unlocked;
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            int count = Mathf.Max(40, (unlocked / 20 + 2) * 20);
            _content.sizeDelta = new Vector2(0f, Bottom + count * Step + 200f);

            // district banners first (behind the road)
            for (int d = 0; d * 20 < count; d++)
            {
                string name = Districts[d % Districts.Length];
                var pal = DistrictColors[d % DistrictColors.Length];
                float y = Bottom + d * 20 * Step - 150f;
                var banner = UIFactory.Ribbon(_content, name + "   " + (d * 20 + 1) + "-" + (d * 20 + 20), 760f, pal);
                banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 0f);
                banner.anchoredPosition = new Vector2(0f, y);
                banner.localScale = Vector3.one * 0.75f;
            }

            // dotted road
            for (int lv = 1; lv < count; lv++)
            {
                Vector2 a = new Vector2(NodeX(lv), Bottom + (lv - 1) * Step);
                Vector2 b = new Vector2(NodeX(lv + 1), Bottom + lv * Step);
                for (int k = 1; k <= 4; k++)
                {
                    var p = Vector2.Lerp(a, b, k / 5f);
                    var dot = UIFactory.Image(_content, "Dot", UISprites.Circle,
                        lv < unlocked ? UITheme.Hex("#FFF6E3") : UITheme.Ink.WithAlpha(0.28f));
                    dot.rectTransform.anchorMin = dot.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    dot.rectTransform.anchoredPosition = p;
                    dot.rectTransform.sizeDelta = new Vector2(26f, 26f);
                }
            }

            for (int lv = 1; lv <= count; lv++) Node(lv, unlocked);
        }

        void Node(int level, int unlocked)
        {
            bool done = level < unlocked, current = level == unlocked;
            var pal = done ? UITheme.Green : current ? UITheme.Orange : UITheme.Grey;
            float size = current ? 190f : 140f;
            var b = UIFactory.Button(_content, current || done ? level.ToString() : null, pal, new Vector2(size, size),
                current || done ? (UIIcon?)null : UIIcon.Lock, current ? 80 : 58);
            var rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(NodeX(level), Bottom + (level - 1) * Step);
            if (!done && !current)
            {
                b.Interactable = false;
                if (b.IconImage != null) b.IconImage.color = UITheme.Grey.Text;
            }
            int lvCopy = level;
            b.Clicked += () => Root.Play(lvCopy);

            // round the face: swap rounded rect for circles
            foreach (var img in rt.GetComponentsInChildren<Image>(true))
                if (img.sprite == UISprites.Rounded) { img.sprite = UISprites.Circle; img.type = Image.Type.Simple; }

            if (current)
            {
                b.MakeCallToAction();
                var pin = UIFactory.Icon(rt, UIIcon.Pin, UITheme.Red.Bottom, 110f);
                pin.rectTransform.anchorMin = pin.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                var ol = pin.gameObject.AddComponent<Outline>();
                ol.effectColor = UITheme.Ink;
                ol.effectDistance = new Vector2(4f, -4f);
                var home = new Vector2(0f, 80f);
                pin.rectTransform.anchoredPosition = home;
                Tween.Run(pin.rectTransform, "bob", 0.9f, Ease.Linear,
                    k => pin.rectTransform.anchoredPosition = home + new Vector2(0f, Mathf.Abs(Mathf.Sin(k * Mathf.PI)) * 30f), 0f, null, true);
            }
            else if (done)
            {
                var star = UIFactory.Icon(rt, UIIcon.Star, UITheme.Gold, 60f);
                star.rectTransform.anchorMin = star.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                star.rectTransform.anchoredPosition = new Vector2(0f, -8f);
                var ol = star.gameObject.AddComponent<Outline>();
                ol.effectColor = UITheme.Ink;
                ol.effectDistance = new Vector2(3f, -3f);
            }
        }
    }
}
