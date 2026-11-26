using UnityEngine;

namespace BazaarFlow
{
    /// <summary>The six goods colours. Each colour will later also get its own shape for colour-blind players.</summary>
    public enum ItemColor
    {
        Red,
        Blue,
        Yellow,
        Green,
        Purple,
        Orange
    }

    public static class GameColors
    {
        static readonly Color[] Palette =
        {
            new Color(0.91f, 0.26f, 0.24f), // Red    - chai cup
            new Color(0.22f, 0.49f, 0.92f), // Blue   - choori
            new Color(0.98f, 0.80f, 0.18f), // Yellow - aam
            new Color(0.27f, 0.75f, 0.36f), // Green  - patang
            new Color(0.62f, 0.36f, 0.86f), // Purple - lattoo
            new Color(0.98f, 0.55f, 0.16f), // Orange - diya
        };

        static readonly string[] Names = { "Laal", "Neela", "Peela", "Hara", "Jamni", "Narangi" };

        public const int Count = 6;

        public static Color Get(ItemColor c) => Palette[(int)c];
        public static string Name(ItemColor c) => Names[(int)c];

        public static readonly Color Background = new Color(0.98f, 0.93f, 0.84f);
        public static readonly Color BeltDark = new Color(0.30f, 0.24f, 0.22f);
        public static readonly Color BeltSlot = new Color(0.42f, 0.35f, 0.32f);
        public static readonly Color ShopBody = new Color(0.55f, 0.33f, 0.20f);
        public static readonly Color ShopDone = new Color(0.70f, 0.70f, 0.70f);
        public static readonly Color Hidden = new Color(0.45f, 0.45f, 0.50f);
        public static readonly Color Gate = new Color(0.20f, 0.16f, 0.15f);
    }
}
