using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.CrashReport;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Ads.FakeMediation;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo;
using Voodoo.Sauce.Internal.Misc;
using Voodoo.Sauce.Internal.CrossPromo.Models;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.AppRater;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts;
using Voodoo.Sauce.Internal.CrossPromo.Mercury;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Sauce.Tools.AccessButton;
using Voodoo.Sauce.Privacy;
using Voodoo.Sauce.Internal.DebugScreen.CodeStage.AdvancedFPSCounter;
using Voodoo.Sauce.Privacy.UI;
using Voodoo.Sauce.Tools.PerformanceDisplay;

#pragma warning disable 0649

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    internal class VoodooSauceBehaviour : MonoBehaviour
    {
        private const string TAG = "VoodooSauceBehaviour";

        private static VoodooSauceBehaviour _instance;

        [SerializeField]
        private AppRater _appRaterPrefab;

        [SerializeField]
        private PrivacyCanvas _privacyCanvasPrefab;

        [SerializeField]
        private CohortDebugMenu _cohortDebugMenuPrefab;

        [SerializeField]
        private BannerBackground _bannerBackgroundPrefab;

        [FormerlySerializedAs("inEditorAdsPrefab"), SerializeField]
        private GameObject fakeAdsPrefab;

        [SerializeField]
        private GameObject _accessButton;

        [SerializeField]
        private AFPSCounter _performanceDisplayPrefab;

        private CohortDebugMenu _cohortDebugMenu;

        private VoodooSettings _settings;

        private PrivacyCanvas _privacyCanvas;

        private PrivacyCanvas PrivacyCanvas {
            get {
                if (_privacyCanvas == null)
                    _privacyCanvas = Instantiate(_privacyCanvasPrefab, transform);
                return _privacyCanvas;
            }
        }

        private IRequest _request;

        private static bool _startCalled;
        private static bool _initStarted;
        private static bool _initFinished;
        private static string _mediationName;

        public static Action OnInternetConnect;
        public static Action OnPrivacyOpened;
        public static Action OnPrivacyClosed;
        public static Action onDeleteDataRequested;

        private static HandledAction<VoodooSauceInitCallbackResult> _initFinishedEvent = new HandledAction<VoodooSauceInitCallbackResult>(TAG);
        [CanBeNull]
        private static VoodooSauceInitCallbackResult _initCallbackResult;
        
        private static event Action<bool> OnAnalyticsConsentEvent;

        private async void Awake()
        {
            if (transform != transform.root)
                throw new Exception("VoodooSauce prefab HAS to be at the ROOT level!");

            _settings = VoodooSettings.Load();
            if (_settings == null)
                throw new Exception(
                    "Can't find VoodooSauce settings file. Please check you have created the file using Assets/Create/VoodooSauce/Settings File");
            
            if (_instance != null) {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(this);
            UnityThreadExecutor.Initialize();
            
            //Initialize Mercury (CrossPromo/BackUpFS) Test Mode
            MercuryTestModeManager.Instance.Initialize();
            
            SelectMediation();
            VoodooSuperPremium.Initialize(_settings);
            AnalyticsManager.StartTrackingVoodooSauceSDKInitializationLoadingTime();
            //Any VAN tracking must be called after preloading the analytics storage
            AnalyticsStorageHelper.Instance.PreLoad();
            AnalyticsSessionManager.Instance().Init();
            _request = new Request();
            _request.Initialize(this);
            VoodooSauceCore.Initialize(this);
            await VoodooSauceCore.GetPrivacy().Instantiate(_settings);

            InitPerformanceDisplay();

            AnalyticsManager.TrackApplicationLaunch();
            AnalyticsManager.TrackUnityEngineStarted();

            if (_settings.UseRemoteConfig) {
                InitVoodooTune();
            } else {
                InitVoodooSauce();
            }
        }
        
        private static void EnableUnityAnalytics()
        {
            UnityEngine.Analytics.Analytics.ResumeInitialization();
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DisableUnityAnalytics()
        {
            UnityEngine.Analytics.Analytics.initializeOnStartup = false;
        }

        private static void SelectMediation()
        {
            // The fake ads manager object must be initialized as soon as possible
            // in order to know if the fake ads are enabled or not.
            var fakeAdsManager = new FakeAdsManager();
            AdsManager.fakeAdsManager = fakeAdsManager;
            fakeAdsManager.Initialize();

            // the mediation should be selected (and not be initialized) before any other module in the VS
            // because we need this value to sent to any analytics event (mainly VAN for now)
            _mediationName = AdsManager.GetSelectedMediationName();
        }

        private void InitVoodooTune()
        {
            VoodooTuneManager.Initialize(InitVoodooSauce);
        }

        private void InitVoodooSauce()
        {
            if (_initStarted) {
                return;
            }

            _initStarted = true;
            VoodooLog.LogDebug(Module.COMMON, TAG,
                $"Initializing VoodooSauce v{VoodooSauce.Version()} on {_instance.name}");
            VoodooLog.LogDebug(Module.COMMON, TAG,
                $"App built with Unity Version: {Application.unityVersion}");
            gameObject.AddComponent<ExponentialBackoff>();
            // Update FakeMediationAdapter's Prefab
            if (AdsManager.fakeAdsManager.IsEnabled()) {
                FakeMediationAdapter.fakeAdsPrefab = fakeAdsPrefab;
            }

            InitCrashReport();
            InitAbTest();
            AnalyticsManager.Instantiate(_mediationName);
            InitAppRater();
            InitPrivacy();

            // Update FakeMediationAdapter's Prefab
            if (AdsManager.fakeAdsManager.IsEnabled()) {
                FakeMediationAdapter.fakeAdsPrefab = fakeAdsPrefab;
            }
        }

        internal static GameObject InstantiateBannerBackgroundPrefab() => _instance != null ? Instantiate(_instance._bannerBackgroundPrefab.gameObject) : null;

        private void InitAbTest()
        {
            AbTestManager.Initialize(_settings);
        }

        private void Start()
        {
            // Right now only the IAP is initialized by this method, but a long term goal is to manage every module initialization in this method
            VoodooSauceCore.GetInAppPurchase().Initialize(VoodooSettings.Load());
            StartCoroutine(CheckInternetAvailability());

            _startCalled = true;

            AccessProcess.InstantiateAccessButton += InstantiateAccessButton;
            AccessProcess.DisposeAccessButton += DisposeAccessButton;
            AccessProcess.CheckInstantiateButton();
        }

        public static void SubscribeOnInitFinishedEvent(Action<VoodooSauceInitCallbackResult> onInitFinished)
        {
            if (_initFinished) {
                onInitFinished?.Invoke(_initCallbackResult);
            } else {
                _initFinishedEvent?.Add(onInitFinished);
            }
        }

        public static void UnSubscribeOnInitFinishedEvent(Action<VoodooSauceInitCallbackResult> onInitFinished)
        {
            _initFinishedEvent?.Remove(onInitFinished);
        }

        internal static bool IsInitFinished() => _initFinished;

        public static void ShowCrossPromo(Action<AssetModel> onSuccess = null, Action<CPShowFailType> onFailure = null)
        {
            if (_instance == null) return;

            /* handle Settings */
            if (!CrossPromoSettings.IsCrossPromoEnabled() || VoodooSauce.IsPremium()) {
                return;
            }

            VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "Show promo from custom Behavior");
            VoodooCrossPromo.Show(onSuccess, onFailure);
        }

        public static void HideCrossPromo()
        {
            VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "Hide promo from custom Behavior");
            VoodooCrossPromo.Hide();
        }

        private static void InitAnalytics(PrivacyCore.GdprConsent consent)
        {
            AnalyticsManager.Initialize(consent);
            if (consent.ExplicitConsentGivenForAnalytics) EnableUnityAnalytics();
        }

        private void OnPrivacyConsent(PrivacyCore.GdprConsent gdpr)
        {
            VoodooLog.LogDebug(Module.PRIVACY, TAG, $"Consent has been given: {gdpr}");

            // analytics consent
            bool analyticsConsent = gdpr.ExplicitConsentGivenForAnalytics;

            OnAnalyticsConsentEvent?.Invoke(analyticsConsent);
            InitAnalytics(gdpr);

            //ads consent
            bool adsConsent = gdpr.ExplicitConsentGivenForAds;
            
            //If embargoed country ads shouldn't be initialized
            //If adsEnforcement is enabled and consent is not given, ads shouldn't be initialized
            if (!gdpr.IsEmbargoedCountry) {
                // Initialize AdsManager but not mediation
                // Used in case BackupAds are enabled
                AdsManager.Initialize(_settings);
                
                if (!(gdpr.IsAdsEnforcement && !adsConsent)) {
                    InitPromo();
                    AdsManager.InitializeMediation(_settings, adsConsent);
                    AudioAdsManager.Instance.Initialize(_settings, VoodooSauce.GetItem<AudioAdConfig>(), adsConsent,
                        VoodooSauceCore.GetPrivacy().IsCcpaApplicable(), VoodooSauceCore.IsPremiumOrIAPSubscribed,
                        AnalyticsManager.AudioAds);
                }

                BackupAdsManager.Instance.Initialize(gdpr);
            }
            
            _initCallbackResult = new VoodooSauceInitCallbackResult {
                AdsConsentGranted = gdpr.ExplicitConsentGivenForAds,
                AnalyticsConsentGranted = gdpr.ExplicitConsentGivenForAnalytics,
                IsEmbargoCountry = gdpr.IsEmbargoedCountry
            };
            _initFinished = true;
            _initFinishedEvent?.Invoke(_initCallbackResult);
            _initFinishedEvent?.Clear();
            _initFinishedEvent = null;

            AnalyticsManager.TrackVoodooSauceSDKInitializationLoadingTime();
        }

        private static void OnPrivacyUpdate(PrivacyCore.GdprConsent gdpr)
        {
            OnAnalyticsConsentEvent?.Invoke(gdpr.ExplicitConsentGivenForAnalytics);
        }

        private static void OnPrivacyShown(bool isPrivacyShown)
        {
            if (isPrivacyShown) {
                AnalyticsManager.StartPrivacyDisplayingTimer();
                OnPrivacyOpened?.Invoke();
            } else {
                AnalyticsManager.StopPrivacyDisplayingTimer();
                OnPrivacyClosed?.Invoke();
            }
        }

        private void InitCrashReport()
        {
            var parameters = new CrashReportCore.CrashReportManagerParameters {
                VoodooSettings = _settings,
                AnalyticsConsentEvent = null
            };
            OnAnalyticsConsentEvent += analyticsConsent => {
                parameters.AnalyticsConsentEvent?.Invoke(analyticsConsent);
            };
            VoodooSauceCore.GetCrashReport().Initialize(parameters);
        }

        private void InitPrivacy()
        {
            if (_privacyCanvasPrefab == null)
                throw new Exception("No Privacy Canvas, please make sure the Privacy Canvas is set in the VoodooSauce prefab");

            var parameters = new PrivacyCore.PrivacyManagerParameters {
                privacyConfiguration = VoodooTuneManager.GetItemOrDefault<PrivacyConfiguration>(),
                voodooSettings = _settings,
                onConsentGiven = OnPrivacyConsent,
                onConsentUpdate = OnPrivacyUpdate,
                onPrivacyShown = OnPrivacyShown,
                hasGameStarted = () => AnalyticsManager.HasGameStarted,
                tryOpenDebugger = () => Debugger.Debugger.TryOpen(),
                onDeleteDataRequest = onDeleteDataRequested,
                canvas = PrivacyCanvas,
                request = _request
            };
            OnInternetConnect += () => { parameters.onInternetAvailable.Invoke(); };
            AnalyticsManager.OnGameFinishedEvent += GameFinishedParameters => { parameters.onGameFinished(); };
            AnalyticsManager.OnGameStartedEvent += GameStartedParameters => { parameters.onGameStarted(); };
            VoodooSauceCore.GetPrivacy().Initialize(parameters);
        }

        private void InitAppRater()
        {
            Instantiate(_appRaterPrefab);

            AppRater.Initialize(_settings.AppleStoreId, Application.identifier,
                _settings.AppRaterDelayAfterLaunchInSeconds, _settings.AppRaterPostponeCooldownInSeconds,
                _settings.AppRaterMinimumNumberOfGamesPlayed);
        }

        private void InitPerformanceDisplay()
        {
            PerformanceDisplayManager.Initialize(_performanceDisplayPrefab);
        }

        private static void InitPromo()
        {
            VoodooCrossPromo.Init();
        }

        public static void ShowCohortDebugMenu()
        {
            if (_instance == null) return;

            if (_instance._cohortDebugMenu == null) {
                _instance._cohortDebugMenu = Instantiate(_instance._cohortDebugMenuPrefab);
            } else {
                _instance._cohortDebugMenu.Show();
            }
        }

        public static void CloseCohortDebugMenu()
        {
            if (_instance == null || _instance._cohortDebugMenu == null) return;

            _instance._cohortDebugMenu.Hide();
        }

        public static void OpenDebugger()
        {
            Debugger.Debugger.TryOpen();
        }

        public static void CloseDebugger()
        {
            Debugger.Debugger.TryClose();
        }

        private static IEnumerator CheckInternetAvailability()
        {
            NetworkReachability reachability = Application.internetReachability;
            while (true) {
                if (reachability != Application.internetReachability) {
                    reachability = Application.internetReachability;
                    if (reachability != NetworkReachability.NotReachable) {
                        OnInternetConnect?.Invoke();
                    }
                }

                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            AnalyticsManager.OnApplicationFocus(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            AnalyticsManager.OnApplicationPause(pauseStatus);
            AdsManager.OnApplicationPause(pauseStatus);
            AudioAdsManager.Instance.OnApplicationPause(pauseStatus);

            if (!pauseStatus) {
                // VoodooIAP.start must be called beforehand
                if (!_startCalled) {
                    return;
                }

                VoodooSauce.InvokeAppResumed();
                VoodooSauceCore.GetInAppPurchase().RefreshIapSubscriptionInfo();
            }
        }

        internal static void InvokeAfter(Action methodToCall, float duration)
        {
            _instance.StartCoroutine(InvokeAfterCoroutine(methodToCall, duration));
        }

        internal static Coroutine InvokeCoroutine(IEnumerator coroutine)
        {
            if (_instance == null) return null;
            return _instance.StartCoroutine(coroutine);
        }
        
        internal static void KillCoroutine(Coroutine coroutine)
        {
            if (_instance == null) return;
            _instance.StopCoroutine(coroutine);
        }

        private static IEnumerator InvokeAfterCoroutine(Action methodToCall, float duration)
        {
            yield return new WaitForSeconds(duration);
            methodToCall();
        }

        private void InstantiateAccessButton()
        {
            if (AccessProcess.ButtonInstance == null) {
                AccessProcess.ButtonInstance = Instantiate(_accessButton);
            }
        }

        private static void DisposeAccessButton()
        {
            if (AccessProcess.ButtonInstance != null) {
                Destroy(AccessProcess.ButtonInstance);
            }
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        private void OnDestroy()
        {
            AccessProcess.InstantiateAccessButton -= InstantiateAccessButton;
            AccessProcess.DisposeAccessButton -= DisposeAccessButton;
        }
        
        // Ensure that the code from the action is called when the VS is initialized.
        internal static void CallAfterInitialization(Action<VoodooSauceInitCallbackResult> action)
        {
            if (IsInitFinished()) {
                action(_initCallbackResult);
                return;
            }
        
            SubscribeOnInitFinishedEvent(action);
        }
    }
}