using System;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow.UI
{
    /// <summary>One row in the shop screen.</summary>
    public class UIProduct
    {
        public string Id;
        public string Title;
        public int Coins;            // 0 for non-coin products
        public string FallbackPrice; // shown until the store returns a real localized price
        public string Badge;         // e.g. "Popular", or null

        public UIProduct(string id, string title, int coins, string fallbackPrice, string badge = null)
        {
            Id = id; Title = title; Coins = coins; FallbackPrice = fallbackPrice; Badge = badge;
        }
    }

    /// <summary>
    /// The seam between the UI and the other systems (ads, IAP, wallet, sound, haptics, gameplay data).
    /// Every member has an editor-safe default, so the UI runs on its own. Other modules replace the delegates at
    /// startup (see MonetizationBootstrap in the monetization branch) without the UI knowing their types.
    /// </summary>
    public static class UIServices
    {
        // -------------------------------------------------------------- ads

        /// <summary>Show a rewarded ad. Placements: "continue", "double_coins", "free_booster". Default: grants at once.</summary>
        public static Action<string, Action<bool>> ShowRewarded = (placement, done) =>
        {
            Debug.Log($"[UIServices] Rewarded '{placement}' (no ad SDK hooked up, granting).");
            done?.Invoke(true);
        };

        public static Func<bool> IsNoAds = () => PlayerPrefs.GetInt("bf_noads", 0) == 1;

        // -------------------------------------------------------------- IAP

        public static Action<string, Action<bool>> Purchase = (id, done) =>
        {
            Debug.Log($"[UIServices] Purchase '{id}' (no IAP hooked up).");
            done?.Invoke(false);
        };

        public static Action<Action<bool>> RestorePurchases = done => done?.Invoke(false);

        /// <summary>Localized price from the store, or null to use the product's fallback price.</summary>
        public static Func<string, string> GetPrice = id => null;

        public static Func<bool> IsStarterOfferActive = () => true;

        /// <summary>Shop rows. Ids match the Play Console products in MonetizationConfig.</summary>
        public static Func<List<UIProduct>> GetProducts = () => new List<UIProduct>
        {
            new UIProduct("bf_starter_pack", "Starter Pack", 1500, "Rs 250", "-70%"),
            new UIProduct("bf_no_ads", "No Ads", 0, "Rs 450"),
            new UIProduct("bf_booster_bundle", "Booster Bundle", 0, "Rs 350"),
            new UIProduct("bf_coins_1", "Handful", 500, "Rs 150"),
            new UIProduct("bf_coins_2", "Pouch", 1200, "Rs 300", "Popular"),
            new UIProduct("bf_coins_3", "Sack", 2600, "Rs 600"),
            new UIProduct("bf_coins_4", "Chest", 5500, "Rs 1,100"),
            new UIProduct("bf_coins_5", "Vault", 12000, "Rs 2,200", "Best Value"),
            new UIProduct("bf_coins_6", "Treasure", 30000, "Rs 5,000"),
        };

        // -------------------------------------------------------------- wallet

        /// <summary>Coins. Default reads GameManager / PlayerPrefs.</summary>
        public static Func<int> GetCoins = () =>
        {
            var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            return gm != null ? gm.Coins : PlayerPrefs.GetInt("bf_coins", 0);
        };

        /// <summary>Add coins (x2 reward, free coins). Null = feature hidden until the wallet module hooks it.</summary>
        public static Action<int> AddCoins;

        /// <summary>Spend coins (coin continue). Null = feature hidden.</summary>
        public static Func<int, bool> TrySpendCoins;

        public static Func<string, int> GetBoosterCount = id => 0;
        public static Func<string, bool> BuyBoosterWithCoins = id => false;

        /// <summary>Booster ids are the BoosterType names: ExtraPatri, Jhaaru, Chashma, UltiBelt.</summary>
        public static Func<string, bool> IsBoosterLive = id => id == "Jhaaru" || id == "Chashma";

        /// <summary>Whether the once-a-day free booster (rewarded ad) row is shown in the shop.</summary>
        public static Func<bool> FreeBoosterAvailable = () => true;

        /// <summary>Raised when coins, no-ads or boosters change. Call NotifyWalletChanged from the economy.</summary>
        public static event Action WalletChanged;
        public static void NotifyWalletChanged() => WalletChanged?.Invoke();

        // -------------------------------------------------------------- progress

        /// <summary>Highest level the player may start.</summary>
        public static Func<int> GetUnlockedLevel = () => Mathf.Max(1, PlayerPrefs.GetInt("bf_level", 1));

        /// <summary>Optional belt fill 0..1 for the HUD meter; return a negative value to hide it.</summary>
        public static Func<float> GetBeltFill = () => -1f;

        // -------------------------------------------------------------- feedback

        /// <summary>
        /// UI sound ids: ui_tap, ui_back, ui_popup, ui_close, ui_toggle, ui_win, ui_star, ui_coin, ui_jam, ui_purchase, ui_locked.
        /// The sound module assigns this; it should respect UISettings.Sound.
        /// </summary>
        public static Action<string> PlaySfx = id => { };

        public static void Sfx(string id)
        {
            if (UISettings.Sound) PlaySfx?.Invoke(id);
        }

        /// <summary>Light haptic tick for button presses.</summary>
        public static void Haptic(HapticStrength strength = HapticStrength.Light)
        {
            if (UISettings.Haptics) Haptics.Play(strength);
        }

        public static Action OpenPrivacyPolicy = () => Application.OpenURL("https://sharykafigma.github.io/bazaar-flow/privacy-policy.html");
    }
}
