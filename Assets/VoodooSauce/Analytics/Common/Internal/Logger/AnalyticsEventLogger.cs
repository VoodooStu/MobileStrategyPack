using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Voodoo.Analytics;
using Voodoo.Sauce.Internal.Extension;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    internal class AnalyticsEventLogger
    {
        private const string TAG = "AnalyticsEventLogger";
        private const string VOODOO_ANALYTICS_WRAPPER_NAME = "VoodooAnalytics";
        private const string PLAYER_PREF_RECORDING_AT_STARTUP_KEY = "van_debugger_recording_startup";
        private static AnalyticsEventLogger _instance;
        private readonly List<DebugAnalyticsLog> _logsList = new List<DebugAnalyticsLog>(500);
        private readonly HashSet<string> _logsIdList = new HashSet<string>();
        private bool _isAnalyticsDebuggingEnabled;
        
        internal event Action<bool> OnRecordingStateChange;
        internal bool IsRecordingEvents { get; private set; }

        internal bool IsRecordingAtStartup
        {
            get {
                if (PlayerPrefs.HasKey(PLAYER_PREF_RECORDING_AT_STARTUP_KEY)) {
                    return PlayerPrefs.GetInt(PLAYER_PREF_RECORDING_AT_STARTUP_KEY) == 1;
                }

                return Application.identifier == VoodooConstants.TEST_APP_BUNDLE;
            }
            set
            {
                if (value)
                {
                    PlayerPrefs.SetInt(PLAYER_PREF_RECORDING_AT_STARTUP_KEY, 1);
                }
                else
                {
                    PlayerPrefs.SetInt(PLAYER_PREF_RECORDING_AT_STARTUP_KEY, 0);
                }
            }
        }

        internal List<DebugAnalyticsLog> GetLocalAnalyticsLog(string wrapperNameFilter = null)
        {
            return string.IsNullOrEmpty(wrapperNameFilter)
                ? _logsList
                : _logsList.Where(nameInList => nameInList.WrapperName.Contains(wrapperNameFilter)).ToList();
        }

        internal static event Action<DebugAnalyticsLog, bool> OnAnalyticsEventStateChanged;

        internal static AnalyticsEventLogger GetInstance() => _instance ?? (_instance = new AnalyticsEventLogger());

        private AnalyticsEventLogger() {}
            
        internal void Init()
        {
            SetAnalyticsEventRecording(IsRecordingAtStartup);
        }

        private void LogEventLocallyIfRelevant(string wrapperName,
                                               string eventName,
                                               DebugAnalyticsStateEnum state,
                                               string eventId,
                                               Dictionary<string, object> param = null,
                                               string error = "")
        {
            var isAlreadyCaughtEvent = state != DebugAnalyticsStateEnum.ForwardedTo3rdParty && _logsIdList.Contains(eventId);

            if (!_isAnalyticsDebuggingEnabled && !IsRecordingEvents && !isAlreadyCaughtEvent) return;

            var localAnalyticsLog = new DebugAnalyticsLog(wrapperName, eventName, param, state, eventId, error,
                AnalyticsSessionManager.Instance().SessionInfo.id, GetAdditionalInformationFromJson(param));
            var isUpdateFromExisting = false;

            if (IsRecordingEvents || isAlreadyCaughtEvent) {
                if (!isAlreadyCaughtEvent) {
                    _logsList.Add(localAnalyticsLog);
                    _logsIdList.Add(localAnalyticsLog.EventId);
                } else {
                    var index = _logsList.FindIndex(logItem => logItem.EventId.Contains(localAnalyticsLog.EventId));
                    _logsList[index] = localAnalyticsLog;
                    isUpdateFromExisting = true;
                }
            }

            OnAnalyticsEventStateChanged?.Invoke(localAnalyticsLog, isUpdateFromExisting);
        }

        internal void LogEventSentTo3RdParty(string wrapperName,
                                             string eventName,
                                             string eventId,
                                             [CanBeNull] Dictionary<string, object> param = null)
        {
            LogEventLocallyIfRelevant(wrapperName, eventName, DebugAnalyticsStateEnum.ForwardedTo3rdParty, eventId, param);
        }

        internal void LogEventException(string wrapperName,
                                        string eventName,
                                        string eventId,
                                        [CanBeNull] Dictionary<string, object> param,
                                        Exception e)
        {
            LogEventLocallyIfRelevant(wrapperName, eventName, DebugAnalyticsStateEnum.Error, eventId, param, e.ToString());
        }

        internal void LogEventsSentSuccessfully(List<string> eventJsons)
        {
            LogAnalyticsSentOrErrorEvent(VOODOO_ANALYTICS_WRAPPER_NAME, eventJsons, DebugAnalyticsStateEnum.Sent);
        }

        internal void LogEventsSentError(List<string> eventJsons, string error)
        {
            LogAnalyticsSentOrErrorEvent(VOODOO_ANALYTICS_WRAPPER_NAME, eventJsons, DebugAnalyticsStateEnum.SentButErrorFromServer, error);
        }

        private void LogAnalyticsSentOrErrorEvent(string wrapperName, List<string> eventJsons,
            DebugAnalyticsStateEnum stateEnum, string error = "")
        {
            foreach (string eventJson in eventJsons)
            {
                if(string.IsNullOrEmpty(eventJson))
                    continue;
                
                Dictionary<string, object> param;
                try
                {
                    param = JsonUtils.DeserializeAsDictionary(eventJson);
                }
                catch (Exception e)
                {
                    VoodooLog.LogError(Module.ANALYTICS, TAG, e.Message);
                    if (!(e is OverflowException || e is InvalidCastException || e is ArgumentOutOfRangeException ||
                          e is ArrayTypeMismatchException))
                    {
                        VoodooSauce.LogCrashReportingMessage(eventJson);
                        VoodooSauce.LogException(e);
                    }

                    continue;
                }

                if (param != null)
                {
                    var eventName = TryToGetStringOrEmpty(AnalyticsEventLoggerConstant.EVENT_NAME, param);
                    var eventId = TryToGetStringOrEmpty(AnalyticsEventLoggerConstant.EVENT_ID, param);
                    LogEventLocallyIfRelevant(wrapperName, eventName, stateEnum, eventId, param, error);
                }
            }
        }

        private static string TryToGetStringOrEmpty<T>(T key, Dictionary<T, object> data)
        {
            if (!data.ContainsKey(key)) return "";
            
            var value = data[key];
            return value != null ? value.ToString() : "";
        }

        private static string GetAdditionalInformationFromJson(Dictionary<string, object> dict, string additionalInformation = "")
        {
            if (dict == null) return additionalInformation;
            Dictionary<string, object> data = null;
            if (dict.TryGetValue(VoodooAnalyticsConstants.DATA, out object dataJson)) {
                if (dataJson is string) {
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson.ToString());
                } else if (dataJson is Dictionary<string, object> dataJsonDict) {
                    data = dataJsonDict;
                }
            }

            if (dict.TryGetValue(AnalyticsConstant.SESSION_LENGTH, out object sessionLengthInSeconds)) {
                TimeSpan sessionLength = TimeSpan.FromSeconds(Convert.ToDouble(sessionLengthInSeconds));
                if (sessionLength.Minutes > 0 || sessionLength.Seconds > 0) {
                    additionalInformation = AnalyticsConstant.SESSION_LENGTH + ": ";
                    if (sessionLength.Minutes > 0) {
                        additionalInformation += (sessionLength.Minutes + "min").BoldText();
                        if (sessionLength.Seconds > 0) additionalInformation += " ";
                    }

                    if (sessionLength.Seconds > 0) additionalInformation += (sessionLength.Seconds + "s").BoldText();
                }
            }

            if (dict.TryGetValue(VoodooAnalyticsConstants.GAME_COUNT, out object gameCount)) {
                if (additionalInformation != "") additionalInformation += ", ";
                additionalInformation += VoodooAnalyticsConstants.GAME_COUNT + ": " + gameCount.ToString().BoldText();
            }

            if (data != null) additionalInformation = GetAdditionalInformationFromJson(data, additionalInformation);
            return additionalInformation;
        }

        internal void SetAnalyticsDebugging(bool enabled)
        {
            _isAnalyticsDebuggingEnabled = enabled;
        }

        internal void SetAnalyticsEventRecording(bool enabled)
        {
            if (enabled != IsRecordingEvents)
            {
                IsRecordingEvents = enabled;
                OnRecordingStateChange?.Invoke(IsRecordingEvents);
            }
        }

        internal void FlushAnalyticsLogs()
        {
            _logsIdList.Clear();
            _logsList.Clear();
        }
    }
}
