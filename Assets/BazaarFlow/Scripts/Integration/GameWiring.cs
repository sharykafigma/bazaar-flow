using System;
using System.Collections.Generic;
using BazaarFlow.UI;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Connects the modules at startup: the UI talks to ads, purchases, wallet, sound and progress only through
    /// UIServices, and this is the one place that plugs the real systems in. Runs before any scene loads.
    /// </summary>
    public static class GameWiring
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Wire()
        {
            // ---- ads
            UIServices.ShowRewarded = AdsService.ShowRewarded;
            UIServices.IsNoAds = () => Economy.NoAds;
            UIServices.BeforeNextLevel = (level, next) => AdsService.OnLevelWon(level, next);
            UIServices.OnRetryAfterLoss = AdsService.OnLevelLost;

            // ---- purchases
            UIServices.Purchase = IapService.Purchase;
            UIServices.RestorePurchases = IapService.Restore;
            UIServices.GetPrice = IapService.GetPrice;
            UIServices.IsStarterOfferActive = () => Economy.StarterOfferActive;
            UIServices.GetProducts = Products;

            // ---- wallet and boosters
            UIServices.GetCoins = () => Economy.Coins;
            UIServices.AddCoins = Economy.AddCoins;
            UIServices.TrySpendCoins = Economy.TrySpendCoins;
            UIServices.GetBoosterCount = id => TryBooster(id, out var b) ? Economy.BoosterCount(b) : 0;
            UIServices.BuyBoosterWithCoins = id => TryBooster(id, out var b) && Economy.TryBuyBoosterWithCoins(b);
            UIServices.IsBoosterLive = id => TryBooster(id, out var b) && Economy.IsLive(b);
            UIServices.FreeBoosterAvailable = () => Economy.FreeBoosterAvailable;
            Economy.Changed += UIServices.NotifyWalletChanged;
            Lives.Changed += UIServices.NotifyWalletChanged;

            // ---- progress and HUD
            UIServices.GetUnlockedLevel = () => Progress.UnlockedLevel;
            UIServices.GetBeltFill = () =>
            {
                var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
                return gm != null && gm.LevelNumber > 0 ? gm.BeltFill : -1f;
            };

            // ---- sound: the uGUI layer replaces the audio module's debug buttons and the OnGUI shop
            UIServices.PlaySfx = AudioManager.PlayUi;
            AudioManager.ShowPrototypeButtons = false;
        }

        static bool TryBooster(string id, out BoosterType b) => Enum.TryParse(id, out b);

        static List<UIProduct> Products()
        {
            var list = new List<UIProduct>();
            foreach (var p in ShopCatalog.Products)
            {
                if (p.Kind == ProductKind.StarterPack && !Economy.StarterOfferActive) continue;
                if (p.Kind == ProductKind.NoAds && Economy.NoAds) continue;
                string badge = null;
                if (p.Kind == ProductKind.StarterPack) badge = "-70%";
                else if (p.Id == MonetizationConfig.ProductCoins2) badge = "Popular";
                else if (p.Id == MonetizationConfig.ProductCoins5) badge = "Best Value";
                list.Add(new UIProduct(p.Id, p.Title, p.Coins, p.FallbackPrice, badge));
            }
            return list;
        }
    }
}
