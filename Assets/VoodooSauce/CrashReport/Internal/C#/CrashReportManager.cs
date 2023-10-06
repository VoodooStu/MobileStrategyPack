using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Utils;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport
{
    [Preserve]
    internal class CrashReportManager : CrashReportCore
    {
#region Constants

        private const string TAG = "CrashReportManager";
        
        private const string PLAYER_PREF_CRASH_REPORTER_V1 = "PLAYER_PREF_CRASHLYTICSVoodoo.Sauce.CrashReport.CrashReporter";
        private const string PLAYER_PREF_CRASH_REPORTER_V2 = "PLAYER_PREF_CRASHLYTICSVoodoo.Sauce.CrashReport.CrashReporter.v2";

#endregion

#region Properties

        // If another crash reporter than Firebase is used in the future please set this setting in the VoodooSettings class;
        // so use this value into the method GetCrashReporter to know if this another crash reporter must be loaded instead of Firebase.
        private VoodooSettings _voodooSettings;

        private ICrashlyticsProvider _crashReporter;
        private readonly Dictionary<CrashReporter, float> _crashReporterPercentages = new Dictionary<CrashReporter, float>(); 

#endregion

#region CrashReportCore Methods

        public override void Initialize(CrashReportManagerParameters parameters)
        {
            RemoveOutdatedSavedConfigurations();
            
            _voodooSettings = parameters.VoodooSettings;
            _crashReporter = SelectCrashReporter();
            _crashReporter?.Initialize(ref parameters.AnalyticsConsentEvent);
        }

        public override CrashReporter GetCrashReporter() => _crashReporter?.CrashReporterType() ?? CrashReporter.None;

        internal override void ForceCrashReporter(CrashReporter crashReporter)
        {
            PlayerPrefs.SetString(PLAYER_PREF_CRASH_REPORTER_V2, crashReporter.ToString());
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Crash reporter forced to: '{crashReporter.ToString()}'");
        }

        public override void LogLevelStart(string level) => _crashReporter?.LogLevelStart(level);

        public override void LogLevelFinish(string level) => _crashReporter?.LogLevelFinish(level);
        
        public override void LogException(Exception exception) => _crashReporter?.LogException(exception);

        public override void SetCustomData(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                VoodooLog.LogDebug(Module.COMMON, TAG, "Can not send a null or empty key on a custom data");
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                VoodooLog.LogDebug(Module.COMMON, TAG, $"Can not send a null or empty value as a custom data (key: '{key}')");
                return;
            }
            
            _crashReporter?.SetCustomData(key, value);
        }

        public override void Log(string message) => _crashReporter?.Log(message);
        
        public override void SetUserId(string userId) => _crashReporter?.SetUserId(userId);
        
        public override void SetUserProfile(string profile) => _crashReporter?.SetUserProfile(profile);

        public override float GetUserPercentage(CrashReporter crashReporter) => _crashReporterPercentages.ContainsKey(crashReporter) ? _crashReporterPercentages[crashReporter] : 0.0f;

#endregion

#region Private Methods

        private static void RemoveOutdatedSavedConfigurations()
        {
            if (PlayerPrefs.HasKey(PLAYER_PREF_CRASH_REPORTER_V1)) {
                PlayerPrefs.DeleteKey(PLAYER_PREF_CRASH_REPORTER_V1);
            }
        }

        private ICrashlyticsProvider SelectCrashReporter()
        {
            // No crash reporter is loaded in the Editor mode
            if (PlatformUtils.UNITY_EDITOR) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "No crash reporter is initialized in the Editor Mode");
                return null;
            }

            var crashReporters = new Dictionary<CrashReporter, ICrashlyticsProvider>();
            var crashReporterPercentages = new List<CrashReporterProviderBucket>();
            var crashReportersToDefinePercentage = new List<ICrashlyticsProvider>();
            var currentPercentage = 0.0;
            
            // Get all the crash reporters from this project
            List<Type> crashReporterTypes = AssembliesUtils.GetTypes(typeof(ICrashlyticsProvider));
            
            foreach (Type crashReporterType in crashReporterTypes) {
                // Configure the crash reporter
                var crashReporter = (ICrashlyticsProvider)Activator.CreateInstance(crashReporterType);
                crashReporter.Configure(_voodooSettings);
                
                CrashReporter crashReporterEnumValue = crashReporter.CrashReporterType();
                
                // Verify that this crash reporter is enabled. If it is not then it is ignored.
                float crashReportPercentage = crashReporter.UserPercentage();
                if (crashReportPercentage == 0.0) {
                    VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"The crash reporter '{crashReporterEnumValue.ToString()}' is disabled (because percentage == {crashReportPercentage})");
                    continue;
                }
                
                // If another crash reporter of the same type is already configured then this one is ignored.
                if (crashReporters.ContainsKey(crashReporterEnumValue)) {
                    VoodooLog.LogWarning(Module.ANALYTICS, TAG, $"More than one crash reporter '{crashReporterEnumValue.ToString()}' is defined, only one is kept");
                    continue;
                }
                
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Crash reporter '{crashReporterEnumValue.ToString()}' configured");
                crashReporters[crashReporterEnumValue] = crashReporter;

                // If the percentage is negative then it means that it must be defined automatically: let's see this a little bit later.
                if (crashReportPercentage < 0.0) {
                    crashReportersToDefinePercentage.Add(crashReporter);
                } else {
                    currentPercentage += crashReportPercentage;
                    crashReporterPercentages.Add(new CrashReporterProviderBucket(currentPercentage, crashReporter));
                    _crashReporterPercentages.Add(crashReporter.CrashReporterType(), crashReportPercentage);
                }
            }

            if (currentPercentage > 100.0) {
                VoodooLog.LogWarning(Module.ANALYTICS, TAG, $"The cumulative percentage of all the crash reporters exceeds 100%, please fix it in the VS settings or in VoodooTune");
            } else if (crashReportersToDefinePercentage.Count > 0) {
                // Let's define the percentage of the crash reporters that don't have defined percentage.
                double percentage = (100.0 - currentPercentage) / crashReportersToDefinePercentage.Count;
                foreach (ICrashlyticsProvider crashReporterToDefinePercentage in crashReportersToDefinePercentage) {
                    currentPercentage += percentage;
                    crashReporterPercentages.Add(new CrashReporterProviderBucket(currentPercentage, crashReporterToDefinePercentage));
                    _crashReporterPercentages.Add(crashReporterToDefinePercentage.CrashReporterType(), (float)percentage);
                }
            }

            // The same crash reporter is used in the different game sessions, this is why it's saved in the player prefs
            if (PlayerPrefs.HasKey(PLAYER_PREF_CRASH_REPORTER_V2)) {
                string crashReporterString = PlayerPrefs.GetString(PLAYER_PREF_CRASH_REPORTER_V2);
                
                // As some crash reporters could be removed from this SDK, we must ensure the saved crash reporter is still used.
                // If the crash reporter is an unused one, let's remove the saved key from the player prefs.
                // A new crash reporter will be assigned to this user.
                if (!Enum.TryParse(crashReporterString, out CrashReporter crashReporterEnumValue)) {
                    VoodooLog.LogWarning(Module.ANALYTICS, TAG, $"Used a crash reporter that is not used anymore '{crashReporterString}'");
                    PlayerPrefs.DeleteKey(PLAYER_PREF_CRASH_REPORTER_V2);
                } else {
                    // Verify that the crash reporter is configured.
                    if (crashReporters.ContainsKey(crashReporterEnumValue)) {
                        VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Crash reporter used: '{crashReporterString}' (from cache)");
                        return crashReporters[crashReporterEnumValue];
                    }
                    
                    VoodooLog.LogWarning(Module.ANALYTICS, TAG, $"The crash reporter '{crashReporterString}' should be used but it is not configured (is it disabled?)");
                }
            }
            
            // Now it's time to pick up a crash reporter randomly.
            float random = Random.Range(0.0f, (float)currentPercentage);
            foreach (CrashReporterProviderBucket crashReporterPercentage in crashReporterPercentages) {
                if (crashReporterPercentage.Percentage < random) {
                    continue;
                }
                
                ICrashlyticsProvider selectedCrashReporterProvider = crashReporterPercentage.CrashReporterProvider;
            
                // The chosen crash reporter is saved for the next game sessions
                PlayerPrefs.SetString(PLAYER_PREF_CRASH_REPORTER_V2, selectedCrashReporterProvider.CrashReporterType().ToString());

                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Crash reporter used: '{selectedCrashReporterProvider.CrashReporterType().ToString()}' (randomly picked up)");
                return selectedCrashReporterProvider;
            }

            VoodooLog.LogWarning(Module.ANALYTICS, TAG, "No crash reporter loaded");
            return null;
        }
        
#endregion
    }

    internal struct CrashReporterProviderBucket
    {
        internal readonly double Percentage;
        internal readonly ICrashlyticsProvider CrashReporterProvider;
        
        public CrashReporterProviderBucket(double percentage, ICrashlyticsProvider crashReporterProvider)
        {
            Percentage = percentage;
            CrashReporterProvider = crashReporterProvider;
        }
    }
}
