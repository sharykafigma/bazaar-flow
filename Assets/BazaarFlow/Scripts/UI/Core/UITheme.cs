using UnityEngine;

namespace BazaarFlow.UI
{
    /// <summary>A three-tone "candy" colour for chunky 3D buttons: top and bottom of the face, plus the darker lip.</summary>
    public struct ButtonPalette
    {
        public Color Top, Bottom, Lip, Text;

        /// <summary>Dark tint of the button's own hue, used for its outline and text stroke (glossy casual look).</summary>
        public Color Edge => new Color(Lip.r * 0.55f, Lip.g * 0.55f, Lip.b * 0.55f, 1f);

        public ButtonPalette(string top, string bottom, string lip, string text = "#FFFFFF")
        {
            Top = UITheme.Hex(top);
            Bottom = UITheme.Hex(bottom);
            Lip = UITheme.Hex(lip);
            Text = UITheme.Hex(text);
        }
    }

    /// <summary>
    /// Bazaar Flow UI look: warm truck-art bazaar colours, chunky candy buttons, cream paper cards with maroon ribbons.
    /// Every screen reads its colours and sizes from here, so a restyle is one file.
    /// </summary>
    public static class UITheme
    {
        // Reference resolution (portrait phone). CanvasScaler scales everything from this.
        public const float RefWidth = 1080f;
        public const float RefHeight = 1920f;

        // Base colours
        public static readonly Color Ink = Hex("#3B2416");          // outlines and dark text
        public static readonly Color Cream = Hex("#FFF6E3");
        public static readonly Color Paper = Hex("#FCE6BF");
        public static readonly Color PaperDark = Hex("#EFCB94");
        public static readonly Color Maroon = Hex("#8C1D3A");
        public static readonly Color MaroonDark = Hex("#5A0F24");
        public static readonly Color Saffron = Hex("#FFB21F");
        public static readonly Color Gold = Hex("#FFD447");
        public static readonly Color GoldDark = Hex("#D98E0B");
        public static readonly Color Teal = Hex("#14A3A0");
        public static readonly Color SkyTop = Hex("#FFB867");
        public static readonly Color SkyBottom = Hex("#FFE3B0");
        public static readonly Color Backdrop = new Color(0.10f, 0.04f, 0.06f, 0.72f);
        public static readonly Color Shadow = new Color(0.16f, 0.06f, 0.03f, 0.35f);

        // Button palettes
        public static readonly ButtonPalette Green = new ButtonPalette("#7BE07A", "#2FB24E", "#17793A");
        public static readonly ButtonPalette Orange = new ButtonPalette("#FFC85A", "#FF8A1F", "#C0570C");
        public static readonly ButtonPalette Red = new ButtonPalette("#FF8A78", "#E23B3B", "#9A1F2B");
        public static readonly ButtonPalette Teal3D = new ButtonPalette("#5BE3D3", "#16A6A2", "#0C6B6A");
        public static readonly ButtonPalette Purple = new ButtonPalette("#C3A2FF", "#8A5CE6", "#563296");
        public static readonly ButtonPalette Maroon3D = new ButtonPalette("#C9395E", "#8C1D3A", "#4E0C1F");
        public static readonly ButtonPalette Grey = new ButtonPalette("#E4DDD2", "#B8AFA2", "#80786C", "#5C5248");
        public static readonly ButtonPalette Cream3D = new ButtonPalette("#FFFDF5", "#F7E2BC", "#C9A16A", "#3B2416");

        // Confetti / festive accents (the six goods colours, brighter)
        public static readonly Color[] Festive =
        {
            Hex("#FF4D5E"), Hex("#3D8BFF"), Hex("#FFD447"), Hex("#3FD06B"), Hex("#B06BFF"), Hex("#FF9A2E")
        };

        // Sizes (reference pixels)
        public const float ButtonDepth = 14f;   // height of the 3D lip
        public const float CornerScale = 1f;    // corner radius multiplier for sliced sprites
        public const int TitleSize = 96;
        public const int HeaderSize = 64;
        public const int BodySize = 44;
        public const int ButtonTextSize = 58;
        public const int SmallSize = 34;

        // Motion (seconds)
        public const float PopIn = 0.42f;
        public const float PopOut = 0.2f;
        public const float Fade = 0.25f;

        static Font _font;

        /// <summary>True when the chunky display font loaded (it is already heavy, so no fake bold).</summary>
        public static bool HasDisplayFont { get; private set; }

        /// <summary>
        /// UI font: Resources/Fonts/BazaarFont.ttf (Lilita One, SIL Open Font License, free for commercial use).
        /// Falls back to Unity's built-in font if the file is missing.
        /// </summary>
        public static Font Font
        {
            get
            {
                if (_font != null) return _font;
                _font = Resources.Load<Font>("Fonts/BazaarFont");
                HasDisplayFont = _font != null;
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
