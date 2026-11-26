using System;
using UnityEngine;

namespace BazaarFlow.UI
{
    /// <summary>Player settings shown in Settings / Pause. Saved in PlayerPrefs; the sound module listens to Changed.</summary>
    public static class UISettings
    {
        const string SoundKey = "bf_sfx", MusicKey = "bf_music", HapticsKey = "bf_haptics";

        public static event Action Changed;

        public static bool Sound
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set => Set(SoundKey, value);
        }

        public static bool Music
        {
            get => PlayerPrefs.GetInt(MusicKey, 1) == 1;
            set => Set(MusicKey, value);
        }

        public static bool Haptics
        {
            get => PlayerPrefs.GetInt(HapticsKey, 1) == 1;
            set => Set(HapticsKey, value);
        }

        static void Set(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }

    public enum HapticStrength { Light, Medium, Heavy }

    /// <summary>Short vibration ticks. Android uses the Vibrator service with a few milliseconds; elsewhere nothing.</summary>
    public static class Haptics
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        static AndroidJavaObject _vibrator;
        static bool _tried;
#endif

        public static void Play(HapticStrength strength)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (!_tried)
                {
                    _tried = true;
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                        _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
                if (_vibrator == null) return;
                long ms = strength == HapticStrength.Light ? 12 : strength == HapticStrength.Medium ? 25 : 45;
                _vibrator.Call("vibrate", ms);
            }
            catch (Exception) { _vibrator = null; }
#endif
        }
    }
}
