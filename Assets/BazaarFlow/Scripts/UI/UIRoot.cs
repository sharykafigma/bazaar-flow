using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace BazaarFlow.UI
{
    /// <summary>
    /// Builds the whole UI on Play (no prefabs or scene setup) and runs the screen flow:
    /// Main Menu -> (Level Map) -> HUD over gameplay -> Pause / Level Complete / Belt Jam -> next.
    /// Talks to gameplay only through GameManager's public API, so gameplay code stays untouched.
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        public static UIRoot Instance { get; private set; }

        public Canvas Canvas { get; private set; }
        public RectTransform CanvasRect { get; private set; }

        public MainMenuScreen Menu { get; private set; }
        public LevelMapScreen Map { get; private set; }
        public HudScreen Hud { get; private set; }
        public PauseScreen Pause { get; private set; }
        public WinScreen Win { get; private set; }
        public JamScreen Jam { get; private set; }
        public SettingsScreen Settings { get; private set; }
        public DailyRewardScreen Daily { get; private set; }
        public OutOfLivesScreen OutOfLives { get; private set; }
        public BoosterBar BoosterBar { get; private set; }
        public ShopScreen Shop { get; private set; }
        public UIFx Fx { get; private set; }

        public GameManager Game { get; private set; }
        public bool InGame { get; private set; }

        CanvasScaler _scaler;
        GameState _lastState;
        bool _continueUsed;
        bool _legacyHudOff;
        bool _resultPending;

        public const int ContinueCoinCost = 200;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindFirstObjectByType<UIRoot>() != null) return;
            new GameObject("BazaarFlowUI").AddComponent<UIRoot>();
        }

        void Awake()
        {
            Instance = this;
            EnsureEventSystem();

            Canvas = gameObject.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 50;
            Canvas.pixelPerfect = false;
            _scaler = gameObject.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(UITheme.RefWidth, UITheme.RefHeight);
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _scaler.referencePixelsPerUnit = 100f;
            gameObject.AddComponent<GraphicRaycaster>();
            CanvasRect = (RectTransform)transform;
            UpdateMatch();

            Hud = Add<HudScreen>("HUD");
            Menu = Add<MainMenuScreen>("MainMenu");
            Map = Add<LevelMapScreen>("LevelMap");
            Pause = Add<PauseScreen>("Pause");
            Win = Add<WinScreen>("LevelComplete");
            Jam = Add<JamScreen>("BeltJam");
            Shop = Add<ShopScreen>("Shop");
            Settings = Add<SettingsScreen>("Settings");
            Daily = Add<DailyRewardScreen>("DailyReward");
            OutOfLives = Add<OutOfLivesScreen>("OutOfLives");

            // Boosters along the bottom of the HUD, lives on the main menu (opposite the coins).
            BoosterBar = BoosterBar.Create(Hud.transform, this);
            var lives = LivesBadge.Create(Menu.transform);
            ((RectTransform)lives.transform).Place(new Vector2(0f, 1f), new Vector2(210f, -250f), new Vector2(330f, 92f));
            Fx = UIFactory.Rect("FX", transform).Stretch().gameObject.AddComponent<UIFx>();

            _legacyHudOff = TryDisableLegacyHud();

            GameManager.LevelStarted += OnLevelStarted;
            GameManager.RushChanged += OnRush;
            Progress.StreakRewarded += OnStreak;
        }

        void OnDestroy()
        {
            GameManager.LevelStarted -= OnLevelStarted;
            GameManager.RushChanged -= OnRush;
            Progress.StreakRewarded -= OnStreak;
        }

        void OnLevelStarted(int level)
        {
            var intro = Game != null && Game.Level != null ? Game.Level.intro : null;
            if (intro != null) Tween.Delay(this, 0.8f, () => Fx.Toast(intro));
            else if (Game != null && Game.Level != null && Game.Level.difficulty == Difficulty.VeryHard)
                Tween.Delay(this, 0.6f, () => Fx.Toast("Super hard level! Double coins if you win."));
            else if (Game != null && Game.Level != null && Game.Level.difficulty == Difficulty.Hard)
                Tween.Delay(this, 0.6f, () => Fx.Toast("Hard level! Bonus coins if you win."));
        }

        void OnRush(bool on)
        {
            if (on && InGame) Fx.Toast("RUSH HOUR!");
        }

        void OnStreak(BoosterType booster, int streak)
        {
            Tween.Delay(this, 1.6f, () => Fx.Toast($"{streak} wins in a row! Free {Boosters.Name(booster)}"));
        }

        void Start()
        {
            FindGame();
            ShowMenu();
            if (DailyReward.Available) Tween.Delay(this, 0.9f, () => { if (Menu.IsOpen) Daily.Show(); });
        }

        T Add<T>(string name) where T : UIScreen
        {
            var rt = UIFactory.Rect(name, transform);
            var s = rt.gameObject.AddComponent<T>();
            s.Setup(this);
            return s;
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        /// <summary>
        /// The prototype GameManager draws its own OnGUI HUD and win/jam panels. If gameplay exposes a public static
        /// bool DrawPrototypeHud, switch it off; otherwise the UI pauses the GameManager while its own panels are up.
        /// </summary>
        static bool TryDisableLegacyHud()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            var t = typeof(GameManager);
            var f = t.GetField("DrawPrototypeHud", flags);
            if (f != null && f.FieldType == typeof(bool)) { f.SetValue(null, false); return true; }
            var p = t.GetProperty("DrawPrototypeHud", flags);
            if (p != null && p.CanWrite && p.PropertyType == typeof(bool)) { p.SetValue(null, false); return true; }
            return false;
        }

        void FindGame()
        {
            if (Game == null) Game = FindFirstObjectByType<GameManager>();
        }

        void UpdateMatch()
        {
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            // Phones (tall): keep width fixed. Tablets (wide): fit height so nothing gets huge.
            _scaler.matchWidthOrHeight = aspect > 0.62f ? 1f : 0f;
        }

        void Update()
        {
            UpdateMatch();
            FindGame();
            if (BackPressed()) OnBack();
            // GameManager loads its level in its own Start(); freeze it behind the menu only after that ran.
            if (!InGame && Game != null && Game.enabled && Game.LevelNumber > 0) Game.enabled = false;
            if (Game == null || !InGame || _resultPending) return;

            var state = Game.State;
            if (state != _lastState)
            {
                _lastState = state;
                if (state == GameState.Won) OnWon();
                else if (state == GameState.Jammed) OnJammed();
            }
        }

        // ------------------------------------------------------------ flow

        public void ShowMenu()
        {
            InGame = false;
            SetGameRunning(false);
            Hud.Hide();
            Pause.Hide();
            Win.Hide();
            Jam.Hide();
            Map.Hide();
            Menu.Show();
        }

        public void ShowMap()
        {
            Menu.Hide();
            Map.Show();
        }

        /// <summary>Start playing. level &lt;= 0 means "continue the current/saved level".</summary>
        public void Play(int level = 0)
        {
            FindGame();
            if (Game == null) { Debug.LogWarning("[UI] No GameManager in scene."); return; }
            if (!Lives.CanPlay) { OutOfLives.Open(() => Play(level)); return; }
            Menu.Hide();
            Map.Hide();
            Win.Hide();
            Jam.Hide();
            Pause.Hide();
            if (level > 0 && (level != Game.LevelNumber || Game.State != GameState.Playing)) Game.LoadLevel(level);
            else if (Game.State != GameState.Playing) Game.LoadLevel(Mathf.Max(Game.LevelNumber, UIServices.GetUnlockedLevel()));
            BeginLevel();
        }

        void BeginLevel()
        {
            InGame = true;
            _continueUsed = false;
            _resultPending = false;
            _lastState = Game.State;
            SetGameRunning(true);
            Hud.Show();
            Hud.Refresh(true);
        }

        public void PauseGame()
        {
            if (!InGame || Game == null || Game.State != GameState.Playing) return;
            SetGameRunning(false);
            Pause.Show();
        }

        public void ResumeGame()
        {
            Pause.Hide();
            SetGameRunning(true);
        }

        public void Restart() => Restart(false);

        /// <summary>Reload the current level. afterLoss = true when it comes from the Belt Jam screen.</summary>
        public void Restart(bool afterLoss)
        {
            if (Game == null) return;
            if (!Lives.CanPlay) { OutOfLives.Open(() => Restart(afterLoss)); return; }
            if (afterLoss) UIServices.OnRetryAfterLoss?.Invoke();
            Pause.Hide();
            Jam.Hide();
            Win.Hide();
            Game.LoadLevel(Game.LevelNumber);
            BeginLevel();
        }

        public void NextLevel()
        {
            if (Game == null) return;
            Win.Hide();
            int level = Game.LevelNumber;
            UIServices.BeforeNextLevel(level, () =>
            {
                Game.LoadLevel(level + 1);
                BeginLevel();
            });
        }

        public void OpenShop() => Shop.Show();
        public void OpenSettings() => Settings.Show();

        void OnWon()
        {
            _resultPending = true;
            if (!_legacyHudOff) SetGameRunning(false);
            int level = Game.LevelNumber;
            int reward = Game.LastReward;
            int stars = Game.LastStars;
            UIServices.Sfx("ui_win");
            UIServices.Haptic(HapticStrength.Medium);
            Tween.Delay(this, 0.35f, () => Win.Open(level, reward, stars));
        }

        void OnJammed()
        {
            _resultPending = true;
            if (!_legacyHudOff) SetGameRunning(false);
            UIServices.Sfx("ui_jam");
            UIServices.Haptic(HapticStrength.Heavy);
            Hud.ShakeTop();
            Tween.Delay(this, 0.45f, () => Jam.Open(!_continueUsed));
        }

        public void ContinueWithAd()
        {
            UIServices.ShowRewarded("continue", ok =>
            {
                if (!ok) { Fx.Toast("Ad not available. Try again!"); return; }
                DoContinue();
            });
        }

        public void ContinueWithCoins()
        {
            if (UIServices.TrySpendCoins == null || !UIServices.TrySpendCoins(ContinueCoinCost))
            {
                Fx.Toast("Not enough coins");
                Shop.Show();
                return;
            }
            UIServices.NotifyWalletChanged();
            DoContinue();
        }

        void DoContinue()
        {
            _continueUsed = true;
            Jam.Hide();
            SetGameRunning(true);
            Game.ContinueAfterJam();
            _lastState = Game.State;
            _resultPending = false;
        }

        /// <summary>Pausing = disabling the GameManager: its Update (input, belt) and OnGUI both stop.</summary>
        void SetGameRunning(bool running)
        {
            if (Game == null) return;
            if (!running && Game.LevelNumber == 0) return; // not started yet; Update freezes it once it has
            Game.enabled = running;
        }

        // ------------------------------------------------------------ Android back button

        static bool BackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        void OnBack()
        {
            if (Settings.IsOpen) { Settings.Hide(); return; }
            if (Daily.IsOpen) { Daily.Hide(); return; }
            if (OutOfLives.IsOpen) { OutOfLives.Hide(); return; }
            if (Shop.IsOpen) { Shop.Hide(); return; }
            if (Pause.IsOpen) { ResumeGame(); return; }
            if (Map.IsOpen) { Map.Hide(); Menu.Show(); return; }
            if (InGame && Game != null && Game.State == GameState.Playing) { PauseGame(); return; }
            if (Menu.IsOpen) Application.Quit();
        }
    }
}
