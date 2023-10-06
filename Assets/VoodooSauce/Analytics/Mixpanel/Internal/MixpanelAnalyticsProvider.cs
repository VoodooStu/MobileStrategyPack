using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    internal class MixpanelAnalyticsProvider : BaseAnalyticsProviderWithLogger
    {
        private readonly MixpanelParameters _parameters;
        internal override VoodooSauce.AnalyticsProvider GetProviderEnum() => VoodooSauce.AnalyticsProvider.Mixpanel;

        internal override string GetUninitializedErrorMessage()
        {
            if (_parameters.UseVoodooTune) {
                return "Mixpanel is turned off since VoodooTune is used";
            }

            if (!_parameters.UseMixpanel) {
                return "Mixpanel is deactivated via VoodooKitchen";
            }

            if (string.IsNullOrEmpty(_parameters.MixpanelProdToken)) {
                return "Mixpanel is turned off since No tokens for Mixpanel has been set";
            }

            if (!AbTestManager.PlayerIsInACohort()) {
                return "Mixpanel is turned off since User is not in cohort";
            }

            return base.GetUninitializedErrorMessage();
        }

        // Needed for the VoodooGDPRAnalytics class. Do not call it directly.
        public MixpanelAnalyticsProvider() { }

        internal MixpanelAnalyticsProvider(MixpanelParameters parameters)
        {
            _parameters = parameters;
            if (!_parameters.IsMixpanelRemotelyActivated()) return;
            RegisterEvents();
        }

        public override void Instantiate(string mediation) { }

        public override void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild)
        {
            if (!_parameters.IsMixpanelRemotelyActivated()) {
                MixpanelWrapper.Disable();
                return;
            }

            if (!AbTestManager.PlayerIsInACohort()) {
                UnregisterEvents();
                MixpanelWrapper.Disable();
                return;
            }

            MixpanelWrapper.Initialize(_parameters.MixpanelProdToken, consent);
            IsInitialized = true;
        }

        private void RegisterEvents()
        {
            AnalyticsManager.OnGameStartedEvent += OnGameStarted;
            AnalyticsManager.OnGameFinishedEvent += OnGameFinished;
            AnalyticsManager.OnTrackCustomEvent += TrackCustomEvent;
            AnalyticsManager.OnApplicationFirstLaunchEvent += OnApplicationFirstLaunch;
            AnalyticsManager.OnApplicationLaunchEvent += OnApplicationLaunch;
            AnalyticsManager.OnCrossPromoShownEvent += OnCrossPromoShown;
            AnalyticsManager.OnCrossPromoClickEvent += OnCrossPromoClick;
            AnalyticsManager.OnCrossPromoErrorEvent += OnCrossPromoError;
            AnalyticsManager.OnShowInterstitialEvent += OnFsShown;
            AnalyticsManager.OnShowRewardedVideoEvent += OnRvShown;
        }

        private static void UnregisterEvents()
        {
            AnalyticsManager.OnGameStartedEvent -= OnGameStarted;
            AnalyticsManager.OnGameFinishedEvent -= OnGameFinished;
            AnalyticsManager.OnTrackCustomEvent -= TrackCustomEvent;
            AnalyticsManager.OnApplicationFirstLaunchEvent -= OnApplicationFirstLaunch;
            AnalyticsManager.OnApplicationLaunchEvent -= OnApplicationLaunch;
            AnalyticsManager.OnCrossPromoShownEvent -= OnCrossPromoShown;
            AnalyticsManager.OnCrossPromoClickEvent -= OnCrossPromoClick;
            AnalyticsManager.OnCrossPromoErrorEvent -= OnCrossPromoError;
            AnalyticsManager.OnShowInterstitialEvent -= OnFsShown;
            AnalyticsManager.OnShowRewardedVideoEvent -= OnRvShown;
        }

        private static void OnGameStarted(GameStartedParameters gameStartedParameters)
        {
            MixpanelWrapper.TrackTimedEvent(MixpanelConstants.GamePlayedEventName);
        }

        private static void OnGameFinished(GameFinishedParameters parameters)
        {
            Dictionary<string, object> newProperties =
                parameters.eventProperties != null ? new Dictionary<string, object>(parameters.eventProperties) : new Dictionary<string, object>();

            newProperties[MixpanelConstants.WinPropertyName] = parameters.status;
            newProperties[MixpanelConstants.ScorePropertyName] = parameters.score;
            newProperties[MixpanelConstants.GameNumberPropertyName] = AnalyticsStorageHelper.Instance.GetGameCount();
            newProperties[MixpanelConstants.LevelNumberPropertyName] = parameters.level;

            MixpanelWrapper.TrackEvent(MixpanelConstants.GamePlayedEventName, newProperties);
        }

        private static void OnFsShown(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var eventProperties = new Dictionary<string, object> {[MixpanelConstants.AdNumberPropertyName] = adAnalyticsInfo.AdCount};
            if (!string.IsNullOrEmpty(adAnalyticsInfo.AdTag)) eventProperties[MixpanelConstants.TagPropertyName] = adAnalyticsInfo.AdTag;

            MixpanelWrapper.TrackEvent(MixpanelConstants.FSShownEventName, eventProperties);
        }

        private static void OnRvShown(AdShownEventAnalyticsInfo adAnalyticsInfo)
        {
            var eventProperties = new Dictionary<string, object> {[MixpanelConstants.AdNumberPropertyName] = adAnalyticsInfo.AdCount};
            if (!string.IsNullOrEmpty(adAnalyticsInfo.AdTag)) eventProperties[MixpanelConstants.TagPropertyName] = adAnalyticsInfo.AdTag;

            MixpanelWrapper.TrackEvent(MixpanelConstants.RVShownEventName, eventProperties);
        }

        private static void TrackCustomEvent(string eventName,
                                             Dictionary<string, object> eventProperties,
                                             string type,
                                             List<VoodooSauce.AnalyticsProvider> analyticsProviders,
                                             Dictionary<string, object> contextVariables = null)
        {
            if (analyticsProviders.Contains(VoodooSauce.AnalyticsProvider.Mixpanel)) {
                MixpanelWrapper.TrackEvent(eventName, eventProperties);
            }
        }

        private static void OnCrossPromoShown(CrossPromoAnalyticsInfo crossPromoInfo)
        {
            var eventProperties = new Dictionary<string, object> {
                [MixpanelConstants.GameNamePropertyName] = crossPromoInfo.GameName, [MixpanelConstants.AdFormatPropertyName] = crossPromoInfo.Format
            };
            MixpanelWrapper.TrackEvent(MixpanelConstants.CrossPromoShownEventName, eventProperties);
        }

        private static void OnCrossPromoClick(CrossPromoAnalyticsInfo crossPromoInfo)
        {
            var eventProperties = new Dictionary<string, object> {
                [MixpanelConstants.GameNamePropertyName] = crossPromoInfo.GameName, [MixpanelConstants.AdFormatPropertyName] = crossPromoInfo.Format
            };
            MixpanelWrapper.TrackEvent(MixpanelConstants.CrossPromoClickEventName, eventProperties);
        }

        private static void OnCrossPromoError(string errorMessage)
        {
            var eventProperties = new Dictionary<string, object> {[MixpanelConstants.ErrorPropertyName] = errorMessage};
            MixpanelWrapper.TrackEvent(MixpanelConstants.CrossPromoErrorEventName, eventProperties);
        }

        private static void OnApplicationFirstLaunch()
        {
            MixpanelWrapper.TrackEvent(MixpanelConstants.AppFirstLaunchEventName);
        }

        private static void OnApplicationLaunch()
        {
            MixpanelWrapper.TrackEvent(MixpanelConstants.AppLaunchEventName);
        }
    }
}