using System;
using Firebase.Crashlytics;
using Voodoo.Analytics;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.CrashReport;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Firebase;
using Voodoo.Sauce.Internal.VoodooTune;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Firebase
{
    internal static class FirebaseCrashlyticsWrapper
    {
#region Constants

        private const string TAG = "FirebaseCrashlyticsWrapper";
        
#endregion
        
#region Properties

        private static bool _isFirebaseInitialized;
        private static bool _isEnabled;

#endregion

        internal static void SubscribeToFirebaseInitialization(bool consent)
        {
            if (!consent) return;
            
            FirebaseInitializer.Subscribe(initSuccess => {
                _isFirebaseInitialized = initSuccess;
                _isEnabled = initSuccess;
                SetCrashlyticsCollection(true);
                SetUserId(AnalyticsUserIdHelper.GetUserId());
                SetCustomData(FirebaseCrashlyticsConstants.MEDIATION_KEY, AdsManager.MediationAdapter.GetMediationType().ToString());
                SetCustomData(CrashReportConstants.VERSION_KEY, VoodooConfig.Version());
                SetCustomData(CrashReportConstants.SEGMENT_UUID, VoodooTuneManager.GetSegmentationUuid() ?? string.Empty);
                SetCustomData(CrashReportConstants.AB_TEST_UUID, VoodooTuneManager.GetAbTestUuids() ?? string.Empty);
                SetCustomData(CrashReportConstants.COHORT_UUID, VoodooTuneManager.GetAbTestCohortUuids() ?? string.Empty);
                SetCustomData(CrashReportConstants.VERSION_UUID, VoodooTuneManager.GetAbTestVersionUuid() ?? string.Empty);
            });
            
            FirebaseInitializer.Start();
        }

        internal static void SetLevelStart(string levelNumber)
        {
            Log($"{CrashReportConstants.LEVEL_START_KEY}: {levelNumber}");
            SetCustomData(CrashReportConstants.LEVEL_NUM_KEY, levelNumber);
        }

        internal static void SetLevelFinish(string levelNumber) => Log($"{CrashReportConstants.LEVEL_FINISH_KEY}: {levelNumber}");

        internal static void SetUserId(string userId)
        {
            if (!_isFirebaseInitialized || !_isEnabled) return;

            Crashlytics.SetUserId(userId);
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"User ID: {userId}");
        }

        internal static void SetCustomData(string key, string value)
        {
            if (!_isFirebaseInitialized || !_isEnabled) return;

            Crashlytics.SetCustomKey(key, value);
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Custom Data: {key} = {value}");
        }

        internal static void Log(string message)
        {
            if (!_isFirebaseInitialized || !_isEnabled) return;

            Crashlytics.Log(message);
            
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Log: {message}");
        }

        internal static void LogException(Exception exception)
        {
            if (!_isFirebaseInitialized || !_isEnabled) return;

            Crashlytics.LogException(exception);
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Log Exception: {exception.Message}");

            if (exception.InnerException != null) {
                Crashlytics.LogException(exception.InnerException);
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Log Exception: {exception.InnerException.Message}");
            }
        }

        private static void SetCrashlyticsCollection(bool consent)
        {
            if (!_isFirebaseInitialized) return;
            Crashlytics.IsCrashlyticsCollectionEnabled = consent;
        }
    }
}