using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public static class DebugLogTypes
    {
        private const string DEBUG_LOG_TYPES_KEY = "Voodoo_DebuggerLogTypes";
        
        private static readonly Dictionary<LogType, bool> EnabledLogTypes = new Dictionary<LogType, bool> {
            {LogType.Exception, true}, 
            {LogType.Error, true}, 
            {LogType.Warning, true}
        };
        
        private static bool _isInitialized = false;

        public static bool IsEnabled(LogType type)
        {
            Load();
            return EnabledLogTypes.ContainsKey(type) && EnabledLogTypes[type];
        }

        public static void Update(LogType type, bool isEnabled)
        {
            Load();
            EnabledLogTypes[type] = isEnabled;
            Save();
        }

        public static void Reset()
        {
            _isInitialized = false;

            EnabledLogTypes[LogType.Exception] = true;
            EnabledLogTypes[LogType.Error] = true;
            EnabledLogTypes[LogType.Warning] = true;
        }
        
        private static void Load()
        {
            if (_isInitialized) {
                return;
            }

            _isInitialized = true;
            
            int types = PlayerPrefs.GetInt(DEBUG_LOG_TYPES_KEY, 0b111);

            EnabledLogTypes[LogType.Exception] = (types & 0b1) == 0b1;
            EnabledLogTypes[LogType.Error] = (types >> 0b1 & 0b1) == 0b1;
            EnabledLogTypes[LogType.Warning] = (types >> 0b10 & 0b1) == 0b1;
        }

        private static void Save()
        {
            var types = 0b0;
            if (EnabledLogTypes[LogType.Exception]) {
                types |= 0b1;
            }
            if (EnabledLogTypes[LogType.Error]) {
                types |= 0b10;
            }
            if (EnabledLogTypes[LogType.Warning]) {
                types |= 0b100;
            }
            
            PlayerPrefs.SetInt(DEBUG_LOG_TYPES_KEY, types);
            PlayerPrefs.Save();
        }
    }
}