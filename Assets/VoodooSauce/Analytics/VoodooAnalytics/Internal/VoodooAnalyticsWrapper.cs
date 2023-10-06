using System.Collections.Generic;
using UnityEngine;
using Voodoo.Analytics;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.VoodooTune;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public static class VoodooAnalyticsWrapper
    {
        public static void TrackEvent(VanEventName eventName,
                                      Dictionary<string, object> data,
                                      string eventType = null,
                                      Dictionary<string, object> customVariables = null,
                                      Dictionary<string, object> contextVariables = null)
        {
            new VoodooAnalyticsLoggerEvent(eventName, data, eventType, customVariables, contextVariables).Track();
        }

        public static void TrackCustomEvent(string eventName,
                                            Dictionary<string, object> customVariables,
                                            string eventType = null,
                                            Dictionary<string, object> contextVariables = null)
        {
            new VoodooAnalyticsLoggerEvent(eventName, customVariables, eventType, contextVariables).Track();
        }

        public static void TrackEvent(VanEventName eventName,
                                      string data = null,
                                      string eventType = null,
                                      Dictionary<string, object> customVariables = null,
                                      Dictionary<string, object> contextVariables = null)
        {
            new VoodooAnalyticsLoggerEvent(eventName, data, eventType, customVariables, contextVariables).Track();
        }

        public static void Instantiate(AnalyticsConfig analyticsConfig,
                                       VoodooAnalyticsParameters parameters,
                                       string mediation)
        {
            var sessionParameters = new VoodooAnalyticsEventParameters();

            sessionParameters.SetAppBundleId(Application.identifier);
            sessionParameters.SetAppVersion(Application.version);
            sessionParameters.SetVsVersion(VoodooSauce.Version());
            sessionParameters.SetMediation(mediation ?? VoodooAnalyticsConstants.DEFAULT_MEDIATION);
            if (parameters.UseVoodooTune) {
                sessionParameters.SetSegmentationUuid(VoodooTuneManager.GetSegmentationUuid());
                sessionParameters.SetAbTestUuid(VoodooTuneManager.GetAbTestUuids());
                sessionParameters.SetAbTestCohortUuid(VoodooTuneManager.GetAbTestCohortUuids());
                sessionParameters.SetAbTestVersionUuid(VoodooTuneManager.GetAbTestVersionUuid());
            } else {
                sessionParameters.SetAbTestUuid(parameters.LegacyABTestName);
                sessionParameters.SetAbTestCohortUuid(AbTestManager.GetPlayerCohort());
            }

            string gatewayUrl = EnvironmentSettings.GetAnalyticsURL("https://vs-api.voodoo-{0}.io/push-analytics-v2");
            VoodooAnalyticsManager.SetCustomLogger(new VanCustomLog());
            VoodooAnalyticsManager.Init(analyticsConfig, parameters.ProxyServer, gatewayUrl, sessionParameters,
                AnalyticsSessionManager.Instance().OnNewEvent);
        }

        public static void AddGlobalContextParameter(string key, string value, bool cached)
        {
            VoodooAnalyticsManager.GlobalContext.Add(key, value, cached);
        }
    }
}