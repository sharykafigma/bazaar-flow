using UnityEngine;

namespace BazaarFlow
{
    /// <summary>Every sound effect in the game. File names live in AudioManager.SfxFiles.</summary>
    public enum Sfx { Tap, BeltLoad, Pickup, ShopDone, Win, Jam, UiClick, Unlock, Popup, Star, Coin, Purchase, Locked }

    /// <summary>
    /// Plays sound effects and the background music loop. Creates itself before the first scene loads,
    /// survives scene changes, and loads clips from Resources/BazaarFlowAudio, so nothing needs wiring in a scene.
    /// Sound and music can be turned on/off separately; the choice is saved in PlayerPrefs (bf_sfx, bf_music,
    /// shared with UISettings and re-read every quarter second, so either side can flip them).
    /// Gameplay calls the static helpers, e.g. AudioManager.Play(Sfx.Tap).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // Same PlayerPrefs keys as BazaarFlow.UI.UISettings, so the UI's Sound / Music switches mute this too.
        const string SfxKey = "bf_sfx";
        const string MusicKey = "bf_music";
        const string Folder = "BazaarFlowAudio/";
        const string MusicFile = "music_bazaar_loop";
        const int VoiceCount = 10;

        static readonly string[] SfxFiles =
        {
            "sfx_tap", "sfx_belt_load", "sfx_pickup", "sfx_shop_done",
            "sfx_win", "sfx_jam", "sfx_ui_click", "sfx_unlock",
            "sfx_ui_popup", "sfx_star", "sfx_coin", "sfx_purchase", "sfx_locked",
        };

        /// <summary>Base volume per Sfx, same order as the enum.</summary>
        static readonly float[] SfxVolume = { 0.9f, 0.35f, 0.7f, 0.8f, 1f, 1f, 0.6f, 0.8f, 0.6f, 0.8f, 0.5f, 0.9f, 0.7f };

        /// <summary>Shortest gap between two plays of the same Sfx, so busy belts don't turn into noise.</summary>
        static readonly float[] MinGap = { 0.04f, 0.06f, 0.03f, 0.08f, 0.5f, 0.5f, 0.03f, 0.1f, 0.1f, 0.12f, 0.05f, 0.5f, 0.15f };

        public const float MusicVolume = 0.45f;

        /// <summary>Set false once real UI has its own sound/music toggles.</summary>
        public static bool ShowPrototypeButtons = true;

        public static AudioManager Instance { get; private set; }

        AudioClip[] _clips;
        AudioSource[] _voices;
        AudioSource _music;
        float[] _lastPlayed;
        int _nextVoice;
        float _duck = 1f;       // current music duck multiplier
        float _duckUntil;       // music stays ducked until this unscaled time
        int _pickupStreak;
        float _nextPrefsCheck;
        float _lastPickup = -10f;

        public bool SfxOn { get; private set; }
        public bool MusicOn { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if (Instance != null) return;
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            go.AddComponent<AudioManager>();
            go.AddComponent<GameAudioDirector>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SfxOn = PlayerPrefs.GetInt(SfxKey, 1) == 1;
            MusicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;

            _clips = new AudioClip[SfxFiles.Length];
            _lastPlayed = new float[SfxFiles.Length];
            for (int i = 0; i < SfxFiles.Length; i++)
            {
                _clips[i] = Resources.Load<AudioClip>(Folder + SfxFiles[i]);
                _lastPlayed[i] = -10f;
                if (_clips[i] == null) Debug.LogWarning($"[Audio] Missing clip Resources/{Folder}{SfxFiles[i]}");
            }

            _voices = new AudioSource[VoiceCount];
            for (int i = 0; i < VoiceCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                _voices[i] = src;
            }

            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.spatialBlend = 0f;
            _music.priority = 0;
            _music.clip = Resources.Load<AudioClip>(Folder + MusicFile);
            if (_music.clip == null) Debug.LogWarning($"[Audio] Missing music Resources/{Folder}{MusicFile}");
            _music.volume = MusicVolume;
            if (MusicOn && _music.clip != null) _music.Play();
        }

        void Update()
        {
            if (Time.unscaledTime >= _nextPrefsCheck)
            {
                _nextPrefsCheck = Time.unscaledTime + 0.25f;
                RefreshSettings();
            }

            float target = Time.unscaledTime < _duckUntil ? 0.25f : 1f;
            _duck = Mathf.MoveTowards(_duck, target, Time.unscaledDeltaTime * (target < _duck ? 4f : 0.8f));
            _music.volume = MusicVolume * _duck;
        }

        // ---------------- Static helpers used by gameplay ----------------

        /// <summary>Plays one sound effect. Safe to call even if the manager or clip is missing.</summary>
        public static void Play(Sfx id, float pitch = 1f, float volume = 1f)
        {
            if (Instance != null) Instance.PlayInternal(id, pitch, volume);
        }

        /// <summary>Shop pickup "ting" that climbs in pitch when goods land quickly one after another.</summary>
        public static void PlayPickup()
        {
            if (Instance == null) return;
            var a = Instance;
            float now = Time.unscaledTime;
            a._pickupStreak = now - a._lastPickup < 0.9f ? Mathf.Min(a._pickupStreak + 1, 7) : 0;
            a._lastPickup = now;
            // Major-scale steps (semitones) so a combo sounds like a little tune.
            int[] steps = { 0, 2, 4, 5, 7, 9, 11, 12 };
            a.PlayInternal(Sfx.Pickup, Mathf.Pow(2f, steps[a._pickupStreak] / 12f), 1f);
        }

        /// <summary>Plays a big moment (win / Belt Jam) and lowers the music under it.</summary>
        public static void PlayStinger(Sfx id, float duckSeconds = 2.5f)
        {
            if (Instance == null) return;
            Instance._duckUntil = Time.unscaledTime + duckSeconds;
            Instance.PlayInternal(id, 1f, 1f);
        }

        /// <summary>Re-reads the on/off prefs (called automatically; UI may also call it from UISettings.Changed).</summary>
        public void RefreshSettings()
        {
            bool sfx = PlayerPrefs.GetInt(SfxKey, 1) == 1;
            bool music = PlayerPrefs.GetInt(MusicKey, 1) == 1;
            if (sfx != SfxOn) SetSfx(sfx);
            if (music != MusicOn) SetMusic(music);
        }

        /// <summary>
        /// Plays a UI sound by the string ids the UI module uses (UIServices.PlaySfx).
        /// ui_win and ui_jam are skipped on purpose: GameAudioDirector already plays those stingers when the state changes.
        /// </summary>
        public static void PlayUi(string id)
        {
            switch (id)
            {
                case "ui_tap": Play(Sfx.UiClick); break;
                case "ui_back":
                case "ui_close": Play(Sfx.UiClick, 0.8f); break;
                case "ui_toggle": Play(Sfx.UiClick, 1.2f); break;
                case "ui_popup": Play(Sfx.Popup); break;
                case "ui_star": Play(Sfx.Star); break;
                case "ui_coin": Play(Sfx.Coin); break;
                case "ui_purchase": PlayStinger(Sfx.Purchase, 1.2f); break;
                case "ui_locked": Play(Sfx.Locked); break;
                case "ui_win":
                case "ui_jam": break;
                default: Play(Sfx.UiClick); break;
            }
        }

        public void SetSfx(bool on)
        {
            SfxOn = on;
            if (PlayerPrefs.GetInt(SfxKey, 1) != (on ? 1 : 0))
            {
                PlayerPrefs.SetInt(SfxKey, on ? 1 : 0);
                PlayerPrefs.Save();
            }
            if (!on) foreach (var v in _voices) v.Stop();
        }

        public void SetMusic(bool on)
        {
            MusicOn = on;
            if (PlayerPrefs.GetInt(MusicKey, 1) != (on ? 1 : 0))
            {
                PlayerPrefs.SetInt(MusicKey, on ? 1 : 0);
                PlayerPrefs.Save();
            }
            if (on && _music.clip != null && !_music.isPlaying) _music.Play();
            else if (!on) _music.Stop();
        }

        void PlayInternal(Sfx id, float pitch, float volume)
        {
            if (!SfxOn) return;
            int i = (int)id;
            var clip = _clips[i];
            if (clip == null) return;
            float now = Time.unscaledTime;
            if (now - _lastPlayed[i] < MinGap[i]) return;
            _lastPlayed[i] = now;

            var src = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            src.Stop();
            src.clip = clip;
            // Small random pitch drift keeps repeated sounds from feeling robotic.
            float drift = id == Sfx.Pickup || id == Sfx.Win || id == Sfx.Jam ? 1f : Random.Range(0.96f, 1.04f);
            src.pitch = pitch * drift;
            src.volume = SfxVolume[i] * volume;
            src.Play();
        }

        // ---------------- Prototype toggles (until real UI exists) ----------------

        GUIStyle _btn;

        void OnGUI()
        {
            if (!ShowPrototypeButtons) return;
            float u = Screen.height / 100f;
            if (_btn == null || _btn.fontSize != Mathf.RoundToInt(u * 1.8f))
                _btn = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(u * 1.8f), fontStyle = FontStyle.Bold };

            float w = u * 11f, h = u * 4f, gap = u * 1f;
            float x = Screen.width - w - u * 2f;
            float y = u * 6.5f;
            if (GUI.Button(new Rect(x - w - gap, y, w, h), MusicOn ? "Music: ON" : "Music: OFF", _btn))
            {
                SetMusic(!MusicOn);
                Play(Sfx.UiClick);
            }
            if (GUI.Button(new Rect(x, y, w, h), SfxOn ? "Awaaz: ON" : "Awaaz: OFF", _btn))
            {
                SetSfx(!SfxOn);
                Play(Sfx.UiClick);
            }
        }
    }
}
