using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.RemoteConfig;
using Voodoo.Tune.Core;
#pragma warning disable 4014

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.VoodooTune
{
    public static class VoodooTuneManager
    {
        private const string TAG = "VoodooTuneManager";
        private const int DEFAULT_INIT_TIMEOUT_IN_MILLISECONDS_WITHOUT_CACHE = 3000;

        private static readonly VoodooTuneConfigurationManager _configurationManager = new VoodooTuneConfigurationManager();

        private static string _initError;
        private static bool _isTimeout;
        private static string _initFinishedMessage;
        private static bool _initFinished = false;

        internal static VoodooTuneInitAnalyticsInfoLog LastVoodooTuneInitInfo;

        private static VoodooTuneAbTestsTracker abTestTracker;
        private static bool HasCache => !string.IsNullOrEmpty(VoodooTunePersistentData.SavedConfig);

        // A flag to easily know if the VT configuration is misconfigured regarding the AB tests. 
        private static bool IsAbTestValid => _configurationManager.VoodooConfig?.IsValid ?? false;
        
        /// <summary>
        /// Returns the Timeout used in the last VoodooTune initialization.
        /// </summary>
        public static int CurrentTimeoutInMilliseconds { get; private set; }
        private static double CurrentTimeoutInSeconds => Convert.ToDouble(CurrentTimeoutInMilliseconds / 1000f);

        public static async void Initialize(Action initFinishedEvent)
        {
            if (_initFinished)
            {
                _initFinishedMessage = _initError;
                initFinishedEvent?.Invoke();
                return;
            }
            
            // Load local VoodooTune data
            _configurationManager.LoadLocalConfiguration();
            DebugVTManager.Initialize();

            // Init the AB Tests Tracker with the local AB Tests
            abTestTracker = new VoodooTuneAbTestsTracker();
            
            // Define max call duration
            SetTimeout();

            // Launch VoodooTune Configuration Request
            Task<ConfigResponse> configRequest = await SendLoadingConfigurationRequest();

            if (configRequest.IsCompleted) // Directly send the init finished event
            {
                SaveConfiguration(configRequest);
                
                initFinishedEvent?.Invoke();
            }
            else // Wait for the new configuration to be loaded
            {
                initFinishedEvent?.Invoke();
                
                await configRequest;

                SaveConfiguration(configRequest);
            }
            
            _initFinished = true;
        }

        private static void SetTimeout()
        {
            var voodooTuneConfiguration = GetItemOrDefault<RemoteConfigConfiguration>();
            CurrentTimeoutInMilliseconds = HasCache ? Math.Max(0, voodooTuneConfiguration.InitTimeoutInMillisecondsWithCache) : DEFAULT_INIT_TIMEOUT_IN_MILLISECONDS_WITHOUT_CACHE;
            VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, $"Has cache: {HasCache}, Init with current timeout -> {CurrentTimeoutInMilliseconds}ms");
        }

        private static async Task<Task<ConfigResponse>> SendLoadingConfigurationRequest()
        {
            // Setup and launch request
            var configurationManager = new VoodooTuneConfigurationManager(VoodooTunePersistentData.SavedServer);
            var configRequest =  configurationManager.LoadConfiguration();
            
            if (CurrentTimeoutInMilliseconds == 0)
            {
                VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, "No need to wait for the Init response");
                _isTimeout = true;
                return configRequest;
            }
            
            //Setup the timeout system
            CancellationTokenSource cts = new CancellationTokenSource(CurrentTimeoutInMilliseconds);

            // Wait for the request to end
            while (cts.IsCancellationRequested == false && configRequest.IsCompleted == false)
            {
                await Task.Yield();
            }

            if (cts.IsCancellationRequested)
            {
                CancelInitRequest();
            }
            
            return configRequest;
        }

        private static void CancelInitRequest()
        {
            _isTimeout = true;
            _initFinishedMessage = "VoodooTune TimeOut";
                
            VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, $"{_initFinishedMessage} - Cancelling VT init after {CurrentTimeoutInSeconds}s timeout");
            UpdateLastInitInfoAsTimeout();
        }

        private static void SaveConfiguration(Task<ConfigResponse> configRequest)
        {
            ConfigResponse config = configRequest.Result;
            SaveConfiguration(config.Url, config.Response, config.Error, config.ResponseCode, config.DurationInMilliseconds);

            if (_isTimeout == false)
            {
                _initFinishedMessage = config.Error;
            }
        }
        
        private static void SaveConfiguration(string url, string response, string error, long responseCode, double durationInMilliseconds)
        {
            string message = ManageResponse(response, responseCode, error);

            VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, durationInMilliseconds + " " + message);
            
            VoodooTunePersistentData.SavedURL = url;

            var infos = new VoodooTuneInitAnalyticsInfo
            {
                HttpResponseCode = responseCode,
                DurationInMilliseconds = durationInMilliseconds,
                HasCache = HasCache,
                HasTimeout = _isTimeout,
                FormatIssue = !IsAbTestValid
            };

            LastVoodooTuneInitInfo = new VoodooTuneInitAnalyticsInfoLog(infos, response, error, message);

            if (DebugVTManager.HasDebugBehaviorApplied() == false)
            {
                AnalyticsManager.TrackVoodooTuneInitEvent(infos);
            }
        }

        private static string ManageResponse(string response, long responseCode, string error)
        {
            string message;
            if (!string.IsNullOrEmpty(response) && responseCode == 200)
            {
                message = "Init succeeded - ";
                if (_isTimeout)
                {
                    message += $"Save config for the next session: {response}";
                    _configurationManager.SaveConfig(response);
                }
                else
                {
                    message += $"Save and load config: {response}";
                    _configurationManager.SaveAndRefreshConfig(response);

                    if (IsAbTestValid == false)
                    {
                        message += " (the AB tests are misconfigured, please check the AB tests and the cohorts)";
                    }
                }

                TrackAbTestModifications(response);
            }
            else
            {
                if (error != null) // An error occured
                {
                    _initError = error;
                    message = $"Init failed with error: {error}";
                }
                else if (responseCode == 204) // URL not valid
                {
                    _initError = "no configuration for this app";
                    message = $"Init failed: {_initError}";
                }
                else // All other cases
                {
                    _initError = $"responseCode: {responseCode} response: {response}";
                    message = $"Init failed: {_initError}";
                }
            }

            return message;
        }

        private static void TrackAbTestModifications(string response)
        {
            Voodoo.Tune.Core.VoodooConfig voodooConfig = _configurationManager.GetVoodooConfig(response);
            if (voodooConfig == null)
                return;
                
            abTestTracker.TrackAbTestModifications(voodooConfig);
        }

        private static void UpdateLastInitInfoAsTimeout()
        {
            var infos = new VoodooTuneInitAnalyticsInfo
            {
                HttpResponseCode = -1,
                DurationInMilliseconds = CurrentTimeoutInMilliseconds,
                HasCache = HasCache,
                HasTimeout = _isTimeout
            };
            
            LastVoodooTuneInitInfo = new VoodooTuneInitAnalyticsInfoLog(infos, "", _initFinishedMessage, _initFinishedMessage);
        }
        
#region Getters

        /// <summary>
        /// Returns a list of instances of the given type T and its subtypes, from the VoodooTune cache.
        /// if there is no instance present in the cache, the list will contain an instance with the default values defined in the class T.
        /// </summary>
        /// <returns>A list of instances of the given type T and its subtypes.</returns>
        public static List<T> GetSubclassesItems<T>() where T : class, new() => _configurationManager.GetSubclassesItems<T>();

        /// <summary>
        /// Returns a list of instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the list will contain an instance with the default values defined in the class T.
        /// </summary>
        /// <returns>A list of instances of the given type T.</returns>
        public static List<T> GetItems<T>() where T : class, new() => _configurationManager.GetItems<T>();

        /// <summary>
        /// Returns a list of instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the method returns a list of instances with the default values defined in the Resources json files.
        /// </summary>
        /// <returns>An instance of the given type T.</returns>
        public static List<T> GetItemsOrDefaults<T>() where T : class, new() => _configurationManager.GetItemsOrDefaults<T>();

        /// <summary>
        /// Returns the first instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the method returns null.
        /// </summary>
        /// <returns>An instance of the given type T or null.</returns>
        public static T GetItem<T>() where T : class, new() => _configurationManager.GetItem<T>();

        /// <summary>
        /// Returns the first instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the method returns an instance with the default values defined in the class T.
        /// </summary>
        /// <returns>An instance of the given type T. </returns>
        public static T GetItemOrDefault<T>() where T : class, new() => _configurationManager.GetItemOrDefault<T>();

        /// <returns>Configuration Id. </returns>
        public static string GetConfigurationId() => _configurationManager?.VoodooConfig?.ConfigurationId;

        /// <returns>Segmentation Id.</returns>
        public static string GetSegmentationUuid() => _configurationManager?.VoodooConfig?.SegmentIds;

        /// <returns>Segmentation Ids as list.</returns>
        public static List<string> GetSegmentationUuidsAsList() => _configurationManager?.VoodooConfig?.SegmentIdsToList ?? new List<string>();

        /// <returns>First AbTest Id from the list.</returns>
        public static string GetMainAbTestUuid() => _configurationManager?.VoodooConfig?.MainAbTestId;

        /// <returns>AbTest Ids as list.</returns>
        public static List<string> GetAbTestUuidsAsList() => _configurationManager?.VoodooConfig?.AbTestIdsToList ?? new List<string>();

        /// <returns>AbTest Ids.</returns>
        public static string GetAbTestUuids() => _configurationManager?.VoodooConfig?.AbTestIds;
        
        /// <returns>First AbTest Cohort Name from the list.</returns>
        public static string GetMainAbTestCohortName() => _configurationManager?.VoodooConfig?.MainCohortName;

        /// <returns>First AbTest Cohort Id from the list.</returns>
        public static string GetMainAbTestCohortUuid() => _configurationManager?.VoodooConfig?.MainCohortId;

        /// <returns>AbTest Cohort Ids as list.</returns>
        public static List<string> GetAbTestCohortUuidsAsList() =>  _configurationManager?.VoodooConfig?.CohortIdsToList ?? new List<string>();

        /// <returns>AbTest Cohort Ids.</returns>
        public static string GetAbTestCohortUuids() => _configurationManager?.VoodooConfig?.CohortIds;

        /// <returns>AbTest Version Id.</returns>
        public static string GetAbTestVersionUuid() => _configurationManager?.VoodooConfig?.VersionNumber;

        /// <returns>A string instance of JSON content.</returns>
        public static bool GetDebuggerAuthorization() => _configurationManager?.VoodooDebug?.Authorized ?? false;

        /// <returns>The parsed configuration as it is in the json file</returns>
        public static Dictionary<string, string> GetItemsJson() => _configurationManager.GetJsonConfigurations();
        
#endregion
    }
}