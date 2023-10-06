using System;
using UnityEngine;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Internal
{
    public enum Module
    {
        NONE = 0,
        ADS = 1,
        ANALYTICS = 2,
        APP_RATER = 4,
        CROSS_PROMO = 8,
        PRIVACY = 16,
        VOODOO_TUNE = 32,
        IAP = 64,
        COMMON =  128,

        ALL_MODULES = 255
    }

    public static class VoodooLog
    {
        private const string TAG = "VoodooSauce";

        public const string LOG_LEVEL_PLAYER_PREFS_KEY = "VoodooLogLevel2";
        public const string FILTER_PLAYER_PREFS_KEY = "VoodooLogFilter";

        static ILogger _logger;

        public static event Action<string, string, LogType> logReceived
        {
            add => _logger.logReceived += value;
            remove => _logger.logReceived -= value;
        }

        public static bool IsDebugLogsEnabled => _logger.LogLevel == LogType.Log;

        internal static LogType GetLogLevel => _logger.LogLevel;

        internal static int ModuleFilter => _logger.ModuleFilter;

        static VoodooLog()
        {
            Debug.unityLogger.logEnabled = true;
            _logger = new VoodooLogger();
            _logger.ModuleFilter = Math.Min(PlayerPrefs.GetInt(FILTER_PLAYER_PREFS_KEY, (int)Module.ALL_MODULES), (int)Module.ALL_MODULES);
            LogType defaultLogLevel;
            
            if (Application.isEditor || Debug.isDebugBuild) {
                defaultLogLevel = LogType.Log;
            } else {
                defaultLogLevel = LogType.Error;
            }
            
            LogType logLevel = (LogType)PlayerPrefs.GetInt(LOG_LEVEL_PLAYER_PREFS_KEY, (int)defaultLogLevel);
            SetLogLevel(logLevel);
        }
                
        public static void LogDebug(Module module, string tag, string message)
            => Log(module, tag, message, LogType.Log);

        public static void LogError(Module module, string tag, string message)
            => Log(module, tag, message, LogType.Error);

        public static void LogWarning(Module module, string tag, string message)
            => Log(module, tag, message, LogType.Warning);

        static void Log(Module module, string tag, string message, LogType logType)
            => _logger.Log(Format(module, tag, message), module, logType);
        
        static string Format(Module module, string tag, string message)
        {
            return $"{TAG}/{module}/{tag}: {message}";
        }

        public static void EnableDebugLogs(bool enable) => SetLogLevel((int)(enable ? LogType.Log : LogType.Error));

        internal static void SetLogLevel(LogType logType) => SetLogLevel((int) logType);

        internal static void SetLogLevel(int value)
        {
            _logger.LogLevel = (LogType)value;
            Debug.unityLogger.filterLogType = _logger.LogLevel;
            PlayerPrefs.SetInt(LOG_LEVEL_PLAYER_PREFS_KEY, value);

            UpdateAnalyticsLogLevel();
        }
        
        internal static void ToggleModule(int value)
        {
            _logger.ModuleFilter = value ^ _logger.ModuleFilter;
            PlayerPrefs.SetInt(FILTER_PLAYER_PREFS_KEY, _logger.ModuleFilter);
            
            UpdateAnalyticsLogLevel();
        }

        private static void UpdateAnalyticsLogLevel()
        {
            bool enable = IsDebugLogsEnabled && (ModuleFilter & (int)Module.ANALYTICS) > 0;
            AnalyticsManager.SetLogLevel(enable, _logger.LogLevel);
        }
    }
}
