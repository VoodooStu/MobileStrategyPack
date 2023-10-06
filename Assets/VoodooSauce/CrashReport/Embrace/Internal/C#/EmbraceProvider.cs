using System;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    public class EmbraceProvider : ICrashlyticsProvider
    {
#region Constants

        private const string TAG = "EmbraceProvider";
        
#endregion

#region Properties

        private readonly EmbraceConfiguration _configuration = new EmbraceConfiguration();

#endregion

#region ICrashlyticsProvider Methods

        public void Configure(VoodooSettings settings)
        {
            // Get the configuration from Kitchen
            _configuration.appId = settings.EmbraceAppId;
            _configuration.percentage = settings.EmbraceUserPercentage;

            // Get the configuration from VoodooTune
            var remoteConfiguration = VoodooSauce.GetItem<EmbraceConfiguration>();
            if (remoteConfiguration != null) {
                _configuration.percentage = remoteConfiguration.percentage;
                if (!string.IsNullOrEmpty(remoteConfiguration.appId)) {
                    _configuration.appId = remoteConfiguration.appId;
                }
            }
            
            EmbraceWrapper.useRemoteConfig = settings.UseRemoteConfig;

            float percentage = _configuration.percentage;
            string appId = _configuration.appId;
            string apiToken = settings.EmbraceApiToken;

            // If a key is missing Embrace can not be initialized, so it's disabled by putting its percentage to 0.
            if (percentage > 0.0 && (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(apiToken))) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Disabled because of a misconfiguration (appId = '{appId}', apiToken = '{apiToken}')");
                _configuration.percentage = 0.0f;
            } else {
                float maxPercentage = settings.embraceMaxPercentage;
                if (percentage > maxPercentage) {
                    VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"The percentage exceeds the maximum ({percentage} > {maxPercentage}), percentage set to {maxPercentage}");
                    _configuration.percentage = maxPercentage;
                }
            }
        }

        public void Initialize(ref Action<bool> analyticsConsentEvent)
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Initialized");
            analyticsConsentEvent += EmbraceWrapper.AnalyticsConsentEvent;
        }

        CrashReportCore.CrashReporter ICrashlyticsProvider.CrashReporterType() => CrashReportCore.CrashReporter.Embrace;

        float ICrashlyticsProvider.UserPercentage() => _configuration.percentage;

        public void LogLevelStart(string level) => EmbraceWrapper.Log($"{CrashReportConstants.LEVEL_START_KEY}: {level}");

        public void LogLevelFinish(string level) => EmbraceWrapper.Log($"{CrashReportConstants.LEVEL_FINISH_KEY}: {level}");

        public void LogException(Exception exception) => EmbraceWrapper.LogException(exception);

        public void SetCustomData(string key, string value) => EmbraceWrapper.SetCustomData(key, value);

        public void SetUserId(string userId) => EmbraceWrapper.SetUserId(userId);
        
        public void SetUserProfile(string profile) => EmbraceWrapper.SetPersona(profile);
        
        public void Log(string message) => EmbraceWrapper.Log(message);

#endregion
    }
}