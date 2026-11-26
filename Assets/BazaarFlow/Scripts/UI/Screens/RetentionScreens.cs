using System;
using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>Daily login reward: 7 day tiles, today's tile glows, one big CLAIM button.</summary>
    public class DailyRewardScreen : UIScreen
    {
        protected override bool IsPopup => true;

        RectTransform _card;
        readonly RectTransform[] _tiles = new RectTransform[7];
        readonly Image[] _tileFaces = new Image[7];
        readonly Image[] _ticks = new Image[7];
        JuicyButton _claim;

        protected override void Build()
        {
            var content = UIFactory.Card(Safe, "DAILY REWARD", new Vector2(940f, 1200f), out _card, UITheme.Orange);
            _card.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(940f, 1200f));
            PopTarget = _card;
            AddCloseButton(_card, () => Hide());

            var sub = UIFactory.Label(content, "Come back every day for bigger rewards!", UITheme.BodySize, UITheme.Ink, false);
            sub.rectTransform.HStretch(1f, -40f, 80f, 0f);

            // 3 + 3 small tiles, then a wide day-7 chest tile.
            for (int i = 0; i < 7; i++)
            {
                var r = DailyReward.Chain[i];
                Vector2 size = i < 6 ? new Vector2(240f, 220f) : new Vector2(800f, 200f);
                Vector2 pos = i < 6
                    ? new Vector2(-265f + (i % 3) * 265f, -210f - (i / 3) * 250f)
                    : new Vector2(0f, -710f);
                var tile = UIFactory.Rect("Day" + r.Day, content).Place(new Vector2(0.5f, 1f), pos, size);
                var edge = UIFactory.Image(tile, "Edge", UISprites.Rounded, UITheme.Ink);
                edge.rectTransform.Stretch(-6f, -14f, -6f, -6f);
                var face = UIFactory.Image(tile, "Face", UISprites.Rounded, Color.white);
                face.rectTransform.Stretch();
                face.Gradient(UITheme.Cream, UITheme.PaperDark);
                _tileFaces[i] = face;

                var day = UIFactory.Label(tile, "Day " + r.Day, 36, UITheme.Ink, false);
                day.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(size.x, 50f));

                var coin = UIFactory.Coin(tile, i < 6 ? 80f : 110f);
                coin.anchorMin = coin.anchorMax = new Vector2(i < 6 ? 0.5f : 0.3f, 0.5f);
                coin.anchoredPosition = new Vector2(0f, i < 6 ? 4f : -10f);

                string what = r.Coins.ToString();
                if (r.Boosters > 0) what += " + boosters";
                var amount = UIFactory.Label(tile, what, i < 6 ? 44 : 56, UITheme.Gold, UITheme.Ink);
                amount.rectTransform.Place(new Vector2(i < 6 ? 0.5f : 0.62f, 0f), new Vector2(0f, i < 6 ? 40f : 100f),
                    new Vector2(i < 6 ? 240f : 500f, 70f));

                var tick = UIFactory.Icon(tile, UIIcon.Check, UITheme.Hex("#2FB24E"), 120f);
                tick.rectTransform.anchorMin = tick.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                _ticks[i] = tick;
                _tiles[i] = tile;
            }

            _claim = UIFactory.Button(content, "CLAIM", UITheme.Green, new Vector2(620f, 150f), UIIcon.Gift, 68);
            ((RectTransform)_claim.transform).Place(new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(620f, 150f));
            _claim.Clicked += Claim;
        }

        protected override void OnShow()
        {
            int today = DailyReward.NextIndex;
            for (int i = 0; i < 7; i++)
            {
                _ticks[i].gameObject.SetActive(i < today);
                var g = _tileFaces[i].GetComponent<UIGradient>();
                if (i == today)
                {
                    g?.Set(UITheme.Hex("#FFF2A8"), UITheme.Hex("#FFB21F"));
                    Tween.Breathe(_tiles[i], 0.05f, 1.1f);
                }
                else g?.Set(UITheme.Cream, i < today ? UITheme.Hex("#CDE9C0") : UITheme.PaperDark);
            }
            bool can = DailyReward.Available;
            if (_claim.Interactable != can) _claim.Interactable = can;
            _claim.MakeCallToAction();
        }

        void Claim()
        {
            if (!DailyReward.Available) { Hide(); return; }
            int idx = DailyReward.NextIndex;
            var r = DailyReward.Claim();
            UIServices.NotifyWalletChanged();
            UIServices.Sfx("ui_coin");
            Root.Fx.CoinFly(_tiles[idx], (RectTransform)Root.Hud.Coins.transform, UIFx.CoinCount(r.Coins / 10), null, null);
            Root.Fx.Toast(r.Boosters > 0 ? $"+{r.Coins} coins and boosters!" : $"+{r.Coins} coins!");
            _ticks[idx].gameObject.SetActive(true);
            Tween.Delay(this, 0.6f, () => Hide());
        }
    }

    /// <summary>Shown when the player taps Play with no lives: wait, watch an ad for one, or refill with coins.</summary>
    public class OutOfLivesScreen : UIScreen
    {
        protected override bool IsPopup => true;

        RectTransform _card;
        Text _timer;
        JuicyButton _ad, _coins;
        Action _onReady;

        protected override void Build()
        {
            var content = UIFactory.Card(Safe, "OUT OF LIVES", new Vector2(900f, 900f), out _card, UITheme.Red);
            _card.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 900f));
            PopTarget = _card;
            AddCloseButton(_card, () => Hide());

            var heart = LivesBadge.Heart(content, 170f);
            heart.Place(new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(170f, 170f));

            _timer = UIFactory.Label(content, "Next life in 30:00", UITheme.BodySize + 4, UITheme.Ink, false);
            _timer.rectTransform.HStretch(1f, -250f, 70f, 0f);

            _ad = UIFactory.Button(content, "+1 Life", UITheme.Green, new Vector2(700f, 160f), UIIcon.Video, 64);
            ((RectTransform)_ad.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -400f), new Vector2(700f, 160f));
            _ad.Clicked += () => UIServices.ShowRewarded("life", ok =>
            {
                if (!ok) { Root.Fx.Toast("Ad not available. Try again!"); return; }
                Lives.AddLives(1);
                Ready();
            });

            _coins = UIFactory.Button(content, "Refill   " + Lives.RefillCoinPrice, UITheme.Orange, new Vector2(700f, 150f), null, 56);
            ((RectTransform)_coins.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -590f), new Vector2(700f, 150f));
            var coin = UIFactory.Coin(_coins.Face, 70f);
            coin.anchorMin = coin.anchorMax = new Vector2(0.5f, 0.5f);
            coin.anchoredPosition = new Vector2(150f, 4f);
            _coins.Clicked += () =>
            {
                if (!Lives.TryRefillWithCoins())
                {
                    Root.Fx.Toast("Not enough coins");
                    Root.OpenShop();
                    return;
                }
                UIServices.NotifyWalletChanged();
                Ready();
            };
        }

        public void Open(Action onReady)
        {
            _onReady = onReady;
            Show();
            _ad.MakeCallToAction();
        }

        void Ready()
        {
            UIServices.Sfx("ui_coin");
            var then = _onReady;
            _onReady = null;
            Hide(() => then?.Invoke());
        }

        void Update()
        {
            if (!IsOpen) return;
            if (Lives.CanPlay) { Ready(); return; }
            _timer.text = "Next life in " + LivesBadge.Format(Lives.TimeToNext);
        }
    }

    /// <summary>Heart with the life count and the time to the next life. Sits on the main menu.</summary>
    public class LivesBadge : MonoBehaviour
    {
        Text _count, _timer;

        public static LivesBadge Create(Transform parent)
        {
            var rt = UIFactory.Rect("Lives", parent);
            rt.sizeDelta = new Vector2(330f, 92f);
            var pill = UIFactory.Pill(rt, new Vector2(330f, 92f), out var label);
            pill.Stretch();
            label.rectTransform.Stretch(110f, 0f, 16f, 0f);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 40;
            var heart = Heart(rt, 120f);
            heart.Place(new Vector2(0f, 0.5f), new Vector2(40f, 2f), new Vector2(120f, 120f));
            var count = UIFactory.Label(heart, "5", 56, Color.white, UITheme.MaroonDark);
            count.rectTransform.Stretch(0f, 6f, 0f, 0f);
            var b = rt.gameObject.AddComponent<LivesBadge>();
            b._count = count;
            b._timer = label;
            return b;
        }

        /// <summary>A heart made of two circles and a rotated square.</summary>
        public static RectTransform Heart(Transform parent, float size)
        {
            var rt = UIFactory.Rect("Heart", parent);
            rt.sizeDelta = new Vector2(size, size);
            Color red = UITheme.Hex("#E23B3B");
            float r = size * 0.32f;
            var sq = UIFactory.Image(rt, "Body", UISprites.RoundedSmall, red);
            sq.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -size * 0.08f), new Vector2(size * 0.55f, size * 0.55f));
            sq.rectTransform.localRotation = Quaternion.Euler(0, 0, 45f);
            for (int s = -1; s <= 1; s += 2)
            {
                var c = UIFactory.Image(rt, "Lobe", UISprites.Circle, red);
                c.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(s * size * 0.18f, size * 0.1f), new Vector2(r * 2f, r * 2f));
            }
            var shine = UIFactory.Image(rt, "Shine", UISprites.Circle, new Color(1, 1, 1, 0.45f));
            shine.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-size * 0.2f, size * 0.17f), new Vector2(size * 0.16f, size * 0.12f));
            return rt;
        }

        public static string Format(TimeSpan t) => $"{(int)t.TotalMinutes:00}:{t.Seconds:00}";

        void Update()
        {
            if (Lives.Unlimited)
            {
                _count.text = Lives.Max.ToString();
                _timer.text = "Unlimited";
                return;
            }
            int n = Lives.Count;
            _count.text = n.ToString();
            _timer.text = n >= Lives.Max ? "Full" : Format(Lives.TimeToNext);
        }
    }

    /// <summary>Four booster buttons along the bottom of the HUD, each with a count badge (or + to buy).</summary>
    public class BoosterBar : MonoBehaviour
    {
        UIRoot _root;
        readonly JuicyButton[] _buttons = new JuicyButton[Boosters.Count];
        readonly Text[] _badges = new Text[Boosters.Count];
        readonly BoosterType[] _types = new BoosterType[Boosters.Count];

        static readonly string[] Short = { "+4", "Sweep", "Reveal", "Reverse" };
        static readonly ButtonPalette[] Palettes = { UITheme.Teal3D, UITheme.Orange, UITheme.Purple, UITheme.Red };

        public static BoosterBar Create(Transform parent, UIRoot root)
        {
            var rt = UIFactory.Rect("Boosters", parent);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 150f);
            rt.anchoredPosition = new Vector2(0f, 14f);
            var bar = rt.gameObject.AddComponent<BoosterBar>();
            bar._root = root;
            for (int i = 0; i < Boosters.Count; i++)
            {
                var type = (BoosterType)i;
                bar._types[i] = type;
                var b = UIFactory.Button(rt, Short[i], Palettes[i], new Vector2(210f, 120f), null, 40);
                var brt = (RectTransform)b.transform;
                brt.anchorMin = brt.anchorMax = new Vector2((i + 0.5f) / Boosters.Count, 0.5f);
                brt.anchoredPosition = Vector2.zero;
                brt.sizeDelta = new Vector2(210f, 120f);
                b.Clicked += () => bar.Use(type);
                bar._buttons[i] = b;

                var badge = UIFactory.Rect("Badge", brt).Place(new Vector2(1f, 1f), new Vector2(-14f, -6f), new Vector2(68f, 68f));
                var be = UIFactory.Image(badge, "Edge", UISprites.Circle, UITheme.Ink);
                be.rectTransform.Stretch(-4f, -4f, -4f, -4f);
                var bf = UIFactory.Image(badge, "Fill", UISprites.Circle, UITheme.Gold);
                bf.rectTransform.Stretch();
                var t = UIFactory.Label(badge, "0", 36, UITheme.Ink, false);
                t.rectTransform.Stretch();
                bar._badges[i] = t;
            }
            return bar;
        }

        void Use(BoosterType type)
        {
            var game = _root.Game;
            if (game == null || game.State != GameState.Playing) return;
            if (Economy.BoosterCount(type) <= 0)
            {
                if (Economy.TryBuyBoosterWithCoins(type))
                {
                    UIServices.NotifyWalletChanged();
                    _root.Fx.Toast($"Bought {Boosters.Name(type)} for {MonetizationConfig.BoosterCoinPrice} coins");
                }
                else
                {
                    _root.Fx.Toast("Not enough coins");
                    _root.OpenShop();
                    return;
                }
            }
            if (!game.UseBooster(type))
            {
                _root.Fx.Toast(Boosters.Description(type) + ": nothing to do right now");
                return;
            }
            UIServices.NotifyWalletChanged();
            UIServices.Haptic(HapticStrength.Medium);
        }

        void Update()
        {
            var game = _root.Game;
            for (int i = 0; i < _buttons.Length; i++)
            {
                int n = Economy.BoosterCount(_types[i]);
                _badges[i].text = n > 0 ? n.ToString() : "+";
                bool usable = game != null && game.State == GameState.Playing && game.CanUseBooster(_types[i]);
                if (_buttons[i].Interactable != usable) _buttons[i].Interactable = usable;
            }
        }
    }
}
