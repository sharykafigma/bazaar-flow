using System;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>Everything the player owns, saved as one JSON blob.</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int coins;
        public int[] boosters = new int[Boosters.Count];
        public bool noAds;
        public bool starterPackBought;

        /// <summary>UTC ticks of the first loss; the Starter Pack offer runs 72 hours from then.</summary>
        public long starterOfferStartTicks;

        /// <summary>Day number (UTC days since 2000) when the free daily booster was last claimed.</summary>
        public int lastFreeBoosterDay = -1;

        // Ad pacing that should survive restarts.
        public int winsSinceInterstitial;
        public int lossStreak;

        /// <summary>IAP transaction ids already granted, so a re-delivered purchase is never given twice.</summary>
        public List<string> grantedTransactions = new List<string>();
    }

    /// <summary>
    /// Wallet + save system. Static so any script can read coins and boosters.
    /// Stored in PlayerPrefs ("bf_save"). Level progress stays in "bf_level" (owned by GameManager).
    /// </summary>
    public static class Economy
    {
        const string SaveKey = "bf_save";
        const string LegacyCoinsKey = "bf_coins";
        const int MaxTransactionsKept = 200;

        static SaveData _data;

        /// <summary>Fires whenever coins, boosters or No Ads change.</summary>
        public static event Action Changed;

        /// <summary>Boosters the gameplay code can use. Only these are sold and granted.</summary>
        public static readonly BoosterType[] LiveBoosters =
            { BoosterType.ExtraPatri, BoosterType.Jhaaru, BoosterType.Chashma, BoosterType.UltiBelt };

        public static SaveData Data
        {
            get
            {
                if (_data == null) Load();
                return _data;
            }
        }

        public static int Coins => Data.coins;
        public static bool NoAds => Data.noAds;

        public static bool IsLive(BoosterType b) => Array.IndexOf(LiveBoosters, b) >= 0;

        public static void Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, "");
            _data = null;
            if (!string.IsNullOrEmpty(json))
            {
                try { _data = JsonUtility.FromJson<SaveData>(json); }
                catch (Exception e) { Debug.LogWarning("Save data unreadable, starting fresh: " + e.Message); }
            }
            if (_data == null)
            {
                _data = new SaveData();
                // One-time move of coins from the prototype's old key.
                _data.coins = PlayerPrefs.GetInt(LegacyCoinsKey, 0);
            }
            if (_data.boosters == null || _data.boosters.Length != Boosters.Count)
            {
                var fixedArr = new int[Boosters.Count];
                if (_data.boosters != null)
                    Array.Copy(_data.boosters, fixedArr, Mathf.Min(_data.boosters.Length, fixedArr.Length));
                _data.boosters = fixedArr;
            }
            if (_data.grantedTransactions == null) _data.grantedTransactions = new List<string>();
        }

        public static void Save()
        {
            var d = Data;
            if (d.grantedTransactions.Count > MaxTransactionsKept)
                d.grantedTransactions.RemoveRange(0, d.grantedTransactions.Count - MaxTransactionsKept);
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(d));
            PlayerPrefs.SetInt(LegacyCoinsKey, d.coins); // keeps older code that reads bf_coins working
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        // ---------------- Coins ----------------

        public static void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Data.coins += amount;
            Save();
        }

        public static bool TrySpendCoins(int amount)
        {
            if (amount < 0 || Data.coins < amount) return false;
            Data.coins -= amount;
            Save();
            return true;
        }

        // ---------------- Boosters ----------------

        public static int BoosterCount(BoosterType b) => Data.boosters[(int)b];

        public static void AddBooster(BoosterType b, int amount = 1)
        {
            if (amount <= 0) return;
            Data.boosters[(int)b] += amount;
            Save();
        }

        public static bool TryUseBooster(BoosterType b)
        {
            if (Data.boosters[(int)b] <= 0) return false;
            Data.boosters[(int)b]--;
            Save();
            return true;
        }

        public static bool TryBuyBoosterWithCoins(BoosterType b)
        {
            if (!IsLive(b) || !TrySpendCoins(MonetizationConfig.BoosterCoinPrice)) return false;
            AddBooster(b);
            return true;
        }

        // ---------------- No Ads / offers ----------------

        public static void SetNoAds()
        {
            Data.noAds = true;
            Save();
        }

        static int TodayNumber => (int)(DateTime.UtcNow - new DateTime(2000, 1, 1)).TotalDays;

        public static bool FreeBoosterAvailable =>
            MonetizationConfig.DailyFreeBoosterEnabled && Data.lastFreeBoosterDay != TodayNumber;

        public static void ClaimFreeBooster(BoosterType b)
        {
            Data.lastFreeBoosterDay = TodayNumber;
            AddBooster(b);
        }

        /// <summary>Starts the 72 h Starter Pack window (call on the player's first loss).</summary>
        public static void StartStarterOfferIfNeeded()
        {
            if (Data.starterOfferStartTicks != 0 || Data.starterPackBought) return;
            Data.starterOfferStartTicks = DateTime.UtcNow.Ticks;
            Save();
        }

        public static TimeSpan StarterOfferTimeLeft
        {
            get
            {
                if (Data.starterOfferStartTicks == 0 || Data.starterPackBought) return TimeSpan.Zero;
                var end = new DateTime(Data.starterOfferStartTicks, DateTimeKind.Utc).AddHours(MonetizationConfig.StarterPackHours);
                var left = end - DateTime.UtcNow;
                return left > TimeSpan.Zero ? left : TimeSpan.Zero;
            }
        }

        public static bool StarterOfferActive => StarterOfferTimeLeft > TimeSpan.Zero;

        // ---------------- Purchase de-duplication ----------------

        public static bool AlreadyGranted(string transactionId) =>
            !string.IsNullOrEmpty(transactionId) && Data.grantedTransactions.Contains(transactionId);

        public static void MarkGranted(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId) || Data.grantedTransactions.Contains(transactionId)) return;
            Data.grantedTransactions.Add(transactionId);
            Save();
        }

        /// <summary>Debug helper: wipes the wallet (not level progress).</summary>
        public static void ResetAll()
        {
            _data = new SaveData();
            Save();
        }
    }
}
