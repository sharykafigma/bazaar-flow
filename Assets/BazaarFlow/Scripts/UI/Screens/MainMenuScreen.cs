using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>
    /// Title screen: striped market awning, swaying bunting, the BELT JAM: Conveyor Sort logo over slow sun rays,
    /// a little market stall, and a big breathing PLAY button.
    /// </summary>
    public class MainMenuScreen : UIScreen
    {
        RectTransform _logo, _rays, _stall, _bunting, _nav, _tabs;
        JuicyButton _play, _map, _shop, _noAds;

        protected override void Build()
        {
            // Background sky gradient (full bleed, behind the safe area)
            var bg = UIFactory.Image(Rt, "Sky", UISprites.Square, Color.white, true);
            bg.rectTransform.Stretch();
            bg.Gradient(UITheme.SkyTop, UITheme.SkyBottom);
            bg.transform.SetAsFirstSibling();

            _rays = UIFactory.Image(Rt, "Rays", UISprites.Rays, UITheme.Gold.WithAlpha(0.45f)).rectTransform;
            _rays.Place(new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(1900f, 1900f));
            _rays.SetSiblingIndex(1);

            BuildAwning();
            BuildBunting();

            // Top bar
            var coins = CoinCounter.Create(Safe, new Vector2(330f, 92f));
            ((RectTransform)coins.transform).Place(new Vector2(1f, 1f), new Vector2(-210f, -250f), new Vector2(330f, 92f));

            // Logo
            _logo = UIFactory.Rect("Logo", Safe).Place(new Vector2(0.5f, 0.64f), Vector2.zero, new Vector2(960f, 420f));
            var bazaar = UIFactory.Title(_logo, "BELT JAM", 210, UITheme.Gold);
            bazaar.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(960f, 240f));
            bazaar.GetComponent<UIGradient>().Set(UITheme.Hex("#FFF2A8"), UITheme.Hex("#FFA51F"));
            var flowTag = UIFactory.Rect("FlowTag", _logo).Place(new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(760f, 140f));
            var tagEdge = UIFactory.Image(flowTag, "Edge", UISprites.Rounded, UITheme.Ink);
            tagEdge.rectTransform.Stretch(-8f, -22f, -8f, -8f);
            var tagFace = UIFactory.Image(flowTag, "Face", UISprites.Rounded, Color.white);
            tagFace.rectTransform.Stretch();
            tagFace.Gradient(UITheme.Teal3D.Top, UITheme.Teal3D.Bottom);
            var flow = UIFactory.Title(flowTag, "CONVEYOR SORT", 96, Color.white);
            flow.rectTransform.Stretch(0f, 4f, 0f, 0f);
            _logo.localRotation = Quaternion.Euler(0, 0, 4f);

            BuildStall();

            // Buttons
            _play = UIFactory.Button(Safe, "PLAY", UITheme.Green, new Vector2(700f, 220f), UIIcon.Play, 100);
            ((RectTransform)_play.transform).Place(new Vector2(0.5f, 0f), new Vector2(0f, 470f), new Vector2(700f, 220f));
            _play.Clicked += () => Root.Play();
            _play.MakeCallToAction();

            BuildNavBar();
        }

        /// <summary>Bottom tab bar: Shop, Map, Home (raised, selected), No Ads, Settings.</summary>
        void BuildNavBar()
        {
            _nav = UIFactory.Rect("NavBar", Rt);
            _nav.anchorMin = new Vector2(0f, 0f);
            _nav.anchorMax = new Vector2(1f, 0f);
            _nav.pivot = new Vector2(0.5f, 0f);
            _nav.sizeDelta = new Vector2(0f, 250f);
            _nav.anchoredPosition = Vector2.zero;
            var edge = UIFactory.Image(_nav, "Edge", UISprites.Square, UITheme.Ink);
            edge.rectTransform.Stretch(0f, 0f, 0f, -8f);
            var wood = UIFactory.Image(_nav, "Wood", UISprites.Square, Color.white, true);
            wood.rectTransform.Stretch();
            wood.Gradient(UITheme.Hex("#9C2A4A"), UITheme.Hex("#5A0F24"));
            var gloss = UIFactory.Image(_nav, "Gloss", UISprites.Square, new Color(1, 1, 1, 0.12f));
            gloss.rectTransform.Stretch(0f, 170f, 0f, 0f);
            // gold trim along the top edge
            var trim = UIFactory.Image(_nav, "Trim", UISprites.Square, UITheme.Gold);
            trim.rectTransform.anchorMin = new Vector2(0f, 1f);
            trim.rectTransform.anchorMax = new Vector2(1f, 1f);
            trim.rectTransform.sizeDelta = new Vector2(0f, 8f);
            trim.rectTransform.anchoredPosition = new Vector2(0f, -10f);

            var tabs = UIFactory.Rect("Tabs", _nav);
            tabs.Stretch();
            _tabs = tabs;
            _shop = Tab(tabs, 0, UIIcon.Bag, "Shop", UITheme.Purple);
            _shop.Clicked += () => Root.OpenShop();
            _map = Tab(tabs, 1, UIIcon.Map, "Map", UITheme.Orange);
            _map.Clicked += () => Root.ShowMap();
            var home = Tab(tabs, 2, UIIcon.Home, "Home", UITheme.Green);
            home.Clicked += () => Tween.Punch(_logo, 0.08f, 0.4f);
            _noAds = Tab(tabs, 3, UIIcon.Shield, "No Ads", UITheme.Red);
            _noAds.Clicked += () => UIServices.Purchase("bf_no_ads", ok =>
            {
                if (ok) { Root.Fx.Toast("Ads removed. Thank you!"); UIServices.NotifyWalletChanged(); OnShow(); }
            });
            var settings = Tab(tabs, 4, UIIcon.Gear, "Settings", UITheme.Teal3D);
            settings.Clicked += () => Root.OpenSettings();
        }

        JuicyButton Tab(RectTransform parent, int index, UIIcon icon, string caption, ButtonPalette p)
        {
            bool selected = index == 2;
            float size = selected ? 200f : 150f;
            var b = UIFactory.Button(parent, null, p, new Vector2(size, size), icon);
            var rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = new Vector2((index + 0.5f) / 5f, 1f);
            rt.anchoredPosition = new Vector2(0f, selected ? -40f : -105f);
            rt.sizeDelta = new Vector2(size, size);
            if (b.IconImage != null) b.IconImage.rectTransform.sizeDelta = new Vector2(size * 0.55f, size * 0.55f);
            var cap = UIFactory.Label(rt, caption, selected ? 40 : 34, Color.white, UITheme.MaroonDark);
            cap.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0f, -40f), new Vector2(230f, 50f));
            return b;
        }

        void BuildAwning()
        {
            var awning = UIFactory.Rect("Awning", Rt);
            awning.anchorMin = new Vector2(0f, 1f);
            awning.anchorMax = new Vector2(1f, 1f);
            awning.pivot = new Vector2(0.5f, 1f);
            awning.sizeDelta = new Vector2(0f, 260f);
            awning.anchoredPosition = Vector2.zero;
            const int stripes = 9;
            for (int i = 0; i < stripes; i++)
            {
                var c = i % 2 == 0 ? UITheme.Maroon : UITheme.Cream;
                var s = UIFactory.Image(awning, "Stripe", UISprites.Square, c);
                var rt = s.rectTransform;
                rt.anchorMin = new Vector2((float)i / stripes, 0f);
                rt.anchorMax = new Vector2((float)(i + 1) / stripes, 1f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                // scalloped edge
                var scallop = UIFactory.Image(awning, "Scallop", UISprites.Circle, c);
                var srt = scallop.rectTransform;
                srt.anchorMin = srt.anchorMax = new Vector2((i + 0.5f) / stripes, 0f);
                srt.sizeDelta = new Vector2(1080f / stripes, 1080f / stripes * 0.9f);
                srt.anchoredPosition = Vector2.zero;
            }
            var shade = UIFactory.Image(awning, "Shade", UISprites.Square, Color.white);
            shade.rectTransform.Stretch();
            shade.Gradient(new Color(1, 1, 1, 0.15f), new Color(0, 0, 0, 0.12f));
            var trim = UIFactory.Image(awning, "Trim", UISprites.Square, UITheme.Ink);
            trim.rectTransform.anchorMin = new Vector2(0f, 1f);
            trim.rectTransform.anchorMax = new Vector2(1f, 1f);
            trim.rectTransform.sizeDelta = new Vector2(0f, 10f);
        }

        void BuildBunting()
        {
            _bunting = UIFactory.Rect("Bunting", Rt);
            _bunting.anchorMin = new Vector2(0f, 1f);
            _bunting.anchorMax = new Vector2(1f, 1f);
            _bunting.pivot = new Vector2(0.5f, 1f);
            _bunting.anchoredPosition = new Vector2(0f, -400f);
            _bunting.sizeDelta = new Vector2(0f, 120f);
            var rope = UIFactory.Image(_bunting, "Rope", UISprites.Square, UITheme.Ink);
            rope.rectTransform.anchorMin = new Vector2(0f, 1f);
            rope.rectTransform.anchorMax = new Vector2(1f, 1f);
            rope.rectTransform.sizeDelta = new Vector2(80f, 6f);
            const int n = 10;
            for (int i = 0; i < n; i++)
            {
                var flag = UIFactory.Rect("Flag", _bunting);
                flag.anchorMin = flag.anchorMax = new Vector2((i + 0.5f) / n, 1f);
                flag.pivot = new Vector2(0.5f, 1f);
                flag.sizeDelta = new Vector2(84f, 104f);
                flag.anchoredPosition = Vector2.zero;
                var edge = UIFactory.Image(flag, "Edge", UISprites.Pennant, UITheme.Ink);
                edge.preserveAspect = false;
                edge.rectTransform.Stretch(-5f, -8f, -5f, 0f);
                var face = UIFactory.Image(flag, "Face", UISprites.Pennant, UITheme.Festive[i % UITheme.Festive.Length]);
                face.preserveAspect = false;
                face.rectTransform.Stretch();
                float phase = i * 0.7f;
                Tween.Run(flag, "sway", 2.4f, Ease.Linear,
                    k => flag.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * Mathf.PI * 2f + phase) * 7f), 0f, null, true);
            }
            _bunting.localRotation = Quaternion.Euler(0, 0, -2f);
        }

        void BuildStall()
        {
            _stall = UIFactory.Rect("Stall", Safe).Place(new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(560f, 330f));
            var body = UIFactory.Image(_stall, "BodyEdge", UISprites.Rounded, UITheme.Ink);
            body.rectTransform.Stretch(30f, -6f, 30f, 70f);
            var wood = UIFactory.Image(_stall, "Wood", UISprites.Rounded, Color.white);
            wood.rectTransform.Stretch(38f, 2f, 38f, 78f);
            wood.Gradient(UITheme.Hex("#A8693F"), UITheme.Hex("#7A4526"));
            var roofEdge = UIFactory.Image(_stall, "RoofEdge", UISprites.Rounded, UITheme.Ink);
            roofEdge.rectTransform.anchorMin = new Vector2(0f, 1f);
            roofEdge.rectTransform.anchorMax = new Vector2(1f, 1f);
            roofEdge.rectTransform.pivot = new Vector2(0.5f, 1f);
            roofEdge.rectTransform.sizeDelta = new Vector2(8f, 118f);
            roofEdge.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            const int stripes = 7;
            for (int i = 0; i < stripes; i++)
            {
                var s = UIFactory.Image(_stall, "RoofStripe", UISprites.RoundedSmall, i % 2 == 0 ? UITheme.Saffron : UITheme.Cream);
                var rt = s.rectTransform;
                rt.anchorMin = new Vector2((float)i / stripes, 1f);
                rt.anchorMax = new Vector2((float)(i + 1) / stripes, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.offsetMin = new Vector2(i == 0 ? 4f : 0f, -106f);
                rt.offsetMax = new Vector2(i == stripes - 1 ? -4f : 0f, -2f);
            }
            // goods on the counter: one of each colour, bobbing
            for (int i = 0; i < 5; i++)
            {
                var good = UIFactory.Rect("Good", _stall).Place(new Vector2(0.5f, 0f), new Vector2(-180f + i * 90f, 120f), new Vector2(76f, 76f));
                var e = UIFactory.Image(good, "Edge", UISprites.Circle, UITheme.Ink);
                e.rectTransform.Stretch(-5f, -5f, -5f, -5f);
                var f = UIFactory.Image(good, "Fill", UISprites.Circle, UITheme.Festive[i]);
                f.rectTransform.Stretch();
                var hi = UIFactory.Image(good, "Hi", UISprites.Circle, new Color(1, 1, 1, 0.5f));
                hi.rectTransform.Place(new Vector2(0.35f, 0.7f), Vector2.zero, new Vector2(20f, 14f));
                float phase = i * 0.9f;
                Vector2 home = good.anchoredPosition;
                Tween.Run(good, "bob", 1.6f, Ease.Linear,
                    k => good.anchoredPosition = home + new Vector2(0f, Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f + phase)) * 18f), 0f, null, true);
            }
        }

        void Update()
        {
            // Lift the tabs above the gesture bar: the bar's colour runs to the screen edge, the buttons don't.
            if (Screen.height <= 0) return;
            float inset = Screen.safeArea.y / Screen.height * Rt.rect.height;
            _nav.sizeDelta = new Vector2(0f, 250f + inset);
            _tabs.offsetMin = new Vector2(0f, inset);
        }

        protected override void OnShow()
        {
            int level = UIServices.GetUnlockedLevel();
            if (Root.Game != null && Root.Game.LevelNumber > level) level = Root.Game.LevelNumber;
            _play.SetText("PLAY  " + level);
            _noAds.gameObject.SetActive(!UIServices.IsNoAds());

            // entrance: logo drops in, stall and buttons pop one after another
            Vector2 logoHome = new Vector2(0f, 0f);
            _logo.anchoredPosition = logoHome + new Vector2(0f, 500f);
            Tween.Move(_logo, logoHome, 0.7f, Ease.OutBounce, 0.05f);
            Pop(_stall, 0.25f);
            Pop((RectTransform)_play.transform.GetChild(0), 0.4f);
            _nav.anchoredPosition = new Vector2(0f, -300f);
            Tween.Move(_nav, Vector2.zero, 0.45f, Ease.OutBack, 0.3f);
            Tween.Run(_rays, "spin", 40f, Ease.Linear, k => _rays.localRotation = Quaternion.Euler(0, 0, -360f * k), 0f, null, true);
        }

        static void Pop(RectTransform rt, float delay)
        {
            rt.localScale = Vector3.zero;
            Tween.Scale(rt, Vector3.one, 0.45f, Ease.OutBack, delay);
        }
    }
}
