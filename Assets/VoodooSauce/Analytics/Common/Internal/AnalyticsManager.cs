using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Voodoo.Sauce.Core;
using Voodoo.Analytics;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.LoadingTime;
using Voodoo.Sauce.Privacy;
using UnityEngine;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    internal static class AnalyticsManager
    {
        private const string TAG = "VoodooAnalytics";
        private const string NO_GAME_LEVEL = "game";
        internal static bool HasGameStarted { get; private set; }

        internal static event Action<int, bool> OnGamePlayed;
        internal static event Action<bool> OnConsentChangeEvent;
        internal static event Action<GameStartedParameters> OnGameStartedEvent;
        internal static event Action<GameFinishedParameters> OnGameFinishedEvent;
        internal static event Action<string, Dictionary<string, object>, string, List<VoodooSauce.AnalyticsProvider>, Dictionary<string, object>>
            OnTrackCustomEvent;
        internal static event Action<RewardButtonShownEventAnalyticsInfo> OnRewardedVideoButtonShownEvent;
        internal static event Action<ItemTransactionParameters> OnTrackItemTransactionEvent;
        internal static event Action OnNoAdsClickEvent;
        internal static event Action OnApplicationFirstLaunchEvent;
        internal static event Action OnApplicationLaunchEvent;
        internal static event Action<AdEventAnalyticsInfo> OnBannerShownEvent;
        internal static event Action<AdEventAnalyticsInfo> OnBannerClickedEvent;
        internal static event Action<AdShownEventAnalyticsInfo> OnShowInterstitialEvent;
        internal static event Action<AdAnalyticsInfo> OnInterstitialLoadRequestEvent;
        internal static event Action<AdTriggeredEventAnalyticsInfo> OnInterstitialTriggeredEvent;
        internal static event Action<AdClickEventAnalyticsInfo> OnInterstitialClickedEvent;
        internal static event Action<AdClosedEventAnalyticsInfo> OnInterstitialDismissedEvent;
        internal static event Action<AdShowFailedEventAnalyticsInfo> OnInterstitialShowFailedEvent;
        internal static event Action<AdAnalyticsInfo> OnRewardedVideoLoadRequestEvent;
        internal static event Action<AdShownEventAnalyticsInfo> OnShowRewardedVideoEvent;
        internal static event Action<AdClickEventAnalyticsInfo> OnRewardedVideoClickedEvent;
        internal static event Action<AdClosedEventAnalyticsInfo> OnRewardedVideoClosedEvent;
        internal static event Action<AdShowFailedEventAnalyticsInfo> OnRewardedVideoShowFailedEvent;
        internal static event Action<AdShownEventAnalyticsInfo> OnShowAppOpenEvent;
        internal static event Action<AdTriggeredEventAnalyticsInfo> OnAppOpenTriggeredEvent;
        internal static event Action<AdClickEventAnalyticsInfo> OnAppOpenClickedEvent;
        internal static event Action<AdClosedEventAnalyticsInfo> OnAppOpenDismissedEvent;
        internal static event Action<AdShowFailedEventAnalyticsInfo> OnAppOpenShowFailedEvent;
        internal static event Action<AdAnalyticsInfo> OnAppOpenLoadRequestEvent;
        internal static event Action<AdShownEventAnalyticsInfo> OnMrecShownEvent;
        internal static event Action<AdClickEventAnalyticsInfo> OnMrecClickedEvent;
        internal static event Action<AdShownEventAnalyticsInfo> OnNativeAdShownEvent;
        internal static event Action<AdTriggeredEventAnalyticsInfo> OnNativeAdTriggeredEvent;
        internal static event Action<AdClickEventAnalyticsInfo> OnNativeAdClickedEvent;
        internal static event Action<AdClosedEventAnalyticsInfo> OnNativeAdDismissedEvent;
        internal static event Action<AdShowFailedEventAnalyticsInfo> OnNativeAdShowFailedEvent;
        internal static event Action<ImpressionAnalyticsInfo> OnImpressionTrackedEvent;
        internal static event Action<CrossPromoAnalyticsInfo> OnCrossPromoShownEvent;
        internal static event Action<CrossPromoAnalyticsInfo> OnCrossPromoClickEvent;
        internal static event Action<CrossPromoInitInfo> OnCrossPromoInitEvent;
        internal static event Action<string> OnCrossPromoErrorEvent;
        internal static event Action<VoodooTuneInitAnalyticsInfo> OnTrackVoodooTuneInitEvent;
        internal static event Action<VoodooTuneAbTestAnalyticsInfo> OnTrackVoodooTuneAbTestAssignmentEvent;
        internal static event Action<VoodooTuneAbTestAnalyticsInfo> OnTrackVoodooTuneAbTestExitEvent;
        internal static event Action<PerformanceMetricsAnalyticsInfo> OnTrackPerformanceMetricsEvent;
        internal static event Action<string, string, int> OnTrackVoodooFunnelEvent;
        internal static event Action<ConversionEventInfo> OnTrackConversionEvent;
        internal static event Action<AttributionAnalyticsInfo> OnAttributionChangedEvent;
        internal static event Action<LoadingTimeAnalyticsInfo> OnGameInteractableEvent;
        internal static event Action<LoadingTimeAnalyticsInfo> OnVoodooSauceSDKInitializedEvent;
        internal static event Action<LoadingTimeAnalyticsInfo> OnUnityEngineStartedEvent;

        // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
        internal static IAudioAdsAnalyticsManager AudioAds => _audioAdsAnalyticsManager ?? 
            (_audioAdsAnalyticsManager = new AudioAdsAnalyticsManager());

        //IAP Events
        internal static event Action<VoodooIAPAnalyticsInfo> OnPurchaseStarted;
        internal static event Action<VoodooIAPAnalyticsInfo> OnPurchaseProcessing;
        internal static event Action<VoodooIAPAnalyticsInfo> OnPurchaseFailed;
        internal static event Action<VoodooIAPAnalyticsInfo> OnPurchaseValidated;
        internal static event Action<VoodooIAPAnalyticsInfo> OnPurchaseRewarded;
        internal static event Action<VoodooIAPAnalyticsInfo, IAPServerError> OnPurchaseServerError;
        internal static event Action<VoodooIAPAnalyticsInfo> OnTrackPurchaseEvent;

        internal static event Action<BackupAdsAnalyticsInfo> OnBackupAdsClickEvent;
        internal static event Action<CrossPromoInitInfo> OnBackupAdsInitEvent;
        internal static event Action<BackupAdsAnalyticsInfo> OnBackupAdsShownEvent;
        
        internal static event Action OnCloseBannerClick;
        internal static event Action OnCloseBannerPurchase;
        internal static event Action OnCloseBannerCancel;

        private static readonly VoodooSettings Settings = VoodooSettings.Load();
        private static AudioAdsAnalyticsManager _audioAdsAnalyticsManager;

        private static readonly List<VoodooSauce.AnalyticsProvider> DefaultAnalyticsProvider = new List<VoodooSauce.AnalyticsProvider>
        {
            VoodooSauce.AnalyticsProvider.GameAnalytics,
            VoodooSauce.AnalyticsProvider.FirebaseAnalytics,
            VoodooSauce.AnalyticsProvider.VoodooAnalytics

        };

        private static readonly List<IAnalyticsProvider> AnalyticsProviders = new List<IAnalyticsProvider> {
            new GameAnalyticsProvider(true),
            new FirebaseAnalyticsProvider(Settings.UseFirebaseAnalytics, AdsManager.MediationAdapter.GetMediationType().ToString()),
            new MixpanelAnalyticsProvider(new MixpanelParameters(Settings.MixpanelProdToken, Settings.UseMixpanel(), Settings.UseRemoteConfig)),
            new AdjustAnalyticsProvider(new AdjustParameters(Settings.AdjustIosAppToken, Settings.AdjustAndroidAppToken, Settings.NoAdsBundleId)),
            new VoodooAnalyticsProvider(new VoodooAnalyticsParameters(Settings.UseRemoteConfig,
                Settings.UseVoodooAnalytics,
                Settings.LegacyAbTestName,
                EnvironmentSettings.GetProxyServer())),
            new AdnAnalyticsProvider()
        };

        private static IEnumerable<IAnalyticsAttributionProvider> AnalyticsAttributionProviders()
        {
            return AnalyticsProviders.Select(provider => provider as IAnalyticsAttributionProvider)
                                     .Where(provider => provider != null)
                                     .ToList();
        }

        private static readonly AnalyticsEventTimer _gameTimer = new AnalyticsEventTimer();
        private static readonly AnalyticsEventTimer _adsTimer = new AnalyticsEventTimer();

        internal static readonly LoadingTimerManager LoadingTimes = new LoadingTimerManager();

        internal static void Instantiate(string mediation)
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Instantiating Analytics");
            // Initialize providers
            AnalyticsProviders.ForEach(provider => provider.Instantiate(mediation));
            InitializePerformanceMetricMonitor();
            ConversionEventsManager.Instance.Initialize(Settings.ConversionEvents);
        }

        private static void InitializePerformanceMetricMonitor()
        {
            var metricsConfig = VoodooSauce.GetItemOrDefault<PerformanceMetricsConfig>();
            if (metricsConfig.active)
                PerformanceMetricsManager.Initialize(metricsConfig.period);
        }

        internal static void Initialize(PrivacyCore.GdprConsent consent)
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Initializing Analytics");
            // Initialize providers
            AnalyticsProviders.ForEach(provider => provider.Initialize(consent, Settings.IsChinaStore()));
        }

        internal static AttributionData GetAttributionData()
        {
            foreach (IAnalyticsAttributionProvider provider in AnalyticsAttributionProviders()) {
                AttributionData attributionData = provider.GetAttributionData();
                if (attributionData != null) {
                    return attributionData;
                }
            }

            return null;
        }

        internal static void SetConsent(bool consent)
        {
            OnConsentChangeEvent?.Invoke(consent);
        }

        internal static void SetLogLevel(bool enable, LogType globalLevel)
        {
            AnalyticsLog.AnalyticsLogLevel logLevel;
            if (enable) {
                logLevel = globalLevel == LogType.Error ? AnalyticsLog.AnalyticsLogLevel.ERROR :
                    globalLevel == LogType.Warning ? AnalyticsLog.AnalyticsLogLevel.WARNING : AnalyticsLog.AnalyticsLogLevel.DEBUG;
            } else {
                logLevel = AnalyticsLog.AnalyticsLogLevel.DISABLED;
            }

            AnalyticsLog.SetLogLevel(logLevel);
        }
        
#region Track IAP Transaction

        internal static void TrackPurchaseStarted(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseStarted?.Invoke(payload);
        }

        internal static void TrackPurchaseProcessing(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseProcessing?.Invoke(payload);
        }

        internal static void TrackPurchaseFailed(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseFailed?.Invoke(payload);
        }

        internal static void TrackPurchaseValidated(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseValidated?.Invoke(payload);
        }

        internal static void TrackPurchase(VoodooIAPAnalyticsInfo payload)
        {
            OnTrackPurchaseEvent?.Invoke(payload);
        }

        internal static void TrackPurchaseRewarded(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseRewarded?.Invoke(payload);
        }

        internal static void TrackPurchaseServerError(VoodooIAPAnalyticsInfo payload, IAPServerError serverError)
        {
            OnPurchaseServerError?.Invoke(payload, serverError);
        }

#endregion

#region Track Game Events

        internal static void TrackApplicationLaunch()
        {
            AnalyticsStorageHelper.Instance.IncrementAppLaunchCount();
            //fire app launch events
            if (AnalyticsStorageHelper.Instance.IsFirstAppLaunch() || AbTestManager.IsDebugModeForced()) {
                AnalyticsStorageHelper.Instance.SaveFirstAppLaunchDate();
                OnApplicationFirstLaunchEvent?.Invoke();
            }

            OnApplicationLaunchEvent?.Invoke();
        }

        internal static void TrackGameStarted(GameStartedParameters parameters)
        {
            HasGameStarted = true;
            _gameTimer.Start();
            AnalyticsStorageHelper.Instance.UpdateCurrentLevel(parameters.level);
            AnalyticsStorageHelper.Instance.IncrementGameCount();
            
            AnalyticsStorageHelper.Instance.ResetRvUsed();

            if (string.IsNullOrEmpty(parameters.levelDefinitionID) && !string.IsNullOrEmpty(parameters.level))
                parameters.levelDefinitionID = parameters.level;

            OnGameStartedEvent?.Invoke(parameters);
        }

        public static void TrackGamePauseButtonClicked(bool pauseStatus)
        {
            PauseGameTimer(pauseStatus);
        }

        internal static void TrackRewardedVideoButtonShown(RewardButtonShownEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnRewardedVideoButtonShownEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackGameFinished(GameFinishedParameters parameters)
        {
            _gameTimer.Stop();
            AnalyticsStorageHelper.Instance.UpdateCurrentLevel(parameters.level);
            if (parameters.status) {
                // used to calculate the win rate (for VoodooTune)
                AnalyticsStorageHelper.Instance.IncrementSuccessfulGameCount();
            }

            bool newHighestScore = AnalyticsStorageHelper.Instance.UpdateGameHighestScore(parameters.score);
            OnGamePlayed?.Invoke(AnalyticsStorageHelper.Instance.GetGameCount(), newHighestScore);

            int gameDuration = _gameTimer.GetDuration();
            parameters.level = parameters.level ?? NO_GAME_LEVEL;
            parameters.gameDuration = gameDuration;

            if (string.IsNullOrEmpty(parameters.levelDefinitionID) && !string.IsNullOrEmpty(parameters.level))
                parameters.levelDefinitionID = parameters.level;

            if (!HasGameStarted) {
                VoodooLog.LogWarning(Module.ANALYTICS, TAG, "Game Finish was called before Game Started. Ignoring event.");
            } else {
                HasGameStarted = false;
                OnGameFinishedEvent?.Invoke(parameters);
            }
            
            AnalyticsStorageHelper.Instance.ResetRvUsed();
        }

        internal static void TrackCustomEvent(string eventName,
                                              Dictionary<string, object> eventProperties,
                                              string type = null,
                                              List<VoodooSauce.AnalyticsProvider> analyticsProviders = null,
                                              Dictionary<string, object> contextVariables = null)
        {
            if (analyticsProviders == null || analyticsProviders.Count == 0) {
                analyticsProviders = DefaultAnalyticsProvider;
            }

            OnTrackCustomEvent?.Invoke(eventName, eventProperties, type, analyticsProviders, contextVariables);
        }

        private static void PauseGameTimer(bool pauseStatus)
        {
            if (pauseStatus) _gameTimer?.Pause();
            else _gameTimer?.Resume();
        }

#endregion

#region Track NoAds Events

        internal static void TrackNoAdsClick()
        {
            OnNoAdsClickEvent?.Invoke();
        }

#endregion

#region Track purchase Events

        internal static void TrackItemTransaction(ItemTransactionParameters parameters)
        {
            OnTrackItemTransactionEvent?.Invoke(parameters);
        }

#endregion

#region Track Ads Events

        internal static void TrackBannerShown(AdEventAnalyticsInfo adAnalyticsInfo)
        {
            OnBannerShownEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackBannerClick(AdEventAnalyticsInfo adAnalyticsInfo)
        {
            OnBannerClickedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackShowInterstitial(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Start();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnShowInterstitialEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackShowAppOpen(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Start();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnShowAppOpenEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackInterstitialTriggered(AdTriggeredEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnInterstitialTriggeredEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackAppOpenTriggered(AdTriggeredEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnAppOpenTriggeredEvent?.Invoke(adAnalyticsInfo);
        }
        
        internal static void TrackInterstitialLoadRequest(AdAnalyticsInfo adAnalyticsInfo)
        {
            OnInterstitialLoadRequestEvent?.Invoke(adAnalyticsInfo);
        }
        internal static void TrackRewardedVideoLoadRequest(AdAnalyticsInfo adAnalyticsInfo)
        {
            OnRewardedVideoLoadRequestEvent?.Invoke(adAnalyticsInfo);
        }
        internal static void TrackAppOpenLoadRequest(AdAnalyticsInfo adAnalyticsInfo)
        {
            OnAppOpenLoadRequestEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackInterstitialClick(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnInterstitialClickedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackInterstitialDismiss(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            OnInterstitialDismissedEvent?.Invoke(adAnalyticsInfo);
            ConversionEventInfo conversionEvent = ConversionEventsManager.Instance.GetInterstitialConversionEvent(adAnalyticsInfo);
            if (conversionEvent != null) OnTrackConversionEvent?.Invoke(conversionEvent);
        }

        internal static void TrackInterstitialShowFailed(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            OnInterstitialShowFailedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackAppOpenClick(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnAppOpenClickedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackAppOpenDismiss(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            OnAppOpenDismissedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackAppOpenShowFailed(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            OnAppOpenShowFailedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackShowRewardedVideo(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Start();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnShowRewardedVideoEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackRewardedVideoClick(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnRewardedVideoClickedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackRewardedVideoClose(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            if (adAnalyticsInfo.AdReward == true) AnalyticsStorageHelper.Instance.IncreaseRvUsed();
            OnRewardedVideoClosedEvent?.Invoke(adAnalyticsInfo);
            ConversionEventInfo conversionEvent = ConversionEventsManager.Instance.GetRewardedVideoConversionEvent(adAnalyticsInfo);
            if (conversionEvent != null) OnTrackConversionEvent?.Invoke(conversionEvent);
        }
        
        internal static void TrackRewardedVideoShowFailed(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            OnRewardedVideoShowFailedEvent?.Invoke(adAnalyticsInfo);
        }
        
        internal static void TrackMrecShown(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            OnMrecShownEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackMrecClick(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            OnMrecClickedEvent?.Invoke(adAnalyticsInfo);
        }
          
        internal static void TrackNativeAdShown(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Start();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnNativeAdShownEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackNativeAdTriggered(AdTriggeredEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnNativeAdTriggeredEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackNativeAdClick(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            OnNativeAdClickedEvent?.Invoke(adAnalyticsInfo);
        }
        
        internal static void TrackNativeAdDismiss(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            OnNativeAdDismissedEvent?.Invoke(adAnalyticsInfo);
        }
        
        internal static void TrackNativeAdShowFailed(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            _adsTimer.Stop();
            adAnalyticsInfo.GameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            adAnalyticsInfo.AdDuration = _adsTimer.GetDuration();
            OnNativeAdShowFailedEvent?.Invoke(adAnalyticsInfo);
        }

        internal static void TrackImpression(ImpressionAnalyticsInfo impressionInfo)
        {
            OnImpressionTrackedEvent?.Invoke(impressionInfo);
        }

#endregion

#region Track CrossPromo Events

        internal static void TrackCrossPromoShown(CrossPromoAnalyticsInfo crossPromoInfo)
        {
            OnCrossPromoShownEvent?.Invoke(crossPromoInfo);
        }

        internal static void TrackCrossPromoClick(CrossPromoAnalyticsInfo crossPromoInfo)
        {
            OnCrossPromoClickEvent?.Invoke(crossPromoInfo);
        }

        internal static void TrackCrossPromoError(string error)
        {
            OnCrossPromoErrorEvent?.Invoke(error);
        }

        internal static void TrackCrossPromoInit(CrossPromoInitInfo info)
        {
            OnCrossPromoInitEvent?.Invoke(info);
        }

#endregion

#region Track VoodooTune Event

        internal static void TrackVoodooTuneInitEvent(VoodooTuneInitAnalyticsInfo info)
        {
            OnTrackVoodooTuneInitEvent?.Invoke(info);
        }

        public static void TrackVoodooTuneAbTestAssignment(VoodooTuneAbTestAnalyticsInfo abTestAnalyticsInfo)
        {
            OnTrackVoodooTuneAbTestAssignmentEvent?.Invoke(abTestAnalyticsInfo);
        }

        public static void TrackVoodooTuneAbTestExit(VoodooTuneAbTestAnalyticsInfo abTestAnalyticsInfo)
        {
            OnTrackVoodooTuneAbTestExitEvent?.Invoke(abTestAnalyticsInfo);
        }

#endregion

#region Track Metrics Event

        internal static void TrackPerformanceMetrics(PerformanceMetricsAnalyticsInfo info)
        {
            OnTrackPerformanceMetricsEvent?.Invoke(info);
        }

#endregion

#region Analytics Debugger

        [CanBeNull]
        internal static BaseAnalyticsProviderWithLogger GetProviderLoggerWithEnum(VoodooSauce.AnalyticsProvider providerEnum)
        {
            foreach (IAnalyticsProvider analyticsProvider in AnalyticsProviders) {
                if (analyticsProvider is BaseAnalyticsProviderWithLogger providerLogger && providerLogger.GetProviderEnum() == providerEnum) {
                    return providerLogger;
                }
            }

            return null;
        }

#endregion

#region Track Voodoo Funnel

        internal static void TrackVoodooFunnel(string funnelName, string stepName, int stepPosition)
        {
            OnTrackVoodooFunnelEvent?.Invoke(funnelName, stepName, stepPosition);
        }

#endregion

#region Track Attribution

        internal static void TrackAttribution(AttributionAnalyticsInfo attributionInfo)
        {
            OnAttributionChangedEvent?.Invoke(attributionInfo);
        }

#endregion

#region Loading Time

        internal static void OnGameInteractable()
        {
            // The timer must be stopped first.
            LoadingTimes.StopGlobalLoadingTimer();
            
            long duration = LoadingTimes.GetRealGlobalLoadingTimeDuration();
            if (duration < 0) {
                return;
            }

            OnGameInteractableEvent?.Invoke(new LoadingTimeAnalyticsInfo {
                duration = duration,
                start = LoadingTimes.GetGlobalLoadingTimeStartTimestamp(),
                end = LoadingTimes.GetGlobalLoadingTimeEndTimestamp(),
                sessionId = LoadingTimes.GetSessionId(),
            });
        }

        internal static void StartTrackingVoodooSauceSDKInitializationLoadingTime()
        {
            LoadingTimes.StartVoodooSauceSDKLoadingTimer();
        }

        internal static void TrackVoodooSauceSDKInitializationLoadingTime()
        {
            LoadingTimes.StopVoodooSauceSDKLoadingTimer();
            long duration = LoadingTimes.GetVoodooSauceSDKRealLoadingTimeDuration();
            if (duration < 0) {
                return;
            }

            OnVoodooSauceSDKInitializedEvent?.Invoke(new LoadingTimeAnalyticsInfo {
                duration = duration,
                start = LoadingTimes.GetVoodooSauceSDKLoadingTimeStartTimestamp(),
                end = LoadingTimes.GetVoodooSauceSDKLoadingTimeEndTimestamp(),
                sessionId = LoadingTimes.GetSessionId(),
            });
        }

        internal static void TrackUnityEngineStarted()
        {
            // The timer must be stopped first.
            LoadingTimes.StopUnityLoadingTimer();
            
            long duration = LoadingTimes.GetUnityLoadingTimeDuration();
            if (duration < 0) {
                return;
            }

            OnUnityEngineStartedEvent?.Invoke(new LoadingTimeAnalyticsInfo {
                duration = duration,
                start = LoadingTimes.GetUnityLoadingTimeStartTimestamp(),
                end = LoadingTimes.GetUnityLoadingTimeEndTimestamp(),
                sessionId = LoadingTimes.GetSessionId(),
            });
        }

        internal static void StartPrivacyDisplayingTimer()
        {
            LoadingTimes.StartPrivacyDisplayingTimer();
        }

        internal static void StopPrivacyDisplayingTimer()
        {
            LoadingTimes.StopPrivacyDisplayingTimer();
        }

        internal static void StartAttDisplayingTimer()
        {
            LoadingTimes.StartAttDisplayingTimer();
        }

        internal static void StopAttDisplayingTimer()
        {
            LoadingTimes.StopAttDisplayingTimer();
        }
        
        internal static void PauseTrackingLoadingTimes()
        {
            LoadingTimes.Pause();
        }

        internal static void UnpauseTrackingLoadingTimes()
        {
            LoadingTimes.Unpause();
        }

#endregion

#region Track Backup FS

        internal static void TrackBackupAdsClick(BackupAdsAnalyticsInfo backupAdsInfo)
        {
            OnBackupAdsClickEvent?.Invoke(backupAdsInfo);
        }

        internal static void TrackBackupAdsInit(CrossPromoInitInfo backupfsInitInfo)
        {
            OnBackupAdsInitEvent?.Invoke(backupfsInitInfo);
        }

        internal static void TrackBackupAdsShown(BackupAdsAnalyticsInfo backupAdsInfo)
        {
            OnBackupAdsShownEvent?.Invoke(backupAdsInfo);
        }

#endregion

#region Game Thread lifecycle

        public static void OnApplicationPause(bool pauseStatus)
        {
            //DO not pause/resume _adsTimer, OnApplicationPause is not triggered when ads are displayed , ads use a new page/thread
            PauseGameTimer(pauseStatus);
            PerformanceMetricsManager.OnApplicationPause(pauseStatus);
            if (pauseStatus) {
                PauseTrackingLoadingTimes();
            } else {
                // Brought forward after soft closing
                UnpauseTrackingLoadingTimes();
            }
        }

        public static void OnApplicationFocus(bool hasFocus)
        {
            //DO not pause/resume _adsTimer, OnApplicationFocus is not triggered when ads are displayed , ads use a new page/thread
            PauseGameTimer(!hasFocus);
        }

#endregion
        
#region Track Close Banner Events

        internal static void TrackCloseBannerClick()
        {
            OnCloseBannerClick?.Invoke();
        }
        
        internal static void TrackCloseBannerPurchase()
        {
            OnCloseBannerPurchase?.Invoke();
        }
        
        internal static void TrackCloseBannerCancel()
        {
            OnCloseBannerCancel?.Invoke();
        }

#endregion
    }
}
