using System;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// 7-day login chain. Day 7 is the big chest. Missing a day starts the chain again at day 1.
    /// Saved in PlayerPrefs: "bf_daily_last" (UTC day number last claimed), "bf_daily_idx" (next day 0-6).
    /// </summary>
    public static class DailyReward
    {
        public struct Reward
        {
            public int Day;       // 1-7
            public int Coins;
            public int Boosters;  // one of each live booster this many times
        }

        public static readonly Reward[] Chain =
        {
            new Reward { Day = 1, Coins = 50 },
            new Reward { Day = 2, Coins = 75 },
            new Reward { Day = 3, Coins = 100, Boosters = 0 },
            new Reward { Day = 4, Coins = 150 },
            new Reward { Day = 5, Coins = 200 },
            new Reward { Day = 6, Coins = 250 },
            new Reward { Day = 7, Coins = 500, Boosters = 1 },
        };

        const string LastKey = "bf_daily_last";
        const string IndexKey = "bf_daily_idx";

        /// <summary>No daily reward before this level, so day one is all about the game.</summary>
        public const int FromLevel = 3;

        public static event Action<Reward> Claimed;

        static int Today => (int)(DateTime.UtcNow - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalDays;

        public static bool Available =>
            Progress.UnlockedLevel >= FromLevel && PlayerPrefs.GetInt(LastKey, -1) != Today;

        /// <summary>Index (0-6) of the reward that a claim today would give.</summary>
        public static int NextIndex
        {
            get
            {
                int last = PlayerPrefs.GetInt(LastKey, -1);
                int idx = PlayerPrefs.GetInt(IndexKey, 0);
                if (last < 0 || Today - last > 1) return 0; // chain broken
                return Mathf.Clamp(idx, 0, Chain.Length - 1);
            }
        }

        public static Reward Next => Chain[NextIndex];

        public static Reward Claim()
        {
            var r = Next;
            int idx = NextIndex;
            PlayerPrefs.SetInt(LastKey, Today);
            PlayerPrefs.SetInt(IndexKey, (idx + 1) % Chain.Length);
            PlayerPrefs.Save();
            Economy.AddCoins(r.Coins);
            if (r.Boosters > 0)
                foreach (var b in Economy.LiveBoosters) Economy.AddBooster(b, r.Boosters);
            Claimed?.Invoke(r);
            return r;
        }
    }
}
