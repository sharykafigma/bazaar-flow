using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>Shared builders for the sound / music / vibration rows used by Pause and Settings.</summary>
    public static class SettingRows
    {
        public static float Build(RectTransform parent, float top)
        {
            float y = top;
            Row(parent, ref y, UIIcon.Speaker, UITheme.Teal3D, "Sound", UISettings.Sound, v => UISettings.Sound = v);
            Row(parent, ref y, UIIcon.Note, UITheme.Purple, "Music", UISettings.Music, v => UISettings.Music = v);
            Row(parent, ref y, UIIcon.Vibrate, UITheme.Orange, "Vibration", UISettings.Haptics, v =>
            {
                UISettings.Haptics = v;
                if (v) UIServices.Haptic(HapticStrength.Medium);
            });
            return y;
        }

        static void Row(RectTransform parent, ref float y, UIIcon icon, ButtonPalette color, string label, bool on,
            System.Action<bool> set)
        {
            const float h = 140f;
            var row = UIFactory.Rect("Row_" + label, parent);
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(0f, h);
            row.anchoredPosition = new Vector2(0f, -y);
            var edge = UIFactory.Image(row, "Edge", UISprites.Rounded, UITheme.PaperDark);
            edge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var bg = UIFactory.Image(row, "Bg", UISprites.Rounded, UITheme.Hex("#FFFAEF"));
            bg.rectTransform.Stretch();

            var badge = UIFactory.Rect("Badge", row).Place(new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(96f, 96f));
            var bEdge = UIFactory.Image(badge, "Edge", UISprites.RoundedSmall, UITheme.Ink);
            bEdge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var bFace = UIFactory.Image(badge, "Face", UISprites.RoundedSmall, Color.white);
            bFace.rectTransform.Stretch();
            bFace.Gradient(color.Top, color.Bottom);
            var ic = UIFactory.Icon(badge, icon, Color.white, 64f);
            ic.rectTransform.anchoredPosition = Vector2.zero;

            var text = UIFactory.Label(row, label, UITheme.BodySize + 8, UITheme.Ink, false, TextAnchor.MiddleLeft);
            text.rectTransform.Stretch(160f, 0f, 220f, 0f);

            var sw = Switch(row);
            sw.Init(on, 42f);
            sw.Changed += set;
            y += h + 26f;
        }

        public static ToggleSwitch Switch(Transform parent)
        {
            var rt = UIFactory.Rect("Switch", parent).Place(new Vector2(1f, 0.5f), new Vector2(-110f, 0f), new Vector2(170f, 86f));
            var edge = UIFactory.Image(rt, "Edge", UISprites.Pill, UITheme.Ink, true);
            edge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var track = UIFactory.Image(rt, "Track", UISprites.Pill, Color.white);
            track.rectTransform.Stretch();
            var knob = UIFactory.Rect("Knob", rt).Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70f, 70f));
            var kEdge = UIFactory.Image(knob, "Edge", UISprites.Circle, UITheme.Ink);
            kEdge.rectTransform.Stretch(-4f, -4f, -4f, -4f);
            var kFace = UIFactory.Image(knob, "Face", UISprites.Circle, Color.white);
            kFace.rectTransform.Stretch();
            kFace.Gradient(Color.white, UITheme.Hex("#E8E0D4"));
            var sw = rt.gameObject.AddComponent<ToggleSwitch>();
            sw.Track = track.Gradient(Color.white, Color.white);
            sw.Knob = knob;
            return sw;
        }
    }

    /// <summary>Pause popup: settings toggles, Resume, Restart, Home.</summary>
    public class PauseScreen : UIScreen
    {
        protected override bool IsPopup => true;
        JuicyButton _resume;

        protected override void Build()
        {
            var content = UIFactory.Card(Safe, "PAUSED", new Vector2(900f, 1120f), out var card);
            card.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(900f, 1120f));
            PopTarget = card;
            AddCloseButton(card, () => Root.ResumeGame());

            float y = SettingRows.Build(content, 10f);

            _resume = UIFactory.Button(content, "RESUME", UITheme.Green, new Vector2(700f, 170f), UIIcon.Play);
            ((RectTransform)_resume.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -y - 100f), new Vector2(700f, 170f));
            _resume.Clicked += () => Root.ResumeGame();

            var restart = UIFactory.Button(content, "Restart", UITheme.Orange, new Vector2(340f, 150f), UIIcon.Retry, 50);
            ((RectTransform)restart.transform).Place(new Vector2(0.5f, 1f), new Vector2(-180f, -y - 300f), new Vector2(340f, 150f));
            restart.Clicked += () => Root.Restart();

            var home = UIFactory.Button(content, "Home", UITheme.Cream3D, new Vector2(340f, 150f), UIIcon.Home, 50);
            ((RectTransform)home.transform).Place(new Vector2(0.5f, 1f), new Vector2(180f, -y - 300f), new Vector2(340f, 150f));
            home.Clicked += () => Root.ShowMenu();
        }

        protected override void OnShow() => _resume.MakeCallToAction();
    }

    /// <summary>Settings popup (from the main menu): toggles, restore purchases, privacy policy, version.</summary>
    public class SettingsScreen : UIScreen
    {
        protected override bool IsPopup => true;

        protected override void Build()
        {
            var content = UIFactory.Card(Safe, "SETTINGS", new Vector2(900f, 1180f), out var card);
            card.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 1180f));
            PopTarget = card;
            AddCloseButton(card, () => Hide());

            float y = SettingRows.Build(content, 10f);

            var restore = UIFactory.Button(content, "Restore Purchases", UITheme.Teal3D, new Vector2(700f, 140f), null, 48);
            ((RectTransform)restore.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -y - 80f), new Vector2(700f, 140f));
            restore.Clicked += () => UIServices.RestorePurchases(ok =>
            {
                Root.Fx.Toast(ok ? "Purchases restored!" : "Nothing to restore");
                if (ok) UIServices.NotifyWalletChanged();
            });

            var privacy = UIFactory.Button(content, "Privacy Policy", UITheme.Cream3D, new Vector2(700f, 130f), null, 44);
            ((RectTransform)privacy.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -y - 260f), new Vector2(700f, 130f));
            privacy.Clicked += () => UIServices.OpenPrivacyPolicy?.Invoke();

            var version = UIFactory.Label(content, "Bazaar Flow  v" + Application.version, UITheme.SmallSize,
                UITheme.Ink.WithAlpha(0.55f), false);
            version.rectTransform.HStretch(0f, 30f, 60f, 0f);
        }
    }
}
