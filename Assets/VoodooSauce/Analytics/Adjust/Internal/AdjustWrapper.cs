using System.Collections.Generic;
using com.adjust.sdk;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public static class AdjustWrapper
    {
        private const string TAG = "AdjustWrapper";

        private const string ADJUST_USER_ID_KEY = "vs_adjust_user_id";
        private static readonly AttributionData AttributionData = new AttributionData { Name = "Adjust" }; 

        private static bool _isInitialized;
        private static readonly Queue<AdjustAnalyticsEvent> EventsQueue = new Queue<AdjustAnalyticsEvent>();

        internal static void Instantiate()
        {
            // As the Adjust user id is not used anymore, the stored value is deleted.
            PlayerPrefs.DeleteKey(ADJUST_USER_ID_KEY);
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Adjust started");

        }
        
        public static void Initialize(string token, bool isTestMode, bool isChinaBuild)
        {
            AddAdjustSessionCallbackParameters();
            InitializeAdjust(token, isTestMode, isChinaBuild);
            _isInitialized = true;
            while (EventsQueue.Count > 0) {
                EventsQueue.Dequeue().Track();
            }
        }

        public static void TrackEvent(string eventName, Dictionary<string, object> customVariables = null)
        {
            if (string.IsNullOrEmpty(eventName)) {
                return;
            }

            var trackEvent = new AdjustAnalyticsEvent(eventName, customVariables);
            
            if (!_isInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Adjust NOT initialized queuing event...{eventName}");
                EventsQueue.Enqueue(trackEvent);
            } else {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending event {eventName} to Adjust");
                trackEvent.Track();
            }
        }

        public static void TrackAdRevenue(string eventName, string data)
        {
            if (string.IsNullOrEmpty(eventName)) {
                return;
            }

            if (!_isInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Adjust NOT initialized ignore Ad revenue event...{eventName}");
                return;
            }

            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending AdRevenue {eventName}  to Adjust");
            Adjust.trackAdRevenue(eventName, data);
        }

        public static void TrackAdRevenue(AdjustAdRevenue adRevenue)
        {
            if (adRevenue == null) {
                return;
            }

            if (!_isInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Adjust NOT initialized ignore Ad revenue event...{adRevenue.source}");
                return;
            }

            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending AdRevenue {adRevenue.source}  to Adjust");
            Adjust.trackAdRevenue(adRevenue);
        }
        
        private static void AddAdjustSessionCallbackParameters()
        {
            var parameters = new Dictionary<string, string> {[AdjustConstants.VoodooSauceParameterName] = VoodooSauce.Version()};
            string appSubVersion = AbTestManager.GetPlayerCohort();
            if (appSubVersion != null) parameters[AdjustConstants.AppSubVersionParameterName] = appSubVersion;
            AddAdjustSessionCallbackParameters(parameters);
        }

        private static void AddAdjustSessionCallbackParameters(Dictionary<string, string> parameters)
        {
            if (parameters == null) return;

            foreach (KeyValuePair<string, string> parameter in parameters) {
                Adjust.addSessionCallbackParameter(parameter.Key, parameter.Value);
            }
        }

        private static void InitializeAdjust(string adjustAppToken, bool isTestMode, bool isChinaBuild)
        {
            if(Application.isEditor) return;
            
            var adjustComponent = Object.FindObjectOfType<Adjust>();
            if (adjustComponent == null) {
                var adjustGameObject = new GameObject("Adjust");
                adjustGameObject.SetActive(false);
                adjustComponent = adjustGameObject.AddComponent<Adjust>();
                adjustComponent.startManually = false;
                adjustComponent.eventBuffering = false;
                adjustComponent.sendInBackground = true;
                // Allow tracking special behavior if user redirected from tracking link
                adjustComponent.launchDeferredDeeplink = false;
                adjustComponent.appToken = adjustAppToken;
                adjustComponent.environment = isTestMode ? AdjustEnvironment.Sandbox : AdjustEnvironment.Production;
                adjustComponent.logLevel = VoodooLog.IsDebugLogsEnabled ? AdjustLogLevel.Verbose : AdjustLogLevel.Suppress;
               
                if (isChinaBuild) {
                    VoodooLog.LogDebug(Module.ANALYTICS, TAG, "China app -> AdjustUrlStrategy is left to default");
                } else {
                    VoodooLog.LogDebug(Module.ANALYTICS, TAG, "WorldWide app -> AdjustUrlStrategy is set to India");
                    adjustComponent.urlStrategy = AdjustUrlStrategy.India;
                }
#if UNITY_ANDROID
                adjustComponent.preinstallTracking = true;
#endif
                adjustComponent.AttributionChange = OnAttributionChange;
                adjustComponent.needsCost = true;
                adjustGameObject.SetActive(true); // this will ensure adjComponent.Awake is fired with the new values
            } else {
                // if the object is in the scene we expect someone to do the init already
                // otherwise Adjust SDK callbacks won't work with this version
                adjustComponent.gameObject.name = "Adjust";
            }
        }

        internal static AttributionData GetAttributionData() => AttributionData;
        
        private static void OnAttributionChange(AdjustAttribution adjustAttribution)
        {
            var attributionInfo = new AttributionAnalyticsInfo {
                AdId = adjustAttribution.adid,
                Network = adjustAttribution.network,
                AdGroup = adjustAttribution.adgroup,
                Campaign = adjustAttribution.campaign,
                Creative = adjustAttribution.creative,
                ClickLabel = adjustAttribution.clickLabel,
                TrackerName = adjustAttribution.trackerName,
                TrackerToken = adjustAttribution.trackerToken,
                CostType = adjustAttribution.costType,
                CostAmount = adjustAttribution.costAmount,
                CostCurrency = adjustAttribution.costCurrency
            };
            AnalyticsManager.TrackAttribution(attributionInfo);
        }
    }
}