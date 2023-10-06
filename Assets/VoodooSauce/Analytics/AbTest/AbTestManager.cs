using System;
using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal static class AbTestManager
    {
        private const string TAG = "AbTestManager";
        private static bool _debugModeForced;
        private static string[] _runningAbTests;

        private static bool _useVoodooTune;
        private static VoodooSettings _voodooSettings;

        internal static void Initialize(VoodooSettings settings)
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Initializing AB Tests");
            _voodooSettings = settings;
            _useVoodooTune = _voodooSettings.UseRemoteConfig;
            if (_useVoodooTune) return;

            string[] abTests = _voodooSettings.GetRunningABTests();
            if (abTests == null || abTests.Length == 0) return;

            _runningAbTests = AbTestHelper.AddControlCohortsToAbTests(abTests);
            DebugForcedCohort debugForcedCohort = _voodooSettings.GetDebugForcedCohort();
            if (debugForcedCohort.IsDebugCohort()) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Re-initializing cohort because debug is enabled.");
                _debugModeForced = true;
                SetPlayerCohort(debugForcedCohort.HasForcedNoCohort() ? null : debugForcedCohort.GetCohort());
            } else if (ShouldCreateCohort(_voodooSettings)) {
                string cohort = AbTestHelper.GenerateNewRandomCohort(_runningAbTests, _voodooSettings.GeUsersPercentPerCohort());
                SetPlayerCohort(cohort);
            }

            VoodooLog.LogDebug(Module.ANALYTICS, TAG,
                $"AB Test Status: AB Test: {GetPlayerCohort()} - Event tracked: {PlayerIsInACohort()} - People tracked: false");
        }

        private static bool ShouldCreateCohort(VoodooSettings settings)
        {
            if (!AnalyticsStorageHelper.Instance.IsFirstAppLaunch()) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Not initializing AB Test because not first time app launched");
                return false;
            }
			string locale = PrivacyUtils.GetLocale().ToLower();
            string[] abTestsCountryCodes = settings.IsCustomABTestsCountryCodesEnabled()
                ? settings.GetCustomABTestsCountryCodes()
                : new[] {AbTestHelper.ABTestDefaultCountryCode};

            if (abTestsCountryCodes == null || abTestsCountryCodes.All(code => code.ToLower() != locale)) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Not initializing AB Test with {locale}");
                return false;
            }

            VoodooLog.LogDebug(Module.ANALYTICS, TAG,
                $"Initializing AB Test because first time app launched with {Application.platform} - {locale}");
            return true;
        }

        internal static bool PlayerIsInACohort() => GetPlayerCohort() != null;

        internal static string GetPlayerCohort()
        {
            if (_useVoodooTune) return VoodooTuneManager.GetMainAbTestCohortName();
            string savedPlayerCohort = AbTestHelper.GetSavedPlayerCohort();
            if (!string.IsNullOrEmpty(savedPlayerCohort) && _runningAbTests != null) {
                if (_runningAbTests.Contains(savedPlayerCohort)) {
                    return savedPlayerCohort;
                }
                SetPlayerCohort(null);
            }
            return null;
        }

        internal static string[] GetPlayerCohorts()
        {
            if (_useVoodooTune) return VoodooTuneManager.GetAbTestCohortUuidsAsList().ToArray();
            string savedPlayerCohort = GetPlayerCohort();
            return savedPlayerCohort != null ? new [] { savedPlayerCohort } : new string[] { };
        }

        internal static int GetPlayerCohortIndex()
        {
            string savedPlayerCohort = AbTestHelper.GetSavedPlayerCohort();
            if (savedPlayerCohort != null && _runningAbTests != null) {
                return Array.IndexOf(_runningAbTests, savedPlayerCohort);
            }

            return -1;
        }

        internal static void SetPlayerCohort(string cohort)
        {
            if (_useVoodooTune) return;
            if (_voodooSettings == null) {
                VoodooLog.LogError(Module.ANALYTICS, TAG, "Do not set player cohort before AbTestManager initialization");
                return;
            }
            AbTestHelper.SavePlayerCohort(cohort);
            TrackAbTestAssignment(_voodooSettings.LegacyAbTestName, cohort);
        }
        
        private static void TrackAbTestAssignment(string abTest, string cohort)
        {
            if (string.IsNullOrEmpty(abTest) || string.IsNullOrEmpty(cohort)) {
                VoodooLog.LogError(Module.ANALYTICS, TAG, "TrackAbTestAssignment: parameters should not be null or empty");
                return;
            }
            AnalyticsManager.TrackVoodooTuneAbTestAssignment(new VoodooTuneAbTestAnalyticsInfo(abTest, cohort));
        }
        
        private static void TrackAbTestExit(string abTest, string cohort)
        {
            if (string.IsNullOrEmpty(abTest) || string.IsNullOrEmpty(cohort)) {
                VoodooLog.LogError(Module.ANALYTICS, TAG, "TrackAbTestExit: parameters should not be null or empty");
                return;
            }
            AnalyticsManager.TrackVoodooTuneAbTestExit(new VoodooTuneAbTestAnalyticsInfo(abTest, cohort));
        }

        internal static string[] GetAbTests() => _runningAbTests;

        internal static bool IsDebugModeForced() => _debugModeForced;
    }
}
