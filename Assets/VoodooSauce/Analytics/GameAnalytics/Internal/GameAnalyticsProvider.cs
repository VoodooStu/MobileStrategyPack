using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    internal class GameAnalyticsProvider :  BaseAnalyticsProviderWithLogger
    {
        internal override VoodooSauce.AnalyticsProvider GetProviderEnum() => VoodooSauce.AnalyticsProvider.GameAnalytics;
        
        // Needed for the VoodooGDPRAnalytics class. Do not call it directly.
        public GameAnalyticsProvider()
        {
        }

        internal GameAnalyticsProvider(bool initEvents)
        {
            if (initEvents) {
                RegisterEvents();
            }
        }

        public override void Instantiate(string mediation) { }
        
        public override void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild)
        {
            if (!GameAnalyticsWrapper.Initialize(consent)) {
                UnregisterEvents();
            } else {
                IsInitialized = true;
            }
        }

        private static void RegisterEvents()
        {
            AnalyticsManager.OnGameStartedEvent += OnGameStarted;
            AnalyticsManager.OnGameFinishedEvent += OnGameFinished;
            AnalyticsManager.OnTrackCustomEvent += TrackCustomEvent;
            AnalyticsManager.OnPurchaseValidated += TrackPurchase;
            AnalyticsManager.OnTrackPurchaseEvent += TrackPurchase;
        }

        private static void UnregisterEvents()
        {
            AnalyticsManager.OnGameStartedEvent -= OnGameStarted;
            AnalyticsManager.OnGameFinishedEvent -= OnGameFinished;
            AnalyticsManager.OnTrackCustomEvent -= TrackCustomEvent;
            AnalyticsManager.OnPurchaseValidated -= TrackPurchase;
            AnalyticsManager.OnTrackPurchaseEvent -= TrackPurchase;
        }

        private static void OnGameStarted(GameStartedParameters parameters)
        { 
            GameAnalyticsWrapper.TrackProgressEvent(
                GAProgressionStatus.Start,
                parameters.level,
                null,
                MergeContexts(parameters.eventContextProperties, parameters.eventProperties));
        }
        
        private static void TrackPurchase(VoodooIAPAnalyticsInfo purchaseInfo)
        {
            GameAnalyticsWrapper.TrackPurchase(purchaseInfo);
        }

        private static void OnGameFinished(GameFinishedParameters parameters)
        {
            GameAnalyticsWrapper.TrackProgressEvent(
                parameters.status ? GAProgressionStatus.Complete : GAProgressionStatus.Fail,
                parameters.level,
                (int)parameters.score,
                MergeContexts(parameters.eventContextProperties, parameters.eventProperties));
        }

        private static void TrackCustomEvent(string eventName,
                                             Dictionary<string, object> eventProperties,
                                             string type,
                                             List<VoodooSauce.AnalyticsProvider> analyticsProviders,
                                             Dictionary<string, object> contextVariables = null)
        {
            if (analyticsProviders.Contains(VoodooSauce.AnalyticsProvider.GameAnalytics)) {
                GameAnalyticsWrapper.TrackDesignEvent(eventName, MergeContexts(contextVariables, eventProperties));
            }
        }

        private static Dictionary<string, object> MergeContexts(Dictionary<string, object> context,
                                                                Dictionary<string, object> legacyContext)
        {
            if (legacyContext == null) {
                return context;
            }
            
            if (context == null) {
                return legacyContext;
            }

            var newContext = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> pair in legacyContext) {
                newContext[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<string, object> pair in context) {
                newContext[pair.Key] = pair.Value;
            }

            return newContext;
        }
    }
}