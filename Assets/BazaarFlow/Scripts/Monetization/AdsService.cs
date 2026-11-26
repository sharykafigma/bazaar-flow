using System;
using UnityEngine;
#if BF_LEVELPLAY
using Unity.Services.LevelPlay;
#endif

namespace BazaarFlow
{
    /// <summary>
    /// Rewarded, interstitial and (optional) banner ads through LevelPlay mediation.
    ///
    /// Compiles with or without the "Ads Mediation" package:
    ///  - Package installed  -> BF_LEVELPLAY is defined (MonetizationDefines editor script) and real/mock ads run.
    ///  - Package missing    -> Editor / development builds simulate a rewarded ad so the game flow can be tested;
    ///                          release builds report "no ad" and never give free rewards.
    ///
    /// Usage:
    ///   AdsService.ShowRewarded("continue", ok => { if (ok) Continue(); });
    ///   AdsService.OnLevelWon(level, () => LoadNext());   // may show an interstitial, then calls back
    ///   AdsService.OnLevelLost();
    /// </summary>
    public class AdsService : MonoBehaviour
    {
        public static AdsService Instance { get; private set; }

        /// <summary>True while any full-screen ad is open (pause gameplay and audio).</summary>
        public static bool AdShowing { get; private set; }

        /// <summary>Fires when a full-screen ad opens (true) or closes (false). Sound thread can mute on this.</summary>
        public static event Action<bool> FullscreenAdChanged;

        float _lastInterstitialTime = -9999f;
        Action<bool> _rewardCallback;
        bool _rewardEarned;
        Action _interstitialCallback;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if (Instance != null) return;
            var go = new GameObject("AdsService");
            DontDestroyOnLoad(go);
            go.AddComponent<AdsService>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitSdk();
        }

        // ================= Public API =================

        public static bool IsRewardedReady
        {
            get
            {
#if BF_LEVELPLAY
                return Instance != null && Instance._rewarded != null && Instance._rewarded.IsAdReady();
#else
                return SimulateAds;
#endif
            }
        }

        /// <summary>Shows a rewarded ad. done(true) only if the player watched it to the end.</summary>
        public static void ShowRewarded(string placement, Action<bool> done)
        {
            if (Instance == null || AdShowing)
            {
                done?.Invoke(false);
                return;
            }
            Instance.ShowRewardedInternal(placement, done);
        }

        /// <summary>
        /// Call when the player wins and taps "next". Shows an interstitial if the pacing rules allow it,
        /// then calls next() (immediately if no ad).
        /// </summary>
        public static void OnLevelWon(int levelNumber, Action next)
        {
            var d = Economy.Data;
            d.winsSinceInterstitial++;
            int lossesBefore = d.lossStreak;
            d.lossStreak = 0;
            Economy.Save();

            if (Instance == null || !Instance.InterstitialAllowed(levelNumber, lossesBefore))
            {
                next?.Invoke();
                return;
            }
            Instance.ShowInterstitialInternal(next);
        }

        /// <summary>Call on every Belt Jam that ends in a retry (not on a rewarded continue).</summary>
        public static void OnLevelLost()
        {
            Economy.Data.lossStreak++;
            Economy.StartStarterOfferIfNeeded();
            Economy.Save();
        }

        /// <summary>Banner on result screens only (if enabled in MonetizationConfig and the player has ads).</summary>
        public static void SetBannerVisible(bool visible)
        {
            if (Instance == null) return;
            Instance.SetBannerInternal(visible && MonetizationConfig.BannerEnabled && !Economy.NoAds);
        }

        /// <summary>Call right after the No Ads purchase so a visible banner disappears at once.</summary>
        public static void OnNoAdsBought() => SetBannerVisible(false);

        // ================= Pacing =================

        bool InterstitialAllowed(int levelNumber, int lossesBeforeThisWin)
        {
            if (Economy.NoAds) return false;
            if (levelNumber < MonetizationConfig.InterstitialFirstLevel) return false;
            if (Economy.Data.winsSinceInterstitial < MonetizationConfig.InterstitialEveryNWins) return false;
            if (Time.realtimeSinceStartup - _lastInterstitialTime < MonetizationConfig.InterstitialMinGapSeconds) return false;
            if (lossesBeforeThisWin >= MonetizationConfig.InterstitialSkipAfterLosses) return false;
            return true;
        }

        void MarkInterstitialShown()
        {
            _lastInterstitialTime = Time.realtimeSinceStartup;
            Economy.Data.winsSinceInterstitial = 0;
            Economy.Save();
        }

        void SetAdShowing(bool showing)
        {
            AdShowing = showing;
            FullscreenAdChanged?.Invoke(showing);
        }

        void FinishRewarded()
        {
            var cb = _rewardCallback;
            bool ok = _rewardEarned;
            _rewardCallback = null;
            _rewardEarned = false;
            SetAdShowing(false);
            cb?.Invoke(ok);
        }

        void FinishInterstitial()
        {
            var cb = _interstitialCallback;
            _interstitialCallback = null;
            SetAdShowing(false);
            cb?.Invoke();
        }

#if BF_LEVELPLAY
        // ================= LevelPlay =================

        LevelPlayRewardedAd _rewarded;
        LevelPlayInterstitialAd _interstitial;
        LevelPlayBannerAd _banner;
        bool _initDone;
        bool _bannerWanted;

        void InitSdk()
        {
            // Privacy: call LevelPlayPrivacySettings.SetGDPRConsent / SetCCPA here, BEFORE Init,
            // once a consent screen exists (see Docs/MONETIZATION_SETUP.md).
            LevelPlay.OnInitSuccess += OnInitSuccess;
            LevelPlay.OnInitFailed += OnInitFailed;
            if (MonetizationConfig.LaunchLevelPlayTestSuite)
                LevelPlay.SetMetaData("is_test_suite", "enable");
            LevelPlay.Init(MonetizationConfig.LevelPlayAppKey);
        }

        void OnInitSuccess(LevelPlayConfiguration config)
        {
            Debug.Log("LevelPlay SDK initialized successfully");
            _initDone = true;

            _rewarded = new LevelPlayRewardedAd(MonetizationConfig.RewardedAdUnitId);
            _rewarded.OnAdLoaded += OnRewardedLoaded;
            _rewarded.OnAdLoadFailed += OnRewardedLoadFailed;
            _rewarded.OnAdDisplayFailed += OnRewardedDisplayFailed;
            _rewarded.OnAdRewarded += OnRewardedRewarded;
            _rewarded.OnAdClosed += OnRewardedClosed;
            _rewarded.LoadAd();

            _interstitial = new LevelPlayInterstitialAd(MonetizationConfig.InterstitialAdUnitId);
            _interstitial.OnAdLoaded += OnInterstitialLoaded;
            _interstitial.OnAdLoadFailed += OnInterstitialLoadFailed;
            _interstitial.OnAdDisplayFailed += OnInterstitialDisplayFailed;
            _interstitial.OnAdClosed += OnInterstitialClosed;
            if (!Economy.NoAds) _interstitial.LoadAd();

            if (MonetizationConfig.BannerEnabled && !Economy.NoAds) CreateBanner();

            if (MonetizationConfig.LaunchLevelPlayTestSuite) LevelPlay.LaunchTestSuite();
        }

        void OnInitFailed(LevelPlayInitError error)
        {
            Debug.LogWarning($"LevelPlay init failed: {error.ErrorMessage}. Retrying in 60 s.");
            Invoke(nameof(RetryInit), 60f);
        }

        void RetryInit() => LevelPlay.Init(MonetizationConfig.LevelPlayAppKey);

        void OnDestroy()
        {
            if (Instance != this) return;
            LevelPlay.OnInitSuccess -= OnInitSuccess;
            LevelPlay.OnInitFailed -= OnInitFailed;
            if (_rewarded != null)
            {
                _rewarded.OnAdLoaded -= OnRewardedLoaded;
                _rewarded.OnAdLoadFailed -= OnRewardedLoadFailed;
                _rewarded.OnAdDisplayFailed -= OnRewardedDisplayFailed;
                _rewarded.OnAdRewarded -= OnRewardedRewarded;
                _rewarded.OnAdClosed -= OnRewardedClosed;
                _rewarded.DestroyAd();
            }
            if (_interstitial != null)
            {
                _interstitial.OnAdLoaded -= OnInterstitialLoaded;
                _interstitial.OnAdLoadFailed -= OnInterstitialLoadFailed;
                _interstitial.OnAdDisplayFailed -= OnInterstitialDisplayFailed;
                _interstitial.OnAdClosed -= OnInterstitialClosed;
                _interstitial.DestroyAd();
            }
            DestroyBanner();
            Instance = null;
        }

        // ---- Rewarded ----

        void ShowRewardedInternal(string placement, Action<bool> done)
        {
            if (_rewarded == null || !_rewarded.IsAdReady())
            {
                if (_rewarded != null) _rewarded.LoadAd();
                done?.Invoke(false);
                return;
            }
            _rewardCallback = done;
            _rewardEarned = false;
            SetAdShowing(true);
            // Placement names need to exist in the LevelPlay dashboard first, so the default placement is used.
            Debug.Log($"[AdsService] rewarded: {placement}");
            _rewarded.ShowAd();
        }

        void OnRewardedLoaded(LevelPlayAdInfo info) { }

        void OnRewardedLoadFailed(LevelPlayAdError error)
        {
            Debug.LogWarning($"Rewarded load failed: {error.ErrorMessage}");
            Invoke(nameof(LoadRewarded), 30f);
        }

        void LoadRewarded() { if (_rewarded != null) _rewarded.LoadAd(); }

        void OnRewardedDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
        {
            Debug.LogWarning($"Rewarded display failed: {error.ErrorMessage}");
            _rewardEarned = false;
            FinishRewarded();
            LoadRewarded();
        }

        // Some networks fire "rewarded" after "closed"; both orders are handled.
        void OnRewardedRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            if (_rewardCallback == null) return;
            _rewardEarned = true;
            if (!AdShowing) FinishRewarded();
        }

        void OnRewardedClosed(LevelPlayAdInfo info)
        {
            LoadRewarded(); // keep one ready for the next Belt Jam
            if (_rewardEarned) FinishRewarded();
            else StartCoroutine(FinishRewardedAfterGrace());
        }

        System.Collections.IEnumerator FinishRewardedAfterGrace()
        {
            AdShowing = false;
            yield return new WaitForSecondsRealtime(0.5f); // wait for a late "rewarded" callback
            if (_rewardCallback != null) FinishRewarded();
        }

        // ---- Interstitial ----

        void ShowInterstitialInternal(Action next)
        {
            if (_interstitial == null || !_interstitial.IsAdReady())
            {
                if (_interstitial != null) _interstitial.LoadAd();
                next?.Invoke();
                return;
            }
            _interstitialCallback = next;
            SetAdShowing(true);
            MarkInterstitialShown();
            _interstitial.ShowAd();
        }

        void OnInterstitialLoaded(LevelPlayAdInfo info) { }

        void OnInterstitialLoadFailed(LevelPlayAdError error)
        {
            Debug.LogWarning($"Interstitial load failed: {error.ErrorMessage}");
            Invoke(nameof(LoadInterstitial), 30f);
        }

        void LoadInterstitial()
        {
            if (_interstitial != null && !Economy.NoAds) _interstitial.LoadAd();
        }

        void OnInterstitialDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
        {
            Debug.LogWarning($"Interstitial display failed: {error.ErrorMessage}");
            FinishInterstitial();
            LoadInterstitial();
        }

        void OnInterstitialClosed(LevelPlayAdInfo info)
        {
            FinishInterstitial();
            LoadInterstitial();
        }

        // ---- Banner ----

        void CreateBanner()
        {
            if (_banner != null) return;
            var cfg = new LevelPlayBannerAd.Config.Builder();
            cfg.SetSize(LevelPlayAdSize.BANNER);
            cfg.SetPosition(LevelPlayBannerPosition.BottomCenter);
            cfg.SetDisplayOnLoad(false);
            cfg.SetRespectSafeArea(true);
            _banner = new LevelPlayBannerAd(MonetizationConfig.BannerAdUnitId, cfg.Build());
            _banner.OnAdLoaded += OnBannerLoaded;
            _banner.OnAdLoadFailed += OnBannerLoadFailed;
            _banner.LoadAd();
        }

        void DestroyBanner()
        {
            if (_banner == null) return;
            _banner.OnAdLoaded -= OnBannerLoaded;
            _banner.OnAdLoadFailed -= OnBannerLoadFailed;
            _banner.DestroyAd();
            _banner = null;
        }

        void OnBannerLoaded(LevelPlayAdInfo info)
        {
            if (_bannerWanted) _banner.ShowAd();
        }

        void OnBannerLoadFailed(LevelPlayAdError error)
        {
            Debug.LogWarning($"Banner load failed: {error.ErrorMessage}");
        }

        void SetBannerInternal(bool visible)
        {
            _bannerWanted = visible;
            if (!_initDone) return;
            if (Economy.NoAds)
            {
                DestroyBanner();
                return;
            }
            if (visible)
            {
                if (_banner == null) CreateBanner();
                else _banner.ShowAd();
            }
            else if (_banner != null) _banner.HideAd();
        }

#else
        // ================= No SDK: simulation =================

        static bool SimulateAds => Application.isEditor || Debug.isDebugBuild;

        void InitSdk()
        {
            Debug.Log("[AdsService] Ads Mediation package not installed. " +
                      (SimulateAds ? "Simulating ads (editor/dev build)." : "Ads disabled."));
        }

        void ShowRewardedInternal(string placement, Action<bool> done)
        {
            if (!SimulateAds)
            {
                done?.Invoke(false);
                return;
            }
            Debug.Log($"[AdsService] (simulated) rewarded ad: {placement}");
            _rewardCallback = done;
            _rewardEarned = true;
            SetAdShowing(true);
            StartCoroutine(FakeAd(FinishRewarded));
        }

        void ShowInterstitialInternal(Action next)
        {
            if (!SimulateAds)
            {
                next?.Invoke();
                return;
            }
            Debug.Log("[AdsService] (simulated) interstitial");
            _interstitialCallback = next;
            SetAdShowing(true);
            MarkInterstitialShown();
            StartCoroutine(FakeAd(FinishInterstitial));
        }

        System.Collections.IEnumerator FakeAd(Action finish)
        {
            yield return new WaitForSecondsRealtime(1f);
            finish();
        }

        void SetBannerInternal(bool visible) { }

        void OnGUI()
        {
            if (!AdShowing) return;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.03f),
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "TEST AD", style);
        }
#endif
    }
}
