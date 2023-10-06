using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.Firebase;
using Voodoo.Sauce.Privacy;

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.Analytics
{
    //Firebase have an issue with accessing any static variable that are located in FirebaseAnalytics class
    //during the initialization, it will throw an exception and cause the script to stop running.
    //So we need to ensure that there are no call to any FirebaseAnalytics 's static variable or constant 
    //before the CheckDependencies are finished.
    //If you need to use firebase static constant, please add it in FirebaseAnalyticsConstant.
    //And add the switch case and map back to firebase constant in FirebaseAnalyticsEvent.GetFirebaseKey
    [Preserve]
    internal class FirebaseAnalyticsProvider : BaseAnalyticsProviderWithLogger
    {
        private readonly bool _useFirebaseAnalytics;
        private static string _mediation;
        internal override VoodooSauce.AnalyticsProvider GetProviderEnum() => VoodooSauce.AnalyticsProvider.FirebaseAnalytics;

        // Needed for the VoodooGDPRAnalytics class. Do not call it directly.
        public FirebaseAnalyticsProvider() { }

        internal FirebaseAnalyticsProvider(bool useFirebaseAnalytics, string mediation)
        {
            _mediation = mediation;
            _useFirebaseAnalytics = useFirebaseAnalytics;
            
            if (PlatformUtils.UNITY_EDITOR) _useFirebaseAnalytics = false;
            if (!_useFirebaseAnalytics) return;
            RegisterEvents();
        }

        public override void Instantiate(string mediation)
        {
            _mediation = mediation;
        }

        public override void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild)
        {
            if (!_useFirebaseAnalytics || !consent.ExplicitConsentGivenForAnalytics) return;
            
            FirebaseAnalyticsWrapper.SubscribeToFirebaseInitialization(consent, success => {
                if (success) {
                    IsInitialized = true;
                } else {
                    UnregisterEvents();
                }
            });
            FirebaseInitializer.Start();
        }

        private static void RegisterEvents()
        {
            AnalyticsManager.OnGameStartedEvent += OnGameStarted;
            AnalyticsManager.OnGameFinishedEvent += OnGameFinished;
            AnalyticsManager.OnTrackCustomEvent += TrackCustomEvent;
            AnalyticsManager.OnPurchaseValidated += TrackPurchase;
            AnalyticsManager.OnTrackPurchaseEvent += TrackPurchase;
            AnalyticsManager.OnShowInterstitialEvent += OnFsShown;
            AnalyticsManager.OnShowRewardedVideoEvent += OnRvShown;
            AnalyticsManager.OnBannerClickedEvent += OnBannerClicked;
            AnalyticsManager.OnInterstitialClickedEvent += OnFsClicked;
            AnalyticsManager.OnRewardedVideoClickedEvent += OnRvClicked;
            AnalyticsManager.OnImpressionTrackedEvent += OnImpressionTrackedEvent;
        }

        private static void UnregisterEvents()
        {
            AnalyticsManager.OnGameStartedEvent -= OnGameStarted;
            AnalyticsManager.OnGameFinishedEvent -= OnGameFinished;
            AnalyticsManager.OnTrackCustomEvent -= TrackCustomEvent;
            AnalyticsManager.OnPurchaseValidated -= TrackPurchase;
            AnalyticsManager.OnTrackPurchaseEvent -= TrackPurchase;
            AnalyticsManager.OnShowInterstitialEvent -= OnFsShown;
            AnalyticsManager.OnShowRewardedVideoEvent -= OnRvShown;
            AnalyticsManager.OnBannerClickedEvent -= OnBannerClicked;
            AnalyticsManager.OnInterstitialClickedEvent -= OnFsClicked;
            AnalyticsManager.OnRewardedVideoClickedEvent -= OnRvClicked;
            AnalyticsManager.OnImpressionTrackedEvent -= OnImpressionTrackedEvent;
        }

        private static void OnGameStarted(GameStartedParameters parameters)
        {
            Dictionary<string, object> newProperties =
                parameters.eventProperties != null ? new Dictionary<string, object>(parameters.eventProperties) : new Dictionary<string, object>();

            newProperties[FirebaseAnalyticsConstants.PARAMETER_LEVEL] = parameters.level;
            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.EVENT_LEVEL_START, newProperties);
        }

        private static void OnGameFinished(GameFinishedParameters parameters)
        {
            Dictionary<string, object> newProperties =
                parameters.eventProperties != null ? new Dictionary<string, object>(parameters.eventProperties) : new Dictionary<string, object>();

            newProperties[FirebaseAnalyticsConstants.PARAMETER_LEVEL] = parameters.level;
            newProperties[FirebaseAnalyticsConstants.PARAMETER_SCORE] = parameters.score;
            if (parameters.status) {
                FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.EVENT_LEVEL_UP, newProperties);
            }

            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.EVENT_LEVEL_END, newProperties);

            int gameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            if (Array.IndexOf(FirebaseAnalyticsConstants.GameCountsToTrack, gameCount) > -1) {
                newProperties[FirebaseAnalyticsConstants.PARAMETER_VALUE] = gameCount;
                FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.GAMES_PLAYED_EVENT_NAME, newProperties);
            }
        }

        private static void TrackCustomEvent(string eventName,
                                             Dictionary<string, object> eventProperties,
                                             string type,
                                             List<VoodooSauce.AnalyticsProvider> analyticsProviders,
                                             Dictionary<string, object> contextVariables = null)
        {
            if (analyticsProviders.Contains(VoodooSauce.AnalyticsProvider.FirebaseAnalytics)) {
                FirebaseAnalyticsWrapper.TrackEvent(eventName, eventProperties);
            }
        }

        private static void TrackPurchase(VoodooIAPAnalyticsInfo purchaseInfo)
        {
            FirebaseAnalyticsWrapper.TrackPurchase(purchaseInfo);
        }

        private static void OnFsShown(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var properties = new Dictionary<string, object> {
                {FirebaseAnalyticsConstants.PARAMETER_VALUE, adAnalyticsInfo.AdCount}
            };
            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.FS_SHOWN_EVENT_NAME, properties);
        }

        private static void OnRvShown(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var properties = new Dictionary<string, object> {
                {FirebaseAnalyticsConstants.PARAMETER_VALUE, adAnalyticsInfo.AdCount}
            };
            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.RV_SHOWN_EVENT_NAME, properties);
        }

        private static void OnBannerClicked(AdEventAnalyticsInfo adAnalyticsInfo)
        {
            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.BANNER_CLICKED_EVENT_NAME);
        }

        private static void OnFsClicked(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.FS_CLICKED_EVENT_NAME);
        }

        private static void OnRvClicked(AdClickEventAnalyticsInfo adAnalyticsInfo)
        {
            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.RV_CLICKED_EVENT_NAME);
        }

        private static void OnImpressionTrackedEvent(ImpressionAnalyticsInfo impressionInfo)
        {
            var properties = new Dictionary<string, object>();
            properties.AddIfNotNull(FirebaseAnalyticsConstants.ILRD_AD_PLATFORM, _mediation);
            properties.AddIfNotNull(FirebaseAnalyticsConstants.ILRD_AD_SOURCE, impressionInfo.AdNetworkName);
            properties.AddIfNotNull(FirebaseAnalyticsConstants.PARAMETER_VALUE, impressionInfo.Revenue);
            properties.AddIfNotNull(FirebaseAnalyticsConstants.PARAMETER_CURRENCY, impressionInfo.Currency);
            properties.AddIfNotNull(FirebaseAnalyticsConstants.ILRD_PRECISION, impressionInfo.Precision);
            properties.AddIfNotNull(FirebaseAnalyticsConstants.ILRD_AD_UNIT_ID, impressionInfo.AdUnitId);
            properties.AddIfNotNull(FirebaseAnalyticsConstants.ILRD_APP_VERSION, impressionInfo.AppVersion);
            properties.AddIfNotNull(FirebaseAnalyticsConstants.ILRD_COUNTRY, impressionInfo.Country);
            
            switch (impressionInfo.AdUnitFormat) {
                case ImpressionAdUnitFormat.RewardedVideo:
                    properties[FirebaseAnalyticsConstants.ILRD_AD_FORMAT] = FirebaseAnalyticsConstants.REWARDED_VIDEO;
                    break;
                case ImpressionAdUnitFormat.Interstitial:
                    properties[FirebaseAnalyticsConstants.ILRD_AD_FORMAT] = FirebaseAnalyticsConstants.FULLSCREEN;
                    break;
                case ImpressionAdUnitFormat.Banner:
                    properties[FirebaseAnalyticsConstants.ILRD_AD_FORMAT] = FirebaseAnalyticsConstants.BANNER;
                    break;
            }

            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.ILRD_EVENT_NAME, properties);

            properties.AddIfNotNull(FirebaseAnalyticsConstants.ILRD_AD_UNIT_NAME, impressionInfo.AdUnitId);
            FirebaseAnalyticsWrapper.TrackEvent(FirebaseAnalyticsConstants.ILRD_EVENT_NAME_V2, properties);
        }
    }
}