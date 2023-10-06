using System;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.CrashReport;
using Voodoo.Sauce.Internal;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Firebase
{
    public class FirebaseCrashlyticsProvider : ICrashlyticsProvider
    {
#region Constants

        private const string TAG = "FirebaseCrashlyticsProvider";
        
#endregion

#region ICrashlyticsProvider Methods
        
        public void Configure(VoodooSettings settings) { }

        public void Initialize(ref Action<bool> analyticsConsentEvent)
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Initialized");
            analyticsConsentEvent += FirebaseCrashlyticsWrapper.SubscribeToFirebaseInitialization;
        }

        CrashReportCore.CrashReporter ICrashlyticsProvider.CrashReporterType() => CrashReportCore.CrashReporter.Firebase;

        float ICrashlyticsProvider.UserPercentage() => float.MinValue; // As the default crash reporter, the percentage is negative 
        
        public void LogLevelStart(string level) => FirebaseCrashlyticsWrapper.SetLevelStart(level);
        
        public void LogLevelFinish(string level) => FirebaseCrashlyticsWrapper.SetLevelFinish(level);

        public void LogException(Exception exception) => FirebaseCrashlyticsWrapper.LogException(exception);

        public void SetCustomData(string key, string value) => FirebaseCrashlyticsWrapper.SetCustomData(key, value);

        public void SetUserId(string userId) => FirebaseCrashlyticsWrapper.SetUserId(userId);
        
        public void SetUserProfile(string profile) { }
        
        public void Log(string message) => FirebaseCrashlyticsWrapper.Log(message);
        
#endregion
    }
}