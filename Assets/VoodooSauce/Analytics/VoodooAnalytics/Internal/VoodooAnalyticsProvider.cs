using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Analytics;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Privacy;
using VAC = Voodoo.Sauce.Internal.Analytics.VoodooAnalyticsConstants;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    internal class VoodooAnalyticsProvider : BaseAnalyticsProviderWithLogger
    {
        private readonly VoodooAnalyticsParameters _parameters;

        // Needed for the VoodooGDPRAnalytics class. Do not call it directly.
        public VoodooAnalyticsProvider() { }

        internal VoodooAnalyticsProvider(VoodooAnalyticsParameters parameters)
        {
            _parameters = parameters;
            if (!_parameters.UseVoodooAnalytics) return;
            VoodooAnalyticsManager.SetSessionInfoProvider(parameters.GetSessionInfo);
            RegisterEvents();
        }

        internal override VoodooSauce.AnalyticsProvider GetProviderEnum() =>
            VoodooSauce.AnalyticsProvider.VoodooAnalytics;

        public override void Instantiate(string mediation)
        {
            if (!_parameters.UseVoodooAnalytics) return;

            AnalyticsEventLogger.GetInstance().Init();
            VoodooAnalyticsWrapper.Instantiate(
                VoodooAnalyticsConfig.AnalyticsConfig ?? VoodooSauce.GetItemOrDefault<AnalyticsConfig>(),
                _parameters,
                mediation);
            IsInitialized = true;
        }

        public override void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild) { }

        private static void RegisterEvents()
        {
            AnalyticsManager.OnApplicationFirstLaunchEvent += OnApplicationFirstLaunch;
            AnalyticsManager.OnApplicationLaunchEvent += OnApplicationLaunchEvent;
            AnalyticsManager.OnGameStartedEvent += OnGameStarted;
            AnalyticsManager.OnGameFinishedEvent += OnGameFinished;
            AnalyticsManager.OnTrackCustomEvent += TrackCustomEvent;
            AnalyticsManager.OnRewardedVideoButtonShownEvent += OnRewardedVideoButtonShownEvent;
            // Activate this line if we want to track purchase without partner verification
            AnalyticsManager.OnNoAdsClickEvent += OnNoAdsClickEvent;
            AnalyticsManager.OnCrossPromoShownEvent += OnCrossPromoShownEvent;
            AnalyticsManager.OnCrossPromoClickEvent += OnCrossPromoClickEvent;
            AnalyticsManager.OnTrackItemTransactionEvent += OnTrackItemTransactionEvent;
            AnalyticsManager.OnCrossPromoInitEvent += OnCrossPromoInitEvent;

            AnalyticsManager.OnBannerShownEvent += OnBannerShownEvent;
            AnalyticsManager.OnBannerClickedEvent += OnBannerClickedEvent;
            AnalyticsManager.OnInterstitialClickedEvent += OnInterstitialClickedEvent;
            AnalyticsManager.OnInterstitialDismissedEvent += OnInterstitialDismissedEvent;
            AnalyticsManager.OnInterstitialShowFailedEvent += OnInterstitialShowFailedEvent;
            AnalyticsManager.OnShowInterstitialEvent += OnShowInterstitialEvent;
            AnalyticsManager.OnInterstitialTriggeredEvent += OnInterstitialTriggeredEvent;
            AnalyticsManager.OnRewardedVideoClickedEvent += OnRewardedVideoClickedEvent;
            AnalyticsManager.OnRewardedVideoClosedEvent += OnRewardedVideoClosedEvent;
            AnalyticsManager.OnShowRewardedVideoEvent += OnShowRewardedVideoEvent;
            AnalyticsManager.OnRewardedVideoShowFailedEvent += OnRewardedVideoShowFailedEvent;
            AnalyticsManager.AudioAds.OnAudioAdTriggerEvent += OnTriggerAudioAdEvent;
            AnalyticsManager.AudioAds.OnAudioAdShownEvent += OnShowAudioAdEvent;
            AnalyticsManager.AudioAds.OnAudioAdImpressionEvent += OnImpressionAudioAdEvent;
            AnalyticsManager.AudioAds.OnAudioAdClickEvent += OnClickAudioAdEvent;
            AnalyticsManager.AudioAds.OnAudioAdWatchedEvent += OnWatchedAudioAdEvent;
            AnalyticsManager.AudioAds.OnAudioAdCloseEvent += OnCloseAudioAdEvent;
            AnalyticsManager.AudioAds.OnAudioAdFailedEvent += OnFailedAudioAdEvent;
            AnalyticsManager.OnMrecShownEvent += OnMrecShownEvent;
            AnalyticsManager.OnMrecClickedEvent += OnMrecClickedEvent;
            AnalyticsManager.OnNativeAdTriggeredEvent += OnNativeAdTriggeredEvent;
            AnalyticsManager.OnNativeAdShownEvent += OnNativeAdShownEvent;
            AnalyticsManager.OnNativeAdClickedEvent += OnNativeAdClickedEvent;
            AnalyticsManager.OnNativeAdDismissedEvent += OnNativeAdDismissedEvent;
            AnalyticsManager.OnNativeAdShowFailedEvent += OnNativeAdShowFailedEvent;
            
            AnalyticsManager.OnImpressionTrackedEvent += OnImpressionTrackedEvent;
            AnalyticsManager.OnTrackVoodooTuneInitEvent += TrackVoodooTuneInitEvent;
            AnalyticsManager.OnTrackPerformanceMetricsEvent += TrackPerformanceMetrics;
            AnalyticsManager.OnTrackVoodooFunnelEvent += TrackVoodooFunnel;
            AnalyticsManager.OnAttributionChangedEvent += OnAttributionChange;
            AnalyticsManager.OnTrackVoodooTuneAbTestAssignmentEvent += TrackVoodooTuneAbTestAssignment;
            AnalyticsManager.OnTrackVoodooTuneAbTestExitEvent += TrackVoodooTuneAbTestExit;
            AnalyticsManager.OnBackupAdsClickEvent += OnBackupAdsClickEvent;
            AnalyticsManager.OnBackupAdsInitEvent += OnBackupAdsInitEvent;
            AnalyticsManager.OnBackupAdsShownEvent += OnBackupAdsShown;

            AnalyticsManager.OnGameInteractableEvent += OnGameInteractable;
            AnalyticsManager.OnVoodooSauceSDKInitializedEvent += OnVoodooSauceSDKInitialized;
            AnalyticsManager.OnUnityEngineStartedEvent += OnUnityEngineStarted;
            
            //Registering to IAP events
            AnalyticsManager.OnPurchaseStarted += OnPurchaseStarted;
            AnalyticsManager.OnPurchaseProcessing += OnPurchaseProcessing;
            AnalyticsManager.OnPurchaseFailed += OnPurchaseFailed;
            AnalyticsManager.OnPurchaseValidated += OnPurchaseValidated;
            AnalyticsManager.OnPurchaseRewarded += OnPurchaseRewarded;
            AnalyticsManager.OnPurchaseServerError += OnPurchaseServerError;
            AnalyticsManager.OnTrackPurchaseEvent += OnTrackPurchaseEvent;

            AnalyticsManager.OnCloseBannerClick += OnCloseBannerClick;
            AnalyticsManager.OnCloseBannerPurchase += OnCloseBannerPurchase;
            AnalyticsManager.OnCloseBannerCancel += OnCloseBannerCancel;
            
            AnalyticsManager.OnAppOpenClickedEvent += OnAppOpenClickedEvent;
            AnalyticsManager.OnAppOpenDismissedEvent += OnAppOpenDismissedEvent;
            AnalyticsManager.OnAppOpenShowFailedEvent += OnAppOpenShowFailedEvent;
            AnalyticsManager.OnShowAppOpenEvent += OnShowAppOpenEvent;
            AnalyticsManager.OnAppOpenTriggeredEvent += OnAppOpenTriggeredEvent;
        }

        private static void OnNoAdsClickEvent()
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.noads_click);
        }

#region Banner

        private static void OnCloseBannerClick()
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.close_banner_click);
        }
        
        private static void OnCloseBannerPurchase()
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.close_banner_purchase);
        }
        
        private static void OnCloseBannerCancel()
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.close_banner_cancel);
        }

        private static void OnBannerShownEvent(AdEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.banner_shown, data);
        }

        private static void OnBannerClickedEvent(AdEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.banner_click, data);
        }

#endregion
        
#region Interstitial
        
        private static void OnInterstitialClickedEvent(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.INTERSTITIAL_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV, adAnalyticsInfo.IsFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV_REASON, adAnalyticsInfo.ReasonFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.fs_click, data);
        }

        private static void OnShowInterstitialEvent(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.INTERSTITIAL_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV, adAnalyticsInfo.IsFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV_REASON, adAnalyticsInfo.ReasonFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.fs_shown, data);
        }

        private static void OnInterstitialTriggeredEvent(AdTriggeredEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.INTERSTITIAL_TYPE, adAnalyticsInfo.AdTag },
                { VAC.INTERSTITIAL_LOADED, adAnalyticsInfo.AdLoaded },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV, adAnalyticsInfo.IsFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV_REASON, adAnalyticsInfo.ReasonFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            data.AddIfNotNull(VAC.FS_CPM, adAnalyticsInfo.InterstitialCpm);
            data.AddIfNotNull(VAC.RV_CPM, adAnalyticsInfo.RewardedVideoCpm);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.fs_trigger, data);
        }

        private static void OnInterstitialDismissedEvent(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.INTERSTITIAL_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_DURATION, adAnalyticsInfo.AdDuration },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV, adAnalyticsInfo.IsFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.FS_INSTEAD_RV_REASON, adAnalyticsInfo.ReasonFsShownInsteadOfRv);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.fs_watched, data);
        }
        
        private static void OnInterstitialShowFailedEvent(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.INTERSTITIAL_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_DURATION, adAnalyticsInfo.AdDuration },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.ERROR_MESSAGE, adAnalyticsInfo.ErrorMessage);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_MESSAGE, adAnalyticsInfo.AdNetworkErrorString);
            data.AddIfNotNull(VAC.ERROR_CODE, adAnalyticsInfo.ErrorCode);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_CODE, adAnalyticsInfo.AdNetworkErrorCode);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.fs_failed, data);
        }

#endregion

#region Rewarded Video

        private static void OnRewardedVideoClickedEvent(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.REWARDED_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.rv_click, data);
        }

        private static void OnShowRewardedVideoEvent(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.REWARDED_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.rv_shown, data);
        }
        
        private static void OnRewardedVideoShowFailedEvent(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.REWARDED_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_DURATION, adAnalyticsInfo.AdDuration },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.ERROR_MESSAGE, adAnalyticsInfo.ErrorMessage);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_MESSAGE, adAnalyticsInfo.AdNetworkErrorString);
            data.AddIfNotNull(VAC.ERROR_CODE, adAnalyticsInfo.ErrorCode);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_CODE, adAnalyticsInfo.AdNetworkErrorCode);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.rv_failed, data);
        }

        private static void OnRewardedVideoClosedEvent(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.REWARDED_TYPE, adAnalyticsInfo.AdTag },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_DURATION, adAnalyticsInfo.AdDuration },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.rv_watched, data);
        }

        private static void OnRewardedVideoButtonShownEvent(RewardButtonShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.REWARDED_TYPE, adAnalyticsInfo.AdTag },
                { VAC.REWARDED_LOADED, adAnalyticsInfo.AdLoaded },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            data.AddIfNotNull(VAC.FS_CPM, adAnalyticsInfo.InterstitialCpm);
            data.AddIfNotNull(VAC.RV_CPM, adAnalyticsInfo.RewardedVideoCpm);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.rv_trigger, data);
        }
        
#endregion

#region AppOpen
        
        private static void OnAppOpenClickedEvent(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.APP_OPEN_TYPE, AppOpen.AdType.soft_launch.ToString() },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ao_clicked, data);
        }

        private static void OnShowAppOpenEvent(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.APP_OPEN_TYPE, AppOpen.AdType.soft_launch.ToString() },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ao_shown, data);
        }

        private static void OnAppOpenTriggeredEvent(AdTriggeredEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.APP_OPEN_LOADED, adAnalyticsInfo.AdLoaded },
                { VAC.APP_OPEN_TYPE, AppOpen.AdType.soft_launch.ToString() },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ao_trigger, data);
        }

        private static void OnAppOpenDismissedEvent(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.APP_OPEN_TYPE, AppOpen.AdType.soft_launch.ToString() },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.AD_DURATION, adAnalyticsInfo.AdDuration },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ao_watched, data);
        }
        
        private static void OnAppOpenShowFailedEvent(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.APP_OPEN_TYPE, AppOpen.AdType.soft_launch.ToString() },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime },
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            data.AddIfNotNull(VAC.ERROR_MESSAGE, adAnalyticsInfo.ErrorMessage);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_MESSAGE, adAnalyticsInfo.AdNetworkErrorString);
            data.AddIfNotNull(VAC.ERROR_CODE, adAnalyticsInfo.ErrorCode);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_CODE, adAnalyticsInfo.AdNetworkErrorCode);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ao_failed, data);
        }

#endregion

#region AudioAds

        private static void OnTriggerAudioAdEvent(AudioAdTriggerAnalyticsInfo info)
        {
            Dictionary<string, object> data = CreateAudioAdDataDictionary(info);
            data.Add(VAC.AUDIO_AD_LOADED, info.AdLoaded);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.audio_ad_trigger, data);
        }

        private static void OnShowAudioAdEvent(AudioAdAnalyticsInfo info)
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.audio_ad_shown, CreateAudioAdDataDictionary(info));
        }

        private static void OnImpressionAudioAdEvent(AudioAdImpressionAnalyticsInfo info)
        {
            Dictionary<string, object> data = CreateAudioAdDataDictionary(info, false);
            data.Add(VAC.PUBLISHER_REVENUE, info.revenue);
            data.Add(VAC.ADUNIT_FORMAT, info.adUnitFormat);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ad_revenue, data);
        }

        private static void OnClickAudioAdEvent(AudioAdAnalyticsInfo info)
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.audio_ad_clicked, CreateAudioAdDataDictionary(info));
        }

        private static void OnWatchedAudioAdEvent(AudioAdAnalyticsInfo info)
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.audio_ad_watched, CreateAudioAdDataDictionary(info));
        }

        private static void OnCloseAudioAdEvent(AudioAdAnalyticsInfo info)
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.audio_ad_closed, CreateAudioAdDataDictionary(info));
        }

        private static void OnFailedAudioAdEvent(AudioAdFailedAnalyticsInfo info)
        {
            Dictionary<string, object> data = CreateAudioAdDataDictionary(info);
            data.AddIfNotNull(VAC.ERROR_CODE, info.errorCode);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.audio_ad_failed, data);
        }

        private static Dictionary<string, object> CreateAudioAdDataDictionary(AudioAdAnalyticsInfo info, bool hasGameCount = true)
        {
            var dictionary =  new Dictionary<string, object> {
                { VAC.NETWORK_NAME, info.networkName },
                { VAC.AUDIO_ADS_TYPE, info.audioAdType },
                { VAC.IMPRESSION_ID, info.impressionId },
                { VAC.AD_COUNT, info.adCount }
            };
            
            if (hasGameCount) {
                dictionary.Add(VAC.GAME_COUNT, info.gameCount);
            }
            
            return dictionary;
        }

#endregion
        
#region MRec
        private static void OnMrecShownEvent(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                {VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName},
                {VAC.AD_COUNT, adAnalyticsInfo.AdCount},
                {VAC.MREC_TYPE, adAnalyticsInfo.AdTag}
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.thumbnail_shown, data);
        }

        private static void OnMrecClickedEvent(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                {VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName},
                {VAC.AD_COUNT, adAnalyticsInfo.AdCount},
                {VAC.MREC_TYPE, adAnalyticsInfo.AdTag}
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.thumbnail_click, data);
        }
        
#endregion

#region NativeAd

        private static void OnNativeAdTriggeredEvent(AdTriggeredEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                {VAC.NATIVE_TYPE, adAnalyticsInfo.AdTag},
                {VAC.NATIVE_LOADED, adAnalyticsInfo.AdLoaded},
                {VAC.AD_COUNT, adAnalyticsInfo.AdCount},
                {VAC.GAME_COUNT, adAnalyticsInfo.GameCount},
                {VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName}
            };
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.native_trigger, data);
        }
        
        private static void OnNativeAdShownEvent(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                {VAC.NATIVE_TYPE, adAnalyticsInfo.AdTag},
                {VAC.GAME_COUNT, adAnalyticsInfo.GameCount},
                {VAC.AD_COUNT, adAnalyticsInfo.AdCount},
                {VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName}
            };
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.native_shown, data);
        }

        private static void OnNativeAdClickedEvent(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                {VAC.NATIVE_TYPE, adAnalyticsInfo.AdTag},
                {VAC.GAME_COUNT, adAnalyticsInfo.GameCount},
                {VAC.AD_COUNT, adAnalyticsInfo.AdCount},
                {VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName}
            };
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.native_click, data);
        }
        
        private static void OnNativeAdDismissedEvent(AdClosedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                {VAC.NATIVE_TYPE, adAnalyticsInfo.AdTag},
                {VAC.GAME_COUNT, adAnalyticsInfo.GameCount},
                {VAC.AD_DURATION, adAnalyticsInfo.AdDuration},
                {VAC.AD_COUNT, adAnalyticsInfo.AdCount},
                {VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName},
            };
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.AD_UNIT_WATERFALL_NAME, adAnalyticsInfo.WaterfallName);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.native_closed, data);
        }
        
        private static void OnNativeAdShowFailedEvent(AdShowFailedEventAnalyticsInfo adAnalyticsInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.NATIVE_TYPE, adAnalyticsInfo.AdTag},
                { VAC.GAME_COUNT, adAnalyticsInfo.GameCount },
                { VAC.AD_DURATION, adAnalyticsInfo.AdDuration },
                { VAC.AD_COUNT, adAnalyticsInfo.AdCount },
                { VAC.NETWORK_NAME, adAnalyticsInfo.AdNetworkName },
                { VAC.AD_LOADING_TIME, adAnalyticsInfo.AdLoadingTime }
            };
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, adAnalyticsInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.CREATIVE, adAnalyticsInfo.Creative);
            data.AddIfNotNull(VAC.ERROR_MESSAGE, adAnalyticsInfo.ErrorMessage);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_MESSAGE, adAnalyticsInfo.AdNetworkErrorString);
            data.AddIfNotNull(VAC.ERROR_CODE, adAnalyticsInfo.ErrorCode);
            data.AddIfNotNull(VAC.AD_NETWORK_ERROR_CODE, adAnalyticsInfo.AdNetworkErrorCode);
            data.AddIfNotNull(VAC.IMPRESSION_ID, adAnalyticsInfo.ImpressionId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.native_failed, data);
        }
        
#endregion

#region CrossPromo
        
        private static void OnCrossPromoInitEvent(CrossPromoInitInfo parameters)
        {
            var data = new Dictionary<string, object> {
                { VAC.HTTP_RESPONSE, parameters.HttpResponse },
                { VAC.DOWNLOAD_TIME, parameters.DownloadTime },
                { VAC.GAMES_PROMOTED, parameters.GamesPromoted },
                { VAC.HAS_TIMEOUT, parameters.HasTimeout },
            };

            var contextField = new Dictionary<string, object> {
                { VAC.FORMAT, VAC.SQUARE_FORMAT }
            };

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.cp_response_status, data, null, null, contextField);
        }

        private static void OnCrossPromoShownEvent(CrossPromoAnalyticsInfo crossPromoInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.BUNDLE_ID, crossPromoInfo.GameBundleId },
                { VAC.FILE_PATH, crossPromoInfo.AssetPath }
            };
            data.AddIfNotNull(VAC.IMPRESSION_ID, crossPromoInfo.ImpressionId);

            var contextField = new Dictionary<string, object> {
                { VAC.FORMAT, VAC.SQUARE_FORMAT },
                { VAC.AD_COUNT, crossPromoInfo.AdCount },
                { VAC.GAME_COUNT, crossPromoInfo.GameCount },
                { VAC.CAMPAIGN_ID, crossPromoInfo.CampaignId }
            };

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.cp_impression, data, null, null, contextField);
        }

        private static void OnCrossPromoClickEvent(CrossPromoAnalyticsInfo crossPromoInfo)
        {
            var data = new Dictionary<string, object> {
                { VAC.BUNDLE_ID, crossPromoInfo.GameBundleId },
                { VAC.FILE_PATH, crossPromoInfo.AssetPath },
            };
            data.AddIfNotNull(VAC.IMPRESSION_ID, crossPromoInfo.ImpressionId);

            var contextField = new Dictionary<string, object> {
                { VAC.FORMAT, VAC.SQUARE_FORMAT },
                { VAC.AD_COUNT, crossPromoInfo.AdCount },
                { VAC.GAME_COUNT, crossPromoInfo.GameCount },
                { VAC.CAMPAIGN_ID, crossPromoInfo.CampaignId }
            };

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.cp_click, data, null, null, contextField);
        }

#endregion
        
        private static void OnImpressionTrackedEvent(ImpressionAnalyticsInfo impressionInfo)
        {
            var data = new Dictionary<string, object>();
            Dictionary<string, object> contextVariables = null;
            
            data.AddIfNotNull(VAC.NETWORK_NAME, impressionInfo.AdNetworkName);
            data.AddIfNotNull(VAC.WATERFALL_TEST_NAME, impressionInfo.WaterfallTestName);
            data.AddIfNotNull(VAC.PUBLISHER_REVENUE, impressionInfo.Revenue);
            data.AddIfNotNull(VAC.ID, impressionInfo.Id);
            data.AddIfNotNull(VAC.CURRENCY, impressionInfo.Currency);
            data.AddIfNotNull(VAC.NETWORK_PLACEMENT_ID, impressionInfo.NetworkPlacementId);
            data.AddIfNotNull(VAC.PRECISION, impressionInfo.Precision);
            data.AddIfNotNull(VAC.ADUNIT_ID, impressionInfo.AdUnitId);
            data.AddIfNotNull(VAC.APP_VERSION, impressionInfo.AppVersion);
            data.AddIfNotNull(VAC.COUNTRY, impressionInfo.Country);
            data.AddIfNotNull(VAC.AD_COUNT, impressionInfo.AdCount);
            data.AddIfNotNull(VAC.AD_LOADING_TIME, impressionInfo.AdLoadingTime);
            data.AddIfNotNull(VAC.CREATIVE, impressionInfo.Creative);
            data.AddIfNotNull(VAC.IMPRESSION_ID, impressionInfo.ImpressionId);
            switch (impressionInfo.AdUnitFormat) {
                case ImpressionAdUnitFormat.RewardedVideo:
                    data[VAC.ADUNIT_FORMAT] = VAC.REWARDED_VIDEO;
                    data.AddIfNotNull(VAC.REWARDED_TYPE, impressionInfo.AdTag);
                    break;
                case ImpressionAdUnitFormat.Interstitial:
                    data[VAC.ADUNIT_FORMAT] = VAC.FULLSCREEN;
                    data.AddIfNotNull(VAC.INTERSTITIAL_TYPE, impressionInfo.AdTag);
                    data.AddIfNotNull(VAC.FS_INSTEAD_RV, impressionInfo.IsFsShownInsteadOfRv);
                    data.AddIfNotNull(VAC.FS_INSTEAD_RV_REASON, impressionInfo.ReasonFsShownInsteadOfRv);
                    break;
                case ImpressionAdUnitFormat.Banner:
                    data[VAC.ADUNIT_FORMAT] = VAC.BANNER;
                    break;
                case ImpressionAdUnitFormat.Mrec:
                    data.AddIfNotNull(VAC.MREC_TYPE, impressionInfo.AdTag);
                    data[VAC.ADUNIT_FORMAT] = VAC.MREC;
                    break;
                case ImpressionAdUnitFormat.NativeAds:
                    data.AddIfNotNull(VAC.NATIVE_TYPE, impressionInfo.AdTag);
                    data[VAC.ADUNIT_FORMAT] = VAC.NATIVE;
                    break;
                case ImpressionAdUnitFormat.AppOpen:
                    data[VAC.ADUNIT_FORMAT] = VAC.APP_OPEN;
                    data[VAC.APP_OPEN_TYPE] = AppOpen.AdType.soft_launch.ToString();
                    break;
            }

            switch (impressionInfo) {
                case IronSourceImpressionAnalyticsInfo ironSourceImpressionInfo:
                    data.AddIfNotNull(VAC.ADGROUP_ID, ironSourceImpressionInfo.InstanceId);
                    data.AddIfNotNull(VAC.ADGROUP_NAME, ironSourceImpressionInfo.InstanceName);
                    //add extra data
                    contextVariables = new Dictionary<string, object>();
                    contextVariables.AddIfNotNull(VAC.AB, ironSourceImpressionInfo.Ab);
                    contextVariables.AddIfNotNull(VAC.SEGMENT_NAME, ironSourceImpressionInfo.SegmentName);
                    contextVariables.AddIfNotNull(VAC.LIFETIME_REVENUE, ironSourceImpressionInfo.LifetimeRevenue);
                    contextVariables.AddIfNotNull(VAC.CONVERSION_VALUE, ironSourceImpressionInfo.ConversionValue);
                    break;
            }

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ad_revenue, data, contextVariables: contextVariables);
        }

        private static void OnApplicationFirstLaunch()
        {
            var data = new Dictionary<string, object> {
                { VAC.TOTAL_MEMORY_MBYTE, SystemInfo.systemMemorySize }
            };
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.app_install, data);
        }

        private static void OnApplicationLaunchEvent()
        {
            var context = new Dictionary<string, object> {
                { VAC.UNITY_VERSION, Application.unityVersion.Substring(0, Math.Min(25, Application.unityVersion.Length)) }
            };
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.app_open, (string)null, null, null, context);
        }

        private static void OnGameStarted(GameStartedParameters parameters)
        {
            var data = new Dictionary<string, object> {
                { VAC.LEVEL, parameters.level },
                { VAC.GAME_COUNT, AnalyticsStorageHelper.Instance.GetGameCount() },
                { VAC.HIGHEST_SCORE, AnalyticsStorageHelper.Instance.GetGameHighestScore() },
                { VAC.GAME_ROUND_ID, AnalyticsStorageHelper.Instance.CreateRoundId() },
                { VAC.ORDINAL, parameters.ordinal },
                { VAC.LOOP, parameters.loop },
                { VAC.LEVEL_MOVES, parameters.levelMoves },
                { VAC.ADDITIONAL_MOVES_GRANTED, parameters.additionalMovesGranted },
            };
            data.AddIfNotNull(VAC.PROGRESSION, parameters.progression, "main");
            data.AddIfNotNull(VAC.GAME_MODE, parameters.gameMode);
            data.AddIfNotNull(VAC.LEVEL_DEFINITION_ID, parameters.levelDefinitionID);
            data.AddIfNotNull(VAC.SEED, parameters.seed);
            data.AddIfNotNull(VAC.NUMBER_OF_OBJECTIVES, parameters.numberOfObjectives);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.game_start, data, null, parameters.eventProperties,
                parameters.eventContextProperties);
        }

        private static void OnGameFinished(GameFinishedParameters parameters)
        {
            var data = new Dictionary<string, object> {
                { VAC.LEVEL, parameters.level },
                { VAC.GAME_COUNT, AnalyticsStorageHelper.Instance.GetGameCount() },
                { VAC.GAME_ROUND_ID, AnalyticsStorageHelper.Instance.GetGameRoundId() },
                { VAC.GAME_LENGTH, parameters.gameDuration },
                { VAC.RVS_USED, AnalyticsStorageHelper.Instance.GetRvUsed() },
                { VAC.STATUS, parameters.status },
                { VAC.SCORE, parameters.score },
                { VAC.SOFT_CURRENCY_USED, parameters.softCurrencyUsed },
                { VAC.HARD_CURRENCY_USED, parameters.hardCurrencyUsed },
                { VAC.EGPS_USED, parameters.egpsUsed },
                { VAC.EGPS_RV_USED, parameters.egpsRvUsed },
            };
            data.AddIfNotNull(VAC.GAME_END_REASON, parameters.gameEndReason, "other");
            data.AddIfNotNull(VAC.LEVEL_DEFINITION_ID, parameters.levelDefinitionID);
            data.AddIfNotNull(VAC.NB_STARS, parameters.nbStars);
            data.AddIfNotNull(VAC.MOVES_USED, parameters.movesUsed);
            data.AddIfNotNull(VAC.MOVES_LEFT, parameters.movesLeft);
            data.AddIfNotNull(VAC.OBJECTIVES_LEFT, parameters.objectivesLeft);
            data.AddIfNotNull(VAC.LEVEL_MOVES, parameters.levelMoves);
            data.AddIfNotNull(VAC.ADDITIONAL_MOVES_GRANTED, parameters.additionalMovesGranted);
            data.AddIfNotNull(VAC.EGP_MOVES, parameters.egpMoves);
            data.AddIfNotNull(VAC.NB_IN_GAME_BOOSTERS_USED, parameters.nbInGameBoostersUsed);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.game_finish, data, null, parameters.eventProperties,
                parameters.eventContextProperties);
        }

        private static void OnTrackItemTransactionEvent(ItemTransactionParameters parameters)
        {
            float totalPrice = parameters.unitCost * parameters.nbUnits;

            var data = new Dictionary<string, object> {
                { VAC.ITEM, parameters.item.itemName.FormatKeyName() },
                { VAC.ITEM_TYPE, parameters.item.itemType.ToString() },
                { VAC.TRANSACTION_TYPE, parameters.transactionType.ToString().ToLowerInvariant() },
                { VAC.UNIT_COST, parameters.unitCost },
                { VAC.NB_UNITS, parameters.nbUnits },
                { VAC.TOTAL_PRICE, totalPrice },
                { VAC.BALANCE, parameters.balance },
            };
            data.AddIfNotNull(VAC.CURRENCY_USED, parameters.currencyUsed);
            data.AddIfNotNull(VAC.PLACEMENT, parameters.placement);
            data.AddIfNotNull(VAC.LEVEL, parameters.level);
            data.AddIfNotNull(VAC.IAP_LOCAL_CURRENCY, parameters.iapLocalCurrency);
            data.AddIfNotNull(VAC.SUB_PLACEMENT, parameters.subPlacement);
            data.AddIfNotNull(VAC.PLACEMENT_ID, parameters.placementId);
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.item_transaction, data, null, null,
                parameters.eventContextProperties);
        }

        private static void TrackCustomEvent(string eventName,
                                             Dictionary<string, object> customVariables,
                                             string eventType,
                                             List<VoodooSauce.AnalyticsProvider> analyticsProviders,
                                             Dictionary<string, object> contextVariables = null)
        {
            if (analyticsProviders.Contains(VoodooSauce.AnalyticsProvider.VoodooAnalytics)) {
                VoodooAnalyticsWrapper.TrackCustomEvent(eventName, customVariables, eventType, contextVariables);
            }
        }

        private static void TrackVoodooTuneInitEvent(VoodooTuneInitAnalyticsInfo info)
        {
            var data = new Dictionary<string, object> {
                { VAC.HTTP_RESPONSE, $"{info.HttpResponseCode}" },
                { VAC.DOWNLOAD_TIME, info.DurationInMilliseconds },
                { VAC.HAS_CACHE, info.HasCache },
                { VAC.HAS_TIMEOUT, info.HasTimeout }
            };
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.rs_config_status, data);
        }

        private static void TrackVoodooTuneAbTestAssignment(VoodooTuneAbTestAnalyticsInfo info)
        {
            var data = new Dictionary<string, object> {
                { VAC.AB_TEST_UUID, info.AbTestUuid },
                { VAC.COHORT_UUID, info.AbTestCohortUuid },
                { VAC.VERSION_UUID, info.AbTestVersionUuid }
            };
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ab_test_assignment, data);
        }

        private static void TrackVoodooTuneAbTestExit(VoodooTuneAbTestAnalyticsInfo info)
        {
            var data = new Dictionary<string, object> {
                { VAC.AB_TEST_UUID, info.AbTestUuid },
                { VAC.COHORT_UUID, info.AbTestCohortUuid },
                { VAC.VERSION_UUID, info.AbTestVersionUuid }
            };
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.ab_test_exit, data);
        } 

        private static void TrackPerformanceMetrics(PerformanceMetricsAnalyticsInfo performanceMetrics)
        {
            var data = new Dictionary<string, object> {
                { VAC.BATTERY_LEVEL, performanceMetrics.GetBatteryLevelAsString() },
                { VAC.VOLUME, performanceMetrics.Volume },
                { VAC.MIN + VAC.SEPARATOR_SYMBOL + VAC.FPS, (int)performanceMetrics.Fps.Min },
                { VAC.MAX + VAC.SEPARATOR_SYMBOL + VAC.FPS, (int)performanceMetrics.Fps.Max },
                { VAC.AVERAGE + VAC.SEPARATOR_SYMBOL + VAC.FPS, (int)performanceMetrics.Fps.Average }, {
                    VAC.MIN + VAC.SEPARATOR_SYMBOL + VAC.MEMORY_USAGE,
                    ConvertUtils.ByteToMegaByte(performanceMetrics.MemoryUsage.Min)
                }, {
                    VAC.MAX + VAC.SEPARATOR_SYMBOL + VAC.MEMORY_USAGE,
                    ConvertUtils.ByteToMegaByte(performanceMetrics.MemoryUsage.Max)
                }, {
                    VAC.AVERAGE + VAC.SEPARATOR_SYMBOL + VAC.MEMORY_USAGE,
                    ConvertUtils.ByteToMegaByte(performanceMetrics.MemoryUsage.Average)
                }, {
                    VAC.AVERAGE + VAC.SEPARATOR_SYMBOL + VAC.MEMORY_USAGE_PERCENTAGE,
                    performanceMetrics.AverageMemoryUsagePercentage
                },
                { VAC.BAD_FRAMES, performanceMetrics.BadFrames },
                { VAC.TERRIBLE_FRAMES, performanceMetrics.TerribleFrames }
            };

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.performance_metrics, data);
        }

        private static void TrackVoodooFunnel(string funnelName, string stepName, int stepPosition)
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.funnel, new Dictionary<string, object> {
                { VAC.FUNNEL_NAME, funnelName },
                { VAC.STEP_NAME, stepName },
                { VAC.STEP_POSITION, stepPosition }
            });
        }
        
#region IAP

        private static void OnPurchaseStarted(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseEventInternal(VanEventName.iap_started, payload);
        }

        private static void OnPurchaseProcessing(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseEventInternal(VanEventName.iap_processing, payload);
        }

        private static void OnPurchaseFailed(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseEventInternal(VanEventName.iap_failed, payload);
        }

        private static void OnPurchaseValidated(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseEventInternal(VanEventName.iap_validated, payload);
        }

        private static void OnPurchaseRewarded(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseEventInternal(VanEventName.iap_rewarded, payload);
        }

        private static void OnPurchaseServerError(VoodooIAPAnalyticsInfo payload, IAPServerError serverError)
        {
            var data = new Dictionary<string, object> {
                { VAC.ERROR_CODE, serverError.ToString() }
            };

            OnPurchaseEventInternal(VanEventName.iap_server_error, payload, data);
        }

        private static void OnTrackPurchaseEvent(VoodooIAPAnalyticsInfo payload)
        {
            OnPurchaseEventInternal(VanEventName.iap, payload);
        }

        private static void OnPurchaseEventInternal(VanEventName eventName, VoodooIAPAnalyticsInfo payload, Dictionary<string,object> baseData = null)
        {
            var data = baseData ?? new Dictionary<string, object>();
            
            data.Add(VAC.PURCHASE_TRANSACTION_UUID, payload.id);
            data.Add(VAC.PRODUCT_ID, payload.productId);
            
            if (payload.price > 0)
            {
                data.Add(VAC.PRICE, payload.price);
            }
            
            data.AddIfNotNull(VAC.CURRENCY, payload.currency);
            data.AddIfNotNull(VAC.PRODUCT_NAME, payload.productName);
            data.AddIfNotNull(VAC.PURCHASE_TYPE, payload.purchaseType.ToString());
            data.AddIfNotNull(VAC.PURCHASE_TRANSACTION_ID, payload.purchaseTransactionId);
            data.AddIfNotNull(VAC.FAILURE_REASON, payload.failureReason);

            if (payload.isPurchaseValidated != null)
            {
                data.Add(VAC.PURCHASE_VALIDATED, payload.isPurchaseValidated.Value);
            }
            
            if (payload.purchaseRestored != null)
            {
                string purchaseRestoreName = eventName == VanEventName.iap ? VAC.PURCHASE_RESTORED : VAC.IS_PURCHASE_RESTORATION;
                data.Add(purchaseRestoreName, payload.purchaseRestored.Value);
            }

            var customVariables = new Dictionary<string, object> {
                { VAC.ENVIRONMENT, payload.environment }
            };

            if (eventName == VanEventName.iap)
            {
                data.AddIfNotNull(VAC.ENVIRONMENT, payload.environment);
                customVariables.Add(VAC.SOURCE, "in_game");
            }
            
            VoodooAnalyticsWrapper.TrackEvent(eventName, data, null, customVariables);
        }

#endregion

        private static void OnAttributionChange(AttributionAnalyticsInfo attributionInfo)
        {
            var data = new Dictionary<string, object>();
            data.AddIfNotNull(VAC.NETWORK, attributionInfo.Network);
            data.AddIfNotNull(VAC.ADGROUP, attributionInfo.AdGroup);
            data.AddIfNotNull(VAC.CAMPAIGN, attributionInfo.Campaign);
            data.AddIfNotNull(VAC.CREATIVE, attributionInfo.Creative);
            data.AddIfNotNull(VAC.CLICK_LABEL, attributionInfo.ClickLabel);
            data.AddIfNotNull(VAC.TRACKER_NAME, attributionInfo.TrackerName);
            data.AddIfNotNull(VAC.TRACKER_TOKEN, attributionInfo.TrackerToken);
            data.AddIfNotNull(VAC.COST_TYPE, attributionInfo.CostType);
            data.AddIfNotNull(VAC.COST_CURRENCY, attributionInfo.CostCurrency);
            data.AddIfNotNull(VAC.COST_AMOUNT, attributionInfo.CostAmount);

            if (data.Count > 0) VoodooAnalyticsWrapper.TrackEvent(VanEventName.attribution_changed, data);
        }

#region Backup Ads

        private static void OnBackupAdsClickEvent(BackupAdsAnalyticsInfo info)
        {
            var data = new Dictionary<string, object> {
                { VAC.BUNDLE_ID, info.bundleId },
                { VAC.FILE_PATH, info.filePath }
            };

            var contextField = new Dictionary<string, object> {
                { VAC.FORMAT, info.format },
                { VAC.AD_COUNT, info.adCount },
                { VAC.GAME_COUNT, info.gameCount },
                { VAC.CAMPAIGN_ID, info.mercuryCampaignId }
            };

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.cp_click, data, null, null, contextField);
        }

        private static void OnBackupAdsInitEvent(CrossPromoInitInfo info)
        {
            var data = new Dictionary<string, object> {
                { VAC.DOWNLOAD_TIME, info.DownloadTime },
                { VAC.HAS_TIMEOUT, info.HasTimeout },
                { VAC.GAMES_PROMOTED, info.GamesPromoted },
                { VAC.HTTP_RESPONSE, info.HttpResponse }
            };

            var contextField = new Dictionary<string, object> {
                { VAC.FORMAT, VAC.BACKUP_FORMAT }
            };

            if (!string.IsNullOrEmpty(info.StrategyId)) {
                VoodooAnalyticsWrapper.AddGlobalContextParameter(VAC.CP_BACKUP_STRATEGY_ID, info.StrategyId, true);
            }

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.cp_response_status, data, null, null, contextField);
        }

        private static void OnBackupAdsShown(BackupAdsAnalyticsInfo info)
        {
            var data = new Dictionary<string, object> {
                { VAC.BUNDLE_ID, info.bundleId },
                { VAC.FILE_PATH, info.filePath }
            };

            var contextField = new Dictionary<string, object> {
                { VAC.FORMAT, info.format },
                { VAC.AD_COUNT, info.adCount },
                { VAC.GAME_COUNT, info.gameCount },
                { VAC.CAMPAIGN_ID, info.mercuryCampaignId }
            };

            VoodooAnalyticsWrapper.TrackEvent(VanEventName.cp_impression, data, null, null, contextField);
        }

#endregion

#region Loading Time Events

        private static void OnGameInteractable(LoadingTimeAnalyticsInfo loadingTimeInfo)
        {
            TrackLoadingTimeEvent(VAC.LOADING_STEP_GLOBAL, loadingTimeInfo);
        }

        private static void OnVoodooSauceSDKInitialized(LoadingTimeAnalyticsInfo loadingTimeInfo)
        {
            TrackLoadingTimeEvent(VAC.LOADING_STEP_VS_INIT, loadingTimeInfo);
        }

        private static void OnUnityEngineStarted(LoadingTimeAnalyticsInfo loadingTimeInfo)
        {
            TrackLoadingTimeEvent(VAC.LOADING_STEP_UNITY_START, loadingTimeInfo);
        }

        private static void TrackLoadingTimeEvent(string stepName, LoadingTimeAnalyticsInfo loadingTimeInfo)
        {
            VoodooAnalyticsWrapper.TrackEvent(VanEventName.loading_time, new Dictionary<string, object> {
                { VAC.LOADING_STEP, stepName },
                { VAC.STEP_DURATION, loadingTimeInfo.duration },
                { VAC.STEP_START, loadingTimeInfo.start },
                { VAC.STEP_END, loadingTimeInfo.end },
                { VAC.SESSION_ID, loadingTimeInfo.sessionId },
            });
        }

#endregion
    }
}