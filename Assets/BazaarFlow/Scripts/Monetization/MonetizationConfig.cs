namespace BazaarFlow
{
    /// <summary>
    /// ONE place for every ad / store ID and every monetization number.
    /// Swap the TEST values below for your real ones before release (see Docs/MONETIZATION_SETUP.md).
    /// </summary>
    public static class MonetizationConfig
    {
        // ---------------------------------------------------------------
        // LevelPlay (ads). TEST values = LevelPlay's public demo app.
        // Real values: LevelPlay dashboard > Apps (app key) and > Ad units.
        // In the Unity Editor any value gives mock ads (build target must be Android).
        // ---------------------------------------------------------------
        public const string LevelPlayAppKey = "85460dcd";               // TODO: your Android app key
        public const string RewardedAdUnitId = "76yy3nay3ceui2a3";      // TODO: your rewarded ad unit id
        public const string InterstitialAdUnitId = "aeyqi3vqlv6o8sh9";  // TODO: your interstitial ad unit id
        public const string BannerAdUnitId = "thnfvcsog13bhn08";        // TODO: your banner ad unit id

        /// <summary>
        /// Opens the LevelPlay Test Suite on device after init. Turn OFF before the Play Store release.
        /// </summary>
        public const bool LaunchLevelPlayTestSuite = false;

        // ---------------------------------------------------------------
        // Ad rules (from the design doc). Change here, nowhere else.
        // ---------------------------------------------------------------
        /// <summary>No interstitial before this level.</summary>
        public const int InterstitialFirstLevel = 8;
        /// <summary>Show an interstitial after every Nth win.</summary>
        public const int InterstitialEveryNWins = 2;
        /// <summary>Minimum seconds between two interstitials.</summary>
        public const float InterstitialMinGapSeconds = 90f;
        /// <summary>Skip interstitials right after this many losses in a row (frustrated player).</summary>
        public const int InterstitialSkipAfterLosses = 2;
        /// <summary>Design doc says no banner. Set true to show a small banner on the win/jam screens only.</summary>
        public const bool BannerEnabled = false;

        // ---------------------------------------------------------------
        // Google Play product IDs. Create these EXACT ids in Play Console > Monetize > Products.
        // Prices are set in Play Console; the numbers here are only the plan.
        // ---------------------------------------------------------------
        public const string ProductNoAds = "bf_no_ads";              // $3.99, non-consumable
        public const string ProductStarterPack = "bf_starter_pack";  // $1.99, non-consumable (one time)
        public const string ProductBoosterBundle = "bf_booster_bundle"; // $2.99, consumable
        public const string ProductCoins1 = "bf_coins_1";            // $0.99
        public const string ProductCoins2 = "bf_coins_2";            // $2.99
        public const string ProductCoins3 = "bf_coins_3";            // $4.99
        public const string ProductCoins4 = "bf_coins_4";            // $9.99
        public const string ProductCoins5 = "bf_coins_5";            // $14.99
        public const string ProductCoins6 = "bf_coins_6";            // $19.99

        // ---------------------------------------------------------------
        // Economy numbers.
        // ---------------------------------------------------------------
        public const int BoosterCoinPrice = 150;
        public const int StarterPackCoins = 1000;
        public const int StarterPackBoostersEach = 1;   // design: "3 boosters" -> 1 of each live booster
        public const int BoosterBundleEach = 3;
        public const float StarterPackHours = 72f;
        public const int NoAdsOfferFromLevel = 15;

        /// <summary>Rewarded "daily free booster": one per calendar day.</summary>
        public const bool DailyFreeBoosterEnabled = true;
    }
}
