using System.Collections.Generic;

namespace BazaarFlow
{
    /// <summary>The four boosters from the design doc.</summary>
    public enum BoosterType
    {
        ExtraPatri, // +4 belt slots
        Jhaaru,     // sweep one colour off the belt back to the godown
        Chashma,    // reveal all hidden crates
        UltiBelt    // belt runs backwards for one lap
    }

    public static class Boosters
    {
        public const int Count = 4;

        static readonly string[] Names = { "Extra Patri", "Jhaaru", "Chashma", "Ulti Belt" };
        static readonly string[] Info =
        {
            "Belt pe 4 khane aur",
            "Ek rang ka maal wapas godown",
            "Saare chhupe crate dikhao",
            "Belt ek chakkar ulta",
        };

        public static string Name(BoosterType b) => Names[(int)b];
        public static string Description(BoosterType b) => Info[(int)b];
    }

    public enum ProductKind { NoAds, StarterPack, Coins, BoosterBundle }

    /// <summary>One real-money product and what it gives.</summary>
    public class ShopProduct
    {
        public string Id;
        public ProductKind Kind;
        public bool Consumable;
        public string Title;
        public string FallbackPrice; // shown until Google Play returns the local price
        public int Coins;

        public ShopProduct(string id, ProductKind kind, bool consumable, string title, string price, int coins = 0)
        {
            Id = id;
            Kind = kind;
            Consumable = consumable;
            Title = title;
            FallbackPrice = price;
            Coins = coins;
        }
    }

    public static class ShopCatalog
    {
        public static readonly List<ShopProduct> Products = new List<ShopProduct>
        {
            new ShopProduct(MonetizationConfig.ProductNoAds, ProductKind.NoAds, false, "No Ads", "$3.99"),
            new ShopProduct(MonetizationConfig.ProductStarterPack, ProductKind.StarterPack, false, "Starter Pack", "$1.99",
                MonetizationConfig.StarterPackCoins),
            new ShopProduct(MonetizationConfig.ProductBoosterBundle, ProductKind.BoosterBundle, true, "Booster Bundle", "$2.99"),
            new ShopProduct(MonetizationConfig.ProductCoins1, ProductKind.Coins, true, "Mutthi bhar sikke", "$0.99", 300),
            new ShopProduct(MonetizationConfig.ProductCoins2, ProductKind.Coins, true, "Thaili", "$2.99", 1000),
            new ShopProduct(MonetizationConfig.ProductCoins3, ProductKind.Coins, true, "Bara thaila", "$4.99", 1800),
            new ShopProduct(MonetizationConfig.ProductCoins4, ProductKind.Coins, true, "Sandooq", "$9.99", 4000),
            new ShopProduct(MonetizationConfig.ProductCoins5, ProductKind.Coins, true, "Khazana", "$14.99", 6500),
            new ShopProduct(MonetizationConfig.ProductCoins6, ProductKind.Coins, true, "Seth ka khazana", "$19.99", 9000),
        };

        public static ShopProduct Get(string id)
        {
            foreach (var p in Products) if (p.Id == id) return p;
            return null;
        }

        /// <summary>Gives the player what a product contains. Called once per purchase (IapService de-duplicates).</summary>
        public static void Grant(ShopProduct p)
        {
            switch (p.Kind)
            {
                case ProductKind.NoAds:
                    Economy.SetNoAds();
                    break;
                case ProductKind.StarterPack:
                    Economy.SetNoAds();
                    Economy.Data.starterPackBought = true;
                    Economy.AddCoins(p.Coins);
                    foreach (var b in Economy.LiveBoosters)
                        Economy.AddBooster(b, MonetizationConfig.StarterPackBoostersEach);
                    break;
                case ProductKind.BoosterBundle:
                    foreach (var b in Economy.LiveBoosters)
                        Economy.AddBooster(b, MonetizationConfig.BoosterBundleEach);
                    break;
                case ProductKind.Coins:
                    Economy.AddCoins(p.Coins);
                    break;
            }
            Economy.Save();
        }
    }
}
