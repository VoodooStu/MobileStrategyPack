using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Extension;

// ReSharper disable once CheckNamespace
namespace Voodoo.Analytics
{
    internal static class AnalyticsUtil
    {
#region Constants

        private const string TAG = "Analytics - Util";

        private const string CONTEXT_VAR_REGEXP_PATTERN = "^[a-z_]+$";

#endregion

#region JSON Methods

        internal static string ConvertDictionaryToJson(Dictionary<string, object> eventCustomData,
                                                       string dataJson = null,
                                                       string customVariables = null,
                                                       string contextVariables = null)
        {
            //Remove null values
            Dictionary<string, object> jsonDic = eventCustomData.RemoveNullValues();

            if (dataJson != null)
            {
                jsonDic.Add(AnalyticsConstant.DATA, JsonConvert.DeserializeObject(dataJson));
            }
            
            if (customVariables != null)
            {
                jsonDic.Add(AnalyticsConstant.CUSTOM_VARIABLES, JsonConvert.DeserializeObject(customVariables));
            }
            
            if (contextVariables != null)
            {
                jsonDic.Add(AnalyticsConstant.CONTEXT_VARIABLES, JsonConvert.DeserializeObject(contextVariables));
            }
            
            return JsonConvert.SerializeObject(jsonDic);
        }

        internal static string ConvertDictionaryToCustomVarJson(Dictionary<string, object> eventCustomVariables)
        {
            var jsonDictionary = new Dictionary<string, object>();
            var counter = 0;
            foreach (KeyValuePair<string, object> kvp in eventCustomVariables)
            {
                jsonDictionary.Add($"c{counter}_key", kvp.Key);
                jsonDictionary.Add($"c{counter}_val", kvp.Value?.ToString() ?? "");
                counter++;
            }
            
            return JsonConvert.SerializeObject(jsonDictionary);
        }

        internal static string ConvertDictionaryToContextVarJson(Dictionary<string, object> eventContextVariables)
        {
            var jsonDictionary = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> keyValue in eventContextVariables)
            {
                //check key
                string key = keyValue.Key.FormatKeyName();
                if (!Regex.IsMatch(key, CONTEXT_VAR_REGEXP_PATTERN)) {
                    var e = new Exception(
                        $"Context Property name '{keyValue.Key}' on VoodooAnalytics event is invalid. Property name must match pattern \"{CONTEXT_VAR_REGEXP_PATTERN}\"");
                    AnalyticsLog.CustomLoggerLogE(TAG, e);
                    AnalyticsLog.CustomLoggerReportE(e);
                    continue;
                }

                //check value
                object value = CheckValue(keyValue.Value);
                if (value != null) {
                    jsonDictionary.Add(key, value);
                }
            }
            
            return JsonConvert.SerializeObject(jsonDictionary);
        }
        
#endregion

#region Private Methods

        private static object CheckValue(object obj)
        {
            switch (obj)
            {
                case null:
                    return null;
                case string _:
                case char _:
                case byte _:
                case decimal _:
                case int _:
                case long _:
                case sbyte _:
                case short _:
                case uint _:
                case ulong _:
                case ushort _:
                case bool _:
                case float _:
                case double _:
                    return obj;
                default:
                    // The other types are not accepted
                    var e = new Exception(
                        $"Context Property value on VoodooAnalytics event must not be an object of type {obj.GetType().FullName}");
                    AnalyticsLog.CustomLoggerLogE(TAG, e);
                    AnalyticsLog.CustomLoggerReportE(e);
                    return null;
            }
        }

#endregion
    }
}