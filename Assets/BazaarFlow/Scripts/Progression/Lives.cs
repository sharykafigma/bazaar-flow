using System;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Five lives. A jam costs one (given back if the player continues). One life comes back every 30 minutes.
    /// Saved in PlayerPrefs: "bf_lives", "bf_lives_t" (UTC ticks when the next regen countdown started),
    /// "bf_lives_unl" (UTC ticks until unlimited lives end).
    /// </summary>
    public static class Lives
    {
        public const int Max = 5;
        public static readonly TimeSpan RegenTime = TimeSpan.FromMinutes(30);
        /// <summary>Coin price to refill all lives.</summary>
        public const int RefillCoinPrice = 300;

        const string CountKey = "bf_lives";
        const string TimeKey = "bf_lives_t";
        const string UnlimitedKey = "bf_lives_unl";

        public static event Action Changed;

        /// <summary>Lives right now (regen applied).</summary>
        public static int Count
        {
            get
            {
                Regen();
                return PlayerPrefs.GetInt(CountKey, Max);
            }
        }

        public static bool Unlimited => UnlimitedLeft > TimeSpan.Zero;

        public static TimeSpan UnlimitedLeft
        {
            get
            {
                long until = ReadTicks(UnlimitedKey);
                if (until == 0) return TimeSpan.Zero;
                var left = new DateTime(until, DateTimeKind.Utc) - DateTime.UtcNow;
                return left > TimeSpan.Zero ? left : TimeSpan.Zero;
            }
        }

        public static bool CanPlay => Unlimited || Count > 0;

        /// <summary>Time until the next life, or zero when full.</summary>
        public static TimeSpan TimeToNext
        {
            get
            {
                Regen();
                if (PlayerPrefs.GetInt(CountKey, Max) >= Max) return TimeSpan.Zero;
                var start = new DateTime(ReadTicks(TimeKey), DateTimeKind.Utc);
                var left = start + RegenTime - DateTime.UtcNow;
                return left > TimeSpan.Zero ? left : TimeSpan.Zero;
            }
        }

        public static void LoseLife()
        {
            if (Unlimited) return;
            int n = Count;
            if (n <= 0) return;
            if (n == Max) WriteTicks(TimeKey, DateTime.UtcNow.Ticks); // countdown starts on the first lost life
            Set(n - 1);
        }

        /// <summary>The player continued after a jam, so the life is not lost after all.</summary>
        public static void RefundLife()
        {
            if (Unlimited) return;
            AddLives(1);
        }

        public static void AddLives(int amount)
        {
            int n = Mathf.Min(Max, Count + amount);
            Set(n);
        }

        public static void Refill() => Set(Max);

        public static bool TryRefillWithCoins()
        {
            if (Count >= Max || !Economy.TrySpendCoins(RefillCoinPrice)) return false;
            Refill();
            return true;
        }

        public static void GrantUnlimited(TimeSpan duration)
        {
            var from = Unlimited ? new DateTime(ReadTicks(UnlimitedKey), DateTimeKind.Utc) : DateTime.UtcNow;
            WriteTicks(UnlimitedKey, (from + duration).Ticks);
            Refill();
        }

        static void Regen()
        {
            int n = PlayerPrefs.GetInt(CountKey, Max);
            if (n >= Max) return;
            long t = ReadTicks(TimeKey);
            var now = DateTime.UtcNow;
            if (t == 0 || t > now.Ticks)
            {
                WriteTicks(TimeKey, now.Ticks); // missing or clock went back: restart the countdown
                return;
            }
            var start = new DateTime(t, DateTimeKind.Utc);
            int gained = (int)((now - start).Ticks / RegenTime.Ticks);
            if (gained <= 0) return;
            n = Mathf.Min(Max, n + gained);
            PlayerPrefs.SetInt(CountKey, n);
            WriteTicks(TimeKey, n >= Max ? now.Ticks : start.Ticks + gained * RegenTime.Ticks);
            PlayerPrefs.Save();
        }

        static void Set(int n)
        {
            PlayerPrefs.SetInt(CountKey, Mathf.Clamp(n, 0, Max));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        // PlayerPrefs has no long, so ticks are stored as a string.
        static long ReadTicks(string key)
        {
            long.TryParse(PlayerPrefs.GetString(key, "0"), out long v);
            return v;
        }

        static void WriteTicks(string key, long ticks) => PlayerPrefs.SetString(key, ticks.ToString());
    }
}
