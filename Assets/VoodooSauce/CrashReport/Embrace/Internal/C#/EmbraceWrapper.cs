using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.IAP;
using Voodoo.Sauce.Internal.VoodooTune;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    public static class EmbraceWrapper
    {
#region Constants

        private const string TAG = "EmbraceWrapper";

        private const int EMBRACE_PERSONA_MAX_LENGTH = 32;
        
#endregion
        
#region Properties

        internal static bool useRemoteConfig;
        private static bool _isEnabled;
        private static bool _isStarted;
        private static readonly EmbraceStoredValues StoredValues = new EmbraceStoredValues();

#endregion

        internal static void AnalyticsConsentEvent(bool consent)
        {
            _isEnabled = consent;
            
            if (!consent) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Non initialized because the user's consent have not been given");
                return;
            }
            
            Start();
            SetUserId(AnalyticsUserIdHelper.GetUserId());
            SetCustomData(CrashReportConstants.SESSION_ID_KEY, AnalyticsSessionManager.Instance().SessionInfo.id);
            SetCustomData(CrashReportConstants.VERSION_KEY, VoodooConfig.Version());

            if (useRemoteConfig) {
                string abTestVersionUuid = VoodooTuneManager.GetAbTestVersionUuid();
                if (!string.IsNullOrEmpty(abTestVersionUuid)) {
                    SetPersona($"{CrashReportConstants.PERSONA_ABTEST_VERSION_UUID_PREFIX}{abTestVersionUuid}");
                }

                string[] segmentations = VoodooTuneManager.GetSegmentationUuidsAsList().ToArray();
                foreach (string segmentation in segmentations) {
                    SetPersona($"{CrashReportConstants.PERSONA_SEGMENT_UUID_PREFIX}{segmentation}");
                }
            }

            string[] abTests = useRemoteConfig ? VoodooTuneManager.GetAbTestUuidsAsList().ToArray() : AbTestManager.GetAbTests();
            if (abTests != null) {
                foreach (string abTest in abTests) {
                    SetPersona($"{CrashReportConstants.PERSONA_ABTEST_UUID_PREFIX}{abTest}");
                }
            }
            
            string[] cohorts = useRemoteConfig ? VoodooTuneManager.GetAbTestCohortUuidsAsList().ToArray() : AbTestManager.GetPlayerCohorts();
            if (cohorts != null) {
                foreach (string cohort in cohorts) {
                    SetPersona($"{CrashReportConstants.PERSONA_ABTEST_COHORT_UUID_PREFIX}{cohort}");
                }
            }
            
            if (VoodooPremium.IsPremium() || VoodooSauceCore.GetInAppPurchase().IsSubscribedProduct()) {
                SetUserAsPayer();
            }

            ApplyStoredValues();
        }

        private static void Start()
        {
            if (!_isEnabled) {
                return;
            }
            
            EmbraceSDKBridge.Start();

            _isStarted = true;
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Started");
        }

        internal static void SetUserId(string userId)
        {
            if (!_isEnabled) {
                return;
            }

            if (!_isStarted) {
                StoredValues.userId = userId;
                return;
            }

            EmbraceSDKBridge.SetUserIdentifier(userId);
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"User ID: {userId}");
        }

        private static void SetUserAsPayer()
        {
            if (!_isEnabled) {
                return;
            }

            if (!_isStarted) {
                StoredValues.isPayer = true;
                return;
            }

            EmbraceSDKBridge.SetUserAsPayer();
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Set user as payer");
        }

        internal static void SetCustomData(string key, string value)
        {
            if (!_isEnabled) {
                return;
            }

            if (!_isStarted) {
                StoredValues.sessionProperties.Add(key, value);
                return;
            }

            EmbraceSDKBridge.AddSessionProperty(key, value, false);
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Custom Data: {key} = {value}");
        }

        internal static void SetPersona(string persona)
        {
            if (!_isEnabled) {
                return;
            }

            if (!_isStarted) {
                StoredValues.personas.Add(persona);
                return;
            }

            string fixedPersona = FormatPersonaString(persona);
            if (string.IsNullOrEmpty(fixedPersona)) {
                return;
            }
            
            EmbraceSDKBridge.SetUserPersona(fixedPersona);
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Persona: {fixedPersona}");
        }

        internal static void Log(string message)
        {
            if (!_isEnabled) {
                return;
            }

            if (!_isStarted) {
                StoredValues.logs.Add(message);
                return;
            }

            EmbraceSDKBridge.LogBreadcrumb(message);
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Log Breadcrumb: {message}");
        }
        
        internal static void LogException(Exception exception)
        {
            if (!_isEnabled || exception == null) {
                return;
            }

            if (!_isStarted) {
                StoredValues.logs.Add(exception);
                return;
            }
            
            EmbraceSDKBridge.LogException(exception);

            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Log Exception: {exception.Message}");
        }

        private static void ApplyStoredValues()
        {
            if (!_isEnabled || !_isStarted) {
                return;
            }
            
            if (!string.IsNullOrEmpty(StoredValues.userId)) {
                SetUserId(StoredValues.userId);
            }

            if (StoredValues.isPayer) {
                SetUserAsPayer();
            }
            
            foreach (string persona in StoredValues.personas) {
                SetPersona(persona);
            }
            
            foreach (KeyValuePair<string, string> sessionProperty in StoredValues.sessionProperties) {
                SetCustomData(sessionProperty.Key, sessionProperty.Value);
            }
            
            foreach (object log in StoredValues.logs) {
                switch (log) {
                    case string s:
                        Log(s);
                        break;
                    case Exception e:
                        LogException(e);
                        break;
                }
            }
        }
        
        // [Embrace] Personas must only consist of alphanumeric characters or underscores and be at most 32 characters
        private static string FormatPersonaString(string persona)
        {
            string fixedPersona = Regex.Replace(persona, "[^0-9A-Za-z_]", "");

            if (fixedPersona.Length > EMBRACE_PERSONA_MAX_LENGTH) {
                fixedPersona = fixedPersona.Substring(0,EMBRACE_PERSONA_MAX_LENGTH);
            }

            return fixedPersona;
        }
    }

    /// <summary>
    /// An object of this class is used to store all the data for Embrace before this crash reporter is started.
    /// Once Embrace is started all the stored values in this object are injected in Embrace. 
    /// </summary>
    internal class EmbraceStoredValues
    {
        public string userId;
        public bool isPayer;
        public readonly List<string> personas = new List<string>();
        public readonly Dictionary<string,string> sessionProperties = new Dictionary<string,string>();
        public readonly List<object> logs = new List<object>();
    }
}