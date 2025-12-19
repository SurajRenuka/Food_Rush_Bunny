using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using GoogleMobileAds.Api;

#if UNITY_IOS && ENABLE_IOS14_AD_SUPPORT
using Unity.Advertisement.IosSupport; // ATT package (no DllImport needed)
#endif

#if GMA_UMP
using GoogleMobileAds.Ump.Api; // UMP
#endif

/// <summary>
/// Production-ready Google Mobile Ads manager with:
/// - UMP consent (EU/UK + US state regulations) using Update→LoadAndShowIfRequired
/// - iOS ATT + UMP orchestration via 3 toggles (no stacked UI)
/// - Works in iOS builds and the Unity Editor (when GMA_UMP is defined and UMP installed)
/// - Global RequestConfiguration (test devices, COPPA/TFUA, MaxAdContentRating)
/// - App Open, Interstitial, Rewarded, Banner
/// - Test utilities: log device IDs, reset consent, open privacy options
/// </summary>
public class GoogleAd_Manager : Singleton<GoogleAd_Manager>
{
    [Header("Android Ad Unit IDs (replace with live IDs)")]
    public string android_AppOpenUnitID = "ca-app-pub-3940256099942544/3419835294";
    public string android_BannerUnitID = "ca-app-pub-3940256099942544/6300978111";
    public string android_InterstitialUnitID = "ca-app-pub-3940256099942544/1033173712";
    public string android_RewardUnitID = "ca-app-pub-3940256099942544/5224354917";

    [Header("iOS Ad Unit IDs (replace with live IDs)")]
    public string iOS_AppOpenUnitID = "ca-app-pub-3940256099942544/5575463023";
    public string iOS_BannerUnitID = "ca-app-pub-3940256099942544/2934735716";
    public string iOS_InterstitialUnitID = "ca-app-pub-3940256099942544/4411468910";
    public string iOS_RewardUnitID = "ca-app-pub-3940256099942544/1712485313";

    [Header("Feature Toggles")]
    [SerializeField] private bool enableAppOpenAds = true;
    [SerializeField] private bool enableBannerAds = true;
    [SerializeField] private bool enableInterstitialAds = true;
    [SerializeField] private bool enableRewardedAds = true;
    [Tooltip("Show App Open Ad when the app returns to foreground.")]
    [SerializeField] private bool showAppOpenOnResume = true;

    [Header("Global Ad Settings")]
    [Tooltip("If your app is directed to children under 13 (COPPA).")]
    [SerializeField] private bool tagForChildDirectedTreatment = false;
    [Tooltip("If users are under the age of consent in applicable regions (TFUA).")]
    [SerializeField] private bool underAgeOfConsent = false;
    [Tooltip("Max ad content rating across all placements.")]
    [SerializeField] private MaxAdContentRating maxAdContentRating = MaxAdContentRating.T;
    [Tooltip("Force Non-Personalized Ads via npa=1. Normally not needed when using UMP; use only if policy requires it.")]
    [SerializeField] private bool forceNonPersonalizedAds = false;

    [Header("Test Devices (AdMob RequestConfiguration)")]
    [SerializeField] private List<string> testDeviceIds = new List<string>();
    [SerializeField] private bool autoAddCurrentDeviceAsTest = true;

    // ===== NEW: iOS consent orchestration toggles =====
    [Header("iOS Consent Orchestration")]
    [Tooltip("Show Apple's ATT prompt on iOS.")]
    [SerializeField] private bool enableATTPrompt_iOS = true;

    [Tooltip("Show UMP consent on iOS (GDPR/UK + US state regs).")]
    [SerializeField] private bool enableUMPConsent_iOS = true;

    [Tooltip("If both ATT and UMP are enabled on iOS: when TRUE, let UMP trigger ATT (GDPR/US -> ATT). When FALSE, show ATT first, then UMP.")]
    [SerializeField] private bool letUMPHandleATT = true;
    // ================================================

#if GMA_UMP
    [Header("UMP (Consent) Debug Options")]
    [Tooltip("Enable UMP debug geography override for testing consent UI.")]
    [SerializeField] private bool umpEnableDebug = false;
    [Tooltip("0=Disabled, 1=EEA, 2=NotEEA (use 1 to force GDPR in Editor/device)")]
    [SerializeField] private int umpDebugGeography = 0; // 0 Disabled, 1 EEA, 2 NotEEA
    [Tooltip("Optional: UMP test device hashed IDs (same format as AdMob RequestConfiguration)")]
    [SerializeField] private List<string> umpTestDeviceIds = new List<string>();
#endif

    public static event Action OnRewardAdLoaded;
    public static event Action OnRewardAdFailedToLoad;
    public static bool isRewardAdLoaded = false;

    private AppOpenAd appOpenAd;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private BannerView bannerView;

    private DateTime appOpenLoadTime;
    private bool isAppOpenShowing = false;

    public bool IsRewardedAdReady => rewardedAd != null && rewardedAd.CanShowAd();

    // ---- Unity lifecycle ----
    private void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);
        ApplyGlobalRequestConfiguration();
        StartCoroutine(InitializeWithConsentThenLoadAds());
    }

    private void OnApplicationPause(bool paused)
    {
        if (!paused && showAppOpenOnResume)
        {
            ShowAppOpenAd();
        }
    }

    private void OnDestroy()
    {
        interstitialAd?.Destroy();
        rewardedAd?.Destroy();
        bannerView?.Destroy();
        appOpenAd?.Destroy();
    }

    // ---- Consent + initialization ----
    private IEnumerator InitializeWithConsentThenLoadAds()
    {
        // Decide ATT/UMP strategy (iOS only)
        bool runATT_Manually = false;  // will we request ATT ourselves?
        bool runUMP_Now = true;        // will we run UMP in this coroutine?

#if UNITY_IOS
        if (!enableUMPConsent_iOS) runUMP_Now = false;

        if (enableATTPrompt_iOS && enableUMPConsent_iOS && letUMPHandleATT)
        {
            // Option A: UMP will orchestrate ATT internally (GDPR/US -> ATT)
            Debug.Log("[Consent] iOS: letting UMP handle ATT (GDPR/US -> ATT).");
        }
        else if (enableATTPrompt_iOS)
        {
            // Option B: we show ATT manually (before UMP or when UMP disabled)
            runATT_Manually = true;
        }
#endif

#if UNITY_IOS && ENABLE_IOS14_AD_SUPPORT
        if (runATT_Manually)
        {
            // Request ATT before UMP to avoid UI overlap
            yield return RequestATTIfNeeded();
            yield return new WaitForSecondsRealtime(0.2f); // small settle delay so dialogs never overlap
        }
#endif

        bool canRequestAds = true;

#if GMA_UMP
        if (runUMP_Now)
        {
            bool consentFlowDone = false;
            canRequestAds = false;

            var reqParams = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = underAgeOfConsent
            };

            if (umpEnableDebug)
            {
                var debug = new ConsentDebugSettings();
                debug.DebugGeography = (DebugGeography)Mathf.Clamp(umpDebugGeography, 0, 2);
                // UMP 10.3.0+ uses List<string> for TestDeviceHashedIds
                debug.TestDeviceHashedIds = umpTestDeviceIds ?? new List<string>();
                reqParams.ConsentDebugSettings = debug;
            }

            // 1) Always Update
            ConsentInformation.Update(reqParams, (FormError updateError) =>
            {
                if (updateError != null)
                {
                    Debug.LogWarning($"[UMP] Update error: {updateError.Message}");
                }

                // 2) Always call LoadAndShowConsentFormIfRequired — no-op if nothing is required (EU/US)
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    if (formError != null)
                    {
                        Debug.LogWarning($"[UMP] Form error: {formError.Message}");
                    }

                    canRequestAds = ConsentInformation.CanRequestAds();
                    consentFlowDone = true;
                });
            });

            while (!consentFlowDone) yield return null;
        }
        else
        {
            Debug.Log("[UMP] Skipped (enableUMPConsent_iOS=false on iOS, or running on a platform without UMP).");
            canRequestAds = true;
        }
#else
        Debug.LogWarning("[UMP] GMA_UMP not defined or UMP package not installed. Proceeding without UMP.");
        canRequestAds = true;
#endif

        // 3) Initialize the SDK (safe to init regardless, but we load ads only if allowed)
        bool initDone = false;
        MobileAds.Initialize(_ =>
        {
            Debug.Log("[GMA] Mobile Ads Initialized");
            initDone = true;
        });
        while (!initDone) yield return null;

        if (autoAddCurrentDeviceAsTest)
        {
            TryAutoAddCurrentDeviceToTests();
        }

        if (canRequestAds)
        {
            LoadAllAds();
        }
        else
        {
            Debug.Log("[GMA] Consent does not allow requesting ads yet. You can call PresentPrivacyOptions() later.");
        }
    }

#if UNITY_IOS && ENABLE_IOS14_AD_SUPPORT
    // ATT helper that waits for focus, then requests, and polls briefly for completion.
    private IEnumerator RequestATTIfNeeded()
    {
        while (!Application.isFocused) yield return null; // ensure app is foreground
        yield return null;

        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        Debug.Log($"[ATT] Pre-request status={status} (0=NotDetermined,1=Restricted,2=Denied,3=Authorized)");

        if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();

            float t = 0f;
            while (t < 5f &&
                  ATTrackingStatusBinding.GetAuthorizationTrackingStatus()
                      == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        var post = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        Debug.Log($"[ATT] Post-request status={post} (0=NotDetermined,1=Restricted,2=Denied,3=Authorized)");
    }
#endif

    private void ApplyGlobalRequestConfiguration()
    {
        var cfg = new RequestConfiguration
        {
            TagForChildDirectedTreatment = tagForChildDirectedTreatment ? TagForChildDirectedTreatment.True : TagForChildDirectedTreatment.False,
            TagForUnderAgeOfConsent = underAgeOfConsent ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False,
            MaxAdContentRating = maxAdContentRating,
            TestDeviceIds = testDeviceIds,
        };
        MobileAds.SetRequestConfiguration(cfg);
    }

    private AdRequest BuildAdRequest()
    {
        if (forceNonPersonalizedAds)
        {
            return new AdRequest { Extras = new Dictionary<string, string> { { "npa", "1" } } };
        }
        return new AdRequest();
    }

    private void LoadAllAds()
    {
        if (enableAppOpenAds) LoadAppOpenAd();
        if (enableInterstitialAds) LoadInterstitialAd();
        if (enableRewardedAds) LoadRewardedAd();
        if (enableBannerAds) LoadBannerAd();
    }

    // ---- Ad Unit ID helpers ----
    private string AppOpenUnitID =>
#if UNITY_ANDROID
        android_AppOpenUnitID;
#elif UNITY_IOS
        iOS_AppOpenUnitID;
#else
        "unexpected_platform";
#endif

    private string InterstitialUnitID =>
#if UNITY_ANDROID
        android_InterstitialUnitID;
#elif UNITY_IOS
        iOS_InterstitialUnitID;
#else
        "unexpected_platform";
#endif

    private string RewardedUnitID =>
#if UNITY_ANDROID
        android_RewardUnitID;
#elif UNITY_IOS
        iOS_RewardUnitID;
#else
        "unexpected_platform";
#endif

    private string BannerUnitID =>
#if UNITY_ANDROID
        android_BannerUnitID;
#elif UNITY_IOS
        iOS_BannerUnitID;
#else
        "unexpected_platform";
#endif

    // ---- App Open ----
    private void LoadAppOpenAd()
    {
        appOpenAd?.Destroy();
        AppOpenAd.Load(AppOpenUnitID, BuildAdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("[AppOpen] Load failed: " + error);
                return;
            }

            appOpenAd = ad;
            appOpenLoadTime = DateTime.UtcNow;

            ad.OnAdFullScreenContentOpened += () => isAppOpenShowing = true;
            ad.OnAdFullScreenContentClosed += () => { isAppOpenShowing = false; LoadAppOpenAd(); };
            ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogError("[AppOpen] Show failed: " + err.GetMessage());
                isAppOpenShowing = false;
                LoadAppOpenAd();
            };

            Debug.Log("[AppOpen] Loaded");
        });
    }

    public void ShowAppOpenAd()
    {
        if (!enableAppOpenAds) return;
        bool fresh = (DateTime.UtcNow - appOpenLoadTime).TotalHours < 4;
        if (appOpenAd != null && appOpenAd.CanShowAd() && !isAppOpenShowing && fresh)
        {
            appOpenAd.Show();
        }
        else
        {
            Debug.Log("[AppOpen] Not available");
        }
    }

    // ---- Interstitial ----
    private void LoadInterstitialAd()
    {
        interstitialAd?.Destroy();
        InterstitialAd.Load(InterstitialUnitID, BuildAdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("[Interstitial] Load failed: " + error);
                return;
            }

            interstitialAd = ad;
            ad.OnAdFullScreenContentClosed += () => LoadInterstitialAd();
            ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogError("[Interstitial] Show failed: " + err.GetMessage());
                LoadInterstitialAd();
            };

            Debug.Log("[Interstitial] Loaded");
        });
    }

    public void ShowInterstitialAd()
    {
        if (!enableInterstitialAds) return;
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning("[Interstitial] Not ready");
        }
    }

    // ---- Rewarded ----
    private void LoadRewardedAd()
    {
        rewardedAd?.Destroy();
        RewardedAd.Load(RewardedUnitID, BuildAdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("[Rewarded] Load failed: " + error);
                OnRewardAdFailedToLoad?.Invoke();
                isRewardAdLoaded = false;
                return;
            }

            rewardedAd = ad;
            ad.OnAdFullScreenContentClosed += () => LoadRewardedAd();
            ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogError("[Rewarded] Show failed: " + err.GetMessage());
                LoadRewardedAd();
            };

            Debug.Log("[Rewarded] Loaded");
            OnRewardAdLoaded?.Invoke();
            isRewardAdLoaded = true;
        });
    }

    public void ShowRewardedAd(Action onUserEarnedReward)
    {
        if (!enableRewardedAds) return;
        if (!IsRewardedAdReady)
        {
            Debug.LogWarning("[Rewarded] Not ready to show.");
            return;
        }

        bool rewardEarned = false;

        Action<Reward> rewardCallback = (Reward reward) =>
        {
            Debug.Log($"[Rewarded] User earned reward: {reward.Amount} {reward.Type}");
            rewardEarned = true;
        };

        Action adClosedCallback = () =>
        {
            Debug.Log("[Rewarded] Closed.");
            if (rewardEarned)
                onUserEarnedReward?.Invoke();
        };

        RegisterRewardedHandlers(rewardedAd, rewardCallback, adClosedCallback);
        rewardedAd.Show(rewardCallback);
    }

    private void RegisterRewardedHandlers(RewardedAd ad, Action<Reward> onUserEarnedReward, Action onAdClosed)
    {
        ad.OnAdPaid += (AdValue adValue) => Debug.Log($"[Rewarded] Paid {adValue.Value} {adValue.CurrencyCode}");
        ad.OnAdImpressionRecorded += () => Debug.Log("[Rewarded] Impression recorded.");
        ad.OnAdClicked += () => Debug.Log("[Rewarded] Clicked.");
        ad.OnAdFullScreenContentOpened += () => Debug.Log("[Rewarded] Opened.");
        ad.OnAdFullScreenContentClosed += () => onAdClosed?.Invoke();
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("[Rewarded] Failed to open full screen: " + error);
            LoadRewardedAd();
        };
    }

    /// <summary>
    /// Two-ad sequence helper (e.g., full stamina). Calls onFinalReward after the second ad completes.
    /// </summary>
    public void ShowTwoAdsForFullStamina(Action onFinalReward)
    {
        if (!IsRewardedAdReady)
        {
            Debug.LogWarning("[Rewarded] Cannot start two-ad sequence: no ad ready.");
            return;
        }
        Action firstAdReward = () => StartCoroutine(WaitForAndShowSecondAd(onFinalReward));
        ShowRewardedAd(firstAdReward);
    }

    private IEnumerator WaitForAndShowSecondAd(Action onFinalReward)
    {
        while (!IsRewardedAdReady) yield return null;
        ShowRewardedAd(onFinalReward);
    }

    // ---- Banner ----
    public void LoadBannerAd()  // made public for external callers / UnityEvents
    {
        if (!enableBannerAds) return;

        bannerView?.Destroy();
        bannerView = new BannerView(BannerUnitID, AdSize.Banner, AdPosition.Bottom);
        bannerView.OnBannerAdLoaded += () => Debug.Log("[Banner] Loaded");
        bannerView.OnBannerAdLoadFailed += err => Debug.LogError("[Banner] Load failed: " + err.GetMessage());
        bannerView.LoadAd(BuildAdRequest());
    }

    public void ShowBannerAd() => bannerView?.Show();
    public void HideBannerAd() => bannerView?.Hide();
    public void DestroyBannerAd()
    {
        bannerView?.Destroy();
        bannerView = null;
    }

    // ---- Test & Consent Utilities ----

#if GMA_UMP
    [ContextMenu("UMP • Reset (first-run) and Show")]
    private void UMP_ResetAndShow()
    {
        ConsentInformation.Reset();
        StartCoroutine(InitializeWithConsentThenLoadAds());
    }

    [ContextMenu("UMP • Show Privacy Options")]
    public void PresentPrivacyOptions()
    {
        ConsentForm.ShowPrivacyOptionsForm(err =>
        {
            if (err != null) Debug.LogWarning($"[UMP] Privacy options error: {err.Message}");
            else Debug.Log("[UMP] Privacy options closed");
        });
    }
#endif

    [ContextMenu("Log Advertising & Test Device IDs")]
    public void LogAdvertisingAndTestDeviceIds()
    {
        string adId = GetAdvertisingIdSafe();
        string fallback = SystemInfo.deviceUniqueIdentifier ?? string.Empty;

        string md5FromAdId = !string.IsNullOrEmpty(adId) ? ToMd5Upper(adId) : string.Empty;
        string md5FromFallback = !string.IsNullOrEmpty(fallback) ? ToMd5Upper(fallback) : string.Empty;

        Debug.Log($"[TestDevice] Raw Advertising ID: {adId}");
        Debug.Log($"[TestDevice] Raw Fallback (SystemInfo.deviceUniqueIdentifier): {fallback}");
        if (!string.IsNullOrEmpty(md5FromAdId)) Debug.Log($"[TestDevice] TEST_DEVICE_ID (MD5 of Ad ID): {md5FromAdId}");
        if (!string.IsNullOrEmpty(md5FromFallback)) Debug.Log($"[TestDevice] Alt TEST_DEVICE_ID (MD5 of fallback): {md5FromFallback}");

        Debug.Log("[TestDevice] Add one of the MD5 values above to testDeviceIds in the Inspector.");
    }

    private void TryAutoAddCurrentDeviceToTests()
    {
        string adId = GetAdvertisingIdSafe();
        string hashed = string.IsNullOrEmpty(adId) ? null : ToMd5Upper(adId);
        if (!string.IsNullOrEmpty(hashed) && !testDeviceIds.Contains(hashed))
        {
            testDeviceIds.Add(hashed);
            var cfg = MobileAds.GetRequestConfiguration();
            cfg.TestDeviceIds = testDeviceIds;
            MobileAds.SetRequestConfiguration(cfg);
            Debug.Log($"[TestDevice] Auto-added current device to tests: {hashed}");
        }
    }

    private string GetAdvertisingIdSafe()
    {
        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = up.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient"))
            {
                var info = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", activity);
                return info.Call<string>("getId");
            }
#elif UNITY_IOS && !UNITY_EDITOR
            return UnityEngine.iOS.Device.advertisingIdentifier;
#else
            return string.Empty;
#endif
        }
        catch (Exception ex)
        {
            Debug.Log($"[AdID] Failed to get Advertising ID: {ex.Message}");
            return string.Empty;
        }
    }

    private static string ToMd5Upper(string input)
    {
        using (var md5 = MD5.Create())
        {
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
            return sb.ToString().ToUpperInvariant();
        }
    }
}
