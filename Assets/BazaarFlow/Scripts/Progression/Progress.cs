using System;
using System.Globalization;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Level progress, stars and the win streak. Saved in PlayerPrefs:
    /// "bf_level" = highest unlocked level (kept from the prototype, the UI reads it too),
    /// "bf_stars" = stars per level as a comma list, "bf_streak" = wins in a row.
    /// </summary>
    public static class Progress
    {
        const string LevelKey = "bf_level";
        const string StarsKey = "bf_stars";
        const string StreakKey = "bf_streak";
        const string BestStreakKey = "bf_best_streak";

        /// <summary>A free booster every this many wins in a row.</summary>
        public const int StreakRewardEvery = 3;

        /// <summary>Fires when progress changes (level unlocked, stars, streak).</summary>
        public static event Action Changed;

        /// <summary>Fires when a win streak earns a free booster: (booster, streak length).</summary>
        public static event Action<BoosterType, int> StreakRewarded;

        static int[] _stars;

        /// <summary>Highest level the player may start (the next unbeaten one).</summary>
        public static int UnlockedLevel => Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, 1));

        /// <summary>The level to open when the player presses Play.</summary>
        public static int CurrentLevel => UnlockedLevel;

        public static int WinStreak => PlayerPrefs.GetInt(StreakKey, 0);
        public static int BestStreak => PlayerPrefs.GetInt(BestStreakKey, 0);

        public static int StarsFor(int level)
        {
            LoadStars();
            return level >= 1 && level <= _stars.Length ? _stars[level - 1] : 0;
        }

        public static int TotalStars
        {
            get
            {
                LoadStars();
                int sum = 0;
                foreach (var s in _stars) sum += s;
                return sum;
            }
        }

        public static void OnLevelWon(int level, int stars)
        {
            if (level + 1 > UnlockedLevel) PlayerPrefs.SetInt(LevelKey, level + 1);

            LoadStars();
            if (level > _stars.Length) Array.Resize(ref _stars, level);
            if (stars > _stars[level - 1]) _stars[level - 1] = stars;
            SaveStars();

            int streak = WinStreak + 1;
            PlayerPrefs.SetInt(StreakKey, streak);
            if (streak > BestStreak) PlayerPrefs.SetInt(BestStreakKey, streak);
            PlayerPrefs.Save();

            if (streak % StreakRewardEvery == 0)
            {
                var live = Economy.LiveBoosters;
                var booster = live[(streak / StreakRewardEvery - 1) % live.Length];
                Economy.AddBooster(booster);
                StreakRewarded?.Invoke(booster, streak);
            }
            Changed?.Invoke();
        }

        public static void OnLevelLost()
        {
            if (WinStreak == 0) return;
            PlayerPrefs.SetInt(StreakKey, 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        static void LoadStars()
        {
            if (_stars != null) return;
            string raw = PlayerPrefs.GetString(StarsKey, "");
            if (string.IsNullOrEmpty(raw))
            {
                _stars = new int[0];
                return;
            }
            var parts = raw.Split(',');
            _stars = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out _stars[i]);
        }

        static void SaveStars()
        {
            var parts = new string[_stars.Length];
            for (int i = 0; i < _stars.Length; i++) parts[i] = _stars[i].ToString(CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(StarsKey, string.Join(",", parts));
        }

        /// <summary>Debug helper: back to level 1 (wallet untouched).</summary>
        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.DeleteKey(StarsKey);
            PlayerPrefs.DeleteKey(StreakKey);
            PlayerPrefs.DeleteKey(BestStreakKey);
            PlayerPrefs.Save();
            _stars = null;
            Changed?.Invoke();
        }
    }
}
