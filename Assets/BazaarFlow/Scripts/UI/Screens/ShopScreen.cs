using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>
    /// Shop popup: starter-pack banner, a grid of coin packs, No Ads row, and a daily free booster row.
    /// Products, prices and purchases all come through UIServices, so the monetization module drives it.
    /// </summary>
    public class ShopScreen : UIScreen
    {
        protected override bool IsPopup => true;

        RectTransform _content, _list;
        readonly List<(UIProduct p, JuicyButton b)> _buttons = new List<(UIProduct, JuicyButton)>();

        protected override void Build()
        {
            _content = UIFactory.Card(Safe, "SHOP", new Vector2(980f, 1560f), out var card, UITheme.Purple);
            card.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(980f, 1560f));
            PopTarget = card;
            AddCloseButton(card, () => Hide());

            var view = UIFactory.Rect("Viewport", _content).Stretch();
            view.gameObject.AddComponent<RectMask2D>();
            var hit = view.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            _list = UIFactory.Rect("List", view);
            _list.anchorMin = new Vector2(0f, 1f);
            _list.anchorMax = new Vector2(1f, 1f);
            _list.pivot = new Vector2(0.5f, 1f);
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = view;
            scroll.content = _list;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            UIServices.WalletChanged += RefreshPrices;
        }

        void OnDestroy() => UIServices.WalletChanged -= RefreshPrices;

        protected override void OnShow()
        {
            Rebuild();
        }

        void Rebuild()
        {
            for (int i = _list.childCount - 1; i >= 0; i--) Destroy(_list.GetChild(i).gameObject);
            _buttons.Clear();
            var products = UIServices.GetProducts();
            float y = 30f;
            float width = 980f - 88f;

            var starter = products.Find(p => p.Id == "bf_starter_pack");
            if (starter != null && UIServices.IsStarterOfferActive())
            {
                StarterBanner(starter, ref y, width);
            }

            // Coin packs, 3 per row
            var coins = products.FindAll(p => p.Coins > 0 && p.Id != "bf_starter_pack");
            float cellW = (width - 2 * 24f) / 3f, cellH = 360f;
            for (int i = 0; i < coins.Count; i++)
            {
                int col = i % 3, row = i / 3;
                var cell = CoinCell(coins[i], new Vector2(cellW, cellH), i);
                cell.anchorMin = cell.anchorMax = new Vector2(0f, 1f);
                cell.pivot = new Vector2(0f, 1f);
                cell.anchoredPosition = new Vector2(col * (cellW + 24f), -y - row * (cellH + 40f) - 20f);
            }
            y += Mathf.CeilToInt(coins.Count / 3f) * (cellH + 40f) + 30f;

            var noAds = products.Find(p => p.Id == "bf_no_ads");
            if (noAds != null && !UIServices.IsNoAds())
                Row(ref y, UIIcon.Shield, UITheme.Red, noAds.Title, "Remove forced ads forever", noAds, null);

            var bundle = products.Find(p => p.Id == "bf_booster_bundle");
            if (bundle != null)
                Row(ref y, UIIcon.Gift, UITheme.Orange, bundle.Title, "Boosters to beat tough levels", bundle, null);

            if (UIServices.FreeBoosterAvailable())
                Row(ref y, UIIcon.Video, UITheme.Teal3D, "Free Booster", "Watch a short video, once a day", null, () =>
                    UIServices.ShowRewarded("free_booster", ok =>
                    {
                        Root.Fx.Toast(ok ? "Booster added!" : "Ad not available right now");
                        if (ok) { UIServices.NotifyWalletChanged(); Rebuild(); }
                    }));

            _list.sizeDelta = new Vector2(0f, y + 40f);
            _list.anchoredPosition = Vector2.zero;

            // stagger the cells in
            for (int i = 0; i < _list.childCount; i++)
            {
                var c = (RectTransform)_list.GetChild(i);
                c.localScale = Vector3.one * 0.7f;
                Tween.Scale(c, Vector3.one, 0.35f, Ease.OutBack, 0.12f + i * 0.04f);
            }
        }

        void StarterBanner(UIProduct p, ref float y, float width)
        {
            var rt = UIFactory.Rect("Starter", _list);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(width, 300f);
            rt.anchoredPosition = new Vector2(0f, -y);
            var edge = UIFactory.Image(rt, "Edge", UISprites.Rounded, UITheme.Ink);
            edge.rectTransform.Stretch(-6f, -18f, -6f, -6f);
            var face = UIFactory.Image(rt, "Face", UISprites.Rounded, Color.white);
            face.rectTransform.Stretch();
            face.Gradient(UITheme.Hex("#FFE98A"), UITheme.Hex("#FFAA1F"));
            var rays = UIFactory.Image(rt, "Rays", UISprites.Rays, new Color(1, 1, 1, 0.5f));
            rays.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(140f, 0f), new Vector2(420f, 420f));
            Tween.Run(rays.rectTransform, "spin", 12f, Ease.Linear, k => rays.rectTransform.localRotation = Quaternion.Euler(0, 0, 360f * k), 0f, null, true);
            var gift = UIFactory.Icon(rt, UIIcon.Gift, UITheme.Red.Bottom, 180f);
            gift.rectTransform.anchorMin = gift.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            gift.rectTransform.anchoredPosition = new Vector2(140f, 0f);
            var ol = gift.gameObject.AddComponent<Outline>();
            ol.effectColor = UITheme.Ink;
            ol.effectDistance = new Vector2(5f, -5f);
            Tween.Run(gift.rectTransform, "wiggle", 2.2f, Ease.Linear,
                k => gift.rectTransform.localRotation = Quaternion.Euler(0, 0, k < 0.25f ? Mathf.Sin(k * 4f * Mathf.PI * 3f) * 8f : 0f), 0f, null, true);

            var title = UIFactory.Label(rt, p.Title, 64, Color.white, true, TextAnchor.UpperLeft);
            title.rectTransform.Stretch(270f, 140f, 260f, 30f);
            var desc = UIFactory.Label(rt, "No Ads + " + CoinCounter.Format(p.Coins) + " coins + boosters", 36,
                UITheme.Ink, false, TextAnchor.UpperLeft);
            desc.rectTransform.Stretch(270f, 50f, 260f, 130f);

            var buy = UIFactory.Button(rt, Price(p), UITheme.Green, new Vector2(230f, 130f), null, 50);
            ((RectTransform)buy.transform).Place(new Vector2(1f, 0.5f), new Vector2(-140f, 0f), new Vector2(230f, 130f));
            Wire(buy, p);
            buy.MakeCallToAction();

            if (!string.IsNullOrEmpty(p.Badge)) Badge(rt, p.Badge, new Vector2(1f, 1f), new Vector2(-140f, 0f));
            y += 300f + 50f;
        }

        RectTransform CoinCell(UIProduct p, Vector2 size, int index)
        {
            var rt = UIFactory.Rect("Pack_" + p.Id, _list);
            rt.sizeDelta = size;
            var edge = UIFactory.Image(rt, "Edge", UISprites.Rounded, UITheme.PaperDark);
            edge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var bg = UIFactory.Image(rt, "Bg", UISprites.Rounded, Color.white);
            bg.rectTransform.Stretch();
            bg.Gradient(UITheme.Hex("#FFFDF6"), UITheme.Hex("#FFF0D2"));
            var glow = UIFactory.Image(rt, "Glow", UISprites.Glow, UITheme.Gold.WithAlpha(0.7f));
            glow.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(220f, 220f));

            // more coins = a bigger pile
            int pile = Mathf.Clamp(1 + index, 1, 6);
            for (int i = 0; i < pile; i++)
            {
                float ang = i * 2.4f;
                var c = UIFactory.Coin(rt, 90f - pile * 4f);
                c.anchorMin = c.anchorMax = new Vector2(0.5f, 1f);
                c.anchoredPosition = new Vector2(Mathf.Cos(ang) * i * 9f, -110f + Mathf.Sin(ang) * i * 6f + i * 6f);
            }

            var amount = UIFactory.Label(rt, CoinCounter.Format(p.Coins), 50, UITheme.Gold);
            amount.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -205f), new Vector2(size.x - 10f, 60f));

            var buy = UIFactory.Button(rt, Price(p), UITheme.Green, new Vector2(size.x - 30f, 100f), null, 40);
            ((RectTransform)buy.transform).Place(new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(size.x - 30f, 100f));
            Wire(buy, p);

            if (!string.IsNullOrEmpty(p.Badge)) Badge(rt, p.Badge, new Vector2(0.5f, 1f), Vector2.zero);
            return rt;
        }

        void Row(ref float y, UIIcon icon, ButtonPalette color, string title, string desc, UIProduct p, System.Action onFree)
        {
            var rt = UIFactory.Rect("Row_" + title, _list);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 170f);
            rt.anchoredPosition = new Vector2(0f, -y);
            var edge = UIFactory.Image(rt, "Edge", UISprites.Rounded, UITheme.PaperDark);
            edge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var bg = UIFactory.Image(rt, "Bg", UISprites.Rounded, UITheme.Hex("#FFFAEF"));
            bg.rectTransform.Stretch();

            var badge = UIFactory.Rect("Icon", rt).Place(new Vector2(0f, 0.5f), new Vector2(90f, 0f), new Vector2(120f, 120f));
            var bEdge = UIFactory.Image(badge, "Edge", UISprites.RoundedSmall, UITheme.Ink);
            bEdge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var bFace = UIFactory.Image(badge, "Face", UISprites.RoundedSmall, Color.white);
            bFace.rectTransform.Stretch();
            bFace.Gradient(color.Top, color.Bottom);
            UIFactory.Icon(badge, icon, Color.white, 78f).rectTransform.anchoredPosition = Vector2.zero;

            var t = UIFactory.Label(rt, title, 48, UITheme.Ink, false, TextAnchor.LowerLeft);
            t.rectTransform.Stretch(180f, 84f, 290f, 20f);
            var d = UIFactory.Label(rt, desc, 30, UITheme.Ink.WithAlpha(0.7f), false, TextAnchor.UpperLeft);
            d.rectTransform.Stretch(180f, 18f, 290f, 92f);

            var buy = UIFactory.Button(rt, p != null ? Price(p) : "FREE", p != null ? UITheme.Green : UITheme.Purple,
                new Vector2(240f, 110f), p != null ? (UIIcon?)null : UIIcon.Video, 42);
            ((RectTransform)buy.transform).Place(new Vector2(1f, 0.5f), new Vector2(-145f, 4f), new Vector2(240f, 110f));
            if (p != null) Wire(buy, p);
            else buy.Clicked += onFree;
            y += 170f + 30f;
        }

        static void Badge(RectTransform parent, string text, Vector2 anchor, Vector2 pos)
        {
            var b = UIFactory.Rect("Badge", parent).Place(anchor, pos, new Vector2(210f, 62f));
            var edge = UIFactory.Image(b, "Edge", UISprites.Pill, UITheme.Ink);
            edge.rectTransform.Stretch(-4f, -4f, -4f, -4f);
            var face = UIFactory.Image(b, "Face", UISprites.Pill, Color.white);
            face.rectTransform.Stretch();
            face.Gradient(UITheme.Red.Top, UITheme.Red.Bottom);
            var t = UIFactory.Label(b, text, 32, Color.white);
            t.rectTransform.Stretch(10f, 0f, 10f, 0f);
            b.localRotation = Quaternion.Euler(0, 0, 5f);
        }

        static string Price(UIProduct p) => UIServices.GetPrice(p.Id) ?? p.FallbackPrice;

        void Wire(JuicyButton b, UIProduct p)
        {
            _buttons.Add((p, b));
            b.Clicked += () => UIServices.Purchase(p.Id, ok =>
            {
                if (!ok) return;
                UIServices.Sfx("ui_purchase");
                Root.Fx.Confetti(40);
                Root.Fx.Toast("Thank you!");
                UIServices.NotifyWalletChanged();
                Rebuild();
            });
        }

        void RefreshPrices()
        {
            foreach (var item in _buttons)
                if (item.b != null) item.b.SetText(Price(item.p));
        }
    }
}
