using System;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Voodoo.Analytics
{
    public static class VoodooAnalyticsManager
    {
        private const string TAG = "VoodooAnalyticsManager";

        private static bool _isInitialized;

        private static readonly List<QueuedEvent> QueuedEvents = new List<QueuedEvent>();

        public static readonly GlobalContext GlobalContext = new GlobalContext();

        internal static VanEventBasicParameters Parameters;

        private static Action _newEvent;
        private static Func<VanSessionInfo> _sessionInfoProvider;

        /// <summary>
        ///   <param>Init Analytics Manager.</param>
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="proxyServer">Server proxy</param>
        /// <param name="gatewayUrl">URL of the analytics gateway</param>
        /// <param name="parameters">VAN parameters</param>
        /// <param name="newEvent">Action called at every new event sent</param>
        public static void Init(IConfig config,
                                string proxyServer,
                                string gatewayUrl,
                                VanEventBasicParameters parameters,
                                [CanBeNull] Action newEvent = null)
        {
            if (_isInitialized) {
                return;
            }

            _isInitialized = true;

            Parameters = parameters;
            _newEvent = newEvent;

            Tracker.Initialise(config, proxyServer, gatewayUrl);

            SendQueuedEvents();
        }

        public static void SetSessionInfoProvider(Func<VanSessionInfo> sessionInfoProvider)
        {
            _sessionInfoProvider = sessionInfoProvider;
        }

        public static void Reset()
        {
            _isInitialized = false;
        }

        public static void StartTracker()
        {
            VoodooSauce.SubscribeOnInitFinishedEvent(initResult => Tracker.Start());
        }

        public static void StopTracker()
        {
            Tracker.Stop();
        }

        private static void SendQueuedEvents()
        {
            QueuedEvents.ForEach(queuedEvent => {
                InternalTrackEvent(queuedEvent.EventName,
                    queuedEvent.EventDataJson,
                    queuedEvent.EventType,
                    queuedEvent.EventCustomVariablesJson,
                    queuedEvent.EventContextVariablesJson,
                    queuedEvent.EventId,
                    queuedEvent.SessionInfo);
            });

            QueuedEvents.Clear();
        }

        /// <summary>
        ///   <param>Track an event (list in VanEventName)</param>
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="data">Internal data</param>
        /// <param name="eventType">event type</param>
        /// <param name="customVariables">External data (set in game)</param>
        /// <param name="contextVariables">External data (set in game)</param>
        /// <param name="eventId">event identifier</param>
        public static void TrackEvent(VanEventName eventName,
                                      Dictionary<string, object> data,
                                      string eventType = null,
                                      Dictionary<string, object> customVariables = null,
                                      string eventId = null,
                                      Dictionary<string, object> contextVariables = null)
        {
            TrackEvent(eventName.ToString(), data, eventType, customVariables, contextVariables, eventId);
        }

        /// <summary>
        ///   <param>Track custom  event (not list in VanEventName)</param>
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="eventType">event type</param>
        /// <param name="eventId">event identifier</param>
        /// <param name="customVariables">External data (set in game)</param>
        /// <param name="contextVariables">External data (set in game)</param>
        public static void TrackCustomEvent(string eventName,
                                            Dictionary<string, object> customVariables,
                                            string eventType = null,
                                            string eventId = null,
                                            Dictionary<string, object> contextVariables = null)
        {
            TrackEvent(eventName, null, eventType, customVariables, contextVariables, eventId);
        }

        /// <summary>
        ///   <param>Track an event</param>
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="data">Internal data</param>
        /// <param name="eventType">event type</param>
        /// <param name="customVariables">External data (set in game)</param>
        /// <param name="contextVariables">External data (set in game)</param>
        /// <param name="eventId">event identifier</param>
        private static void TrackEvent(string eventName,
                                       Dictionary<string, object> data,
                                       string eventType = null,
                                       Dictionary<string, object> customVariables = null,
                                       Dictionary<string, object> contextVariables = null,
                                       string eventId = null)
        {
            string dataJson = null;
            if (data != null) {
                dataJson = AnalyticsUtil.ConvertDictionaryToJson(data);
            }

            string customVariablesJson = null;
            if (customVariables != null) {
                customVariablesJson = AnalyticsUtil.ConvertDictionaryToCustomVarJson(customVariables);
            }

            if (contextVariables == null) {
                contextVariables = customVariables;
            } else if (customVariables != null) {
                foreach (var keyValue in customVariables) {
                    if (!contextVariables.ContainsKey(keyValue.Key))
                        contextVariables.Add(keyValue.Key, keyValue.Value);
                }
            }

            string contextVariablesJson = CreateContextVariablesJson(contextVariables);

            _newEvent?.Invoke();
            InternalTrackEvent(eventName, dataJson, eventType, customVariablesJson, contextVariablesJson, eventId);
        }

        /// <summary>
        ///   <param>Track an event (in VanEventName list)</param>
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="data">Internal data</param>
        /// <param name="eventType">event type</param>
        /// <param name="customVariables">External data (set in game)</param>
        /// <param name="contextVariables">External data (set in game)</param>
        /// <param name="eventId">event identifier</param>
        public static void TrackEvent(VanEventName eventName,
                                      string data = null,
                                      string eventType = null,
                                      Dictionary<string, object> customVariables = null,
                                      string eventId = null,
                                      Dictionary<string, object> contextVariables = null)
        {
            string customVariablesJson = null;
            if (customVariables != null) {
                customVariablesJson = AnalyticsUtil.ConvertDictionaryToCustomVarJson(customVariables);
            }

            string contextVariablesJson = CreateContextVariablesJson(contextVariables);

            _newEvent?.Invoke();
            InternalTrackEvent(eventName.ToString(), data, eventType, customVariablesJson, contextVariablesJson, eventId);
        }

        /// <summary>
        ///   <param>Set a custom exceptions logger </param>
        /// </summary>
        /// <param name="vanCustomLogger">instance of IVanCustomLog</param>
        public static void SetCustomLogger(IVanCustomLog vanCustomLogger)
        {
            AnalyticsLog.SetCustomLogger(vanCustomLogger);
        }

        private static void InternalTrackEvent(string eventName,
                                               string dataJson,
                                               string eventType,
                                               string customVariablesJson,
                                               string contextVariablesJson,
                                               [CanBeNull] string eventId,
                                               VanSessionInfo sessionInfo = null)
        {
            if (!_isInitialized) {
                var queuedEvent = new QueuedEvent {
                    EventName = eventName,
                    EventDataJson = dataJson,
                    EventType = eventType,
                    EventCustomVariablesJson = customVariablesJson,
                    EventContextVariablesJson = contextVariablesJson,
                    EventId = eventId,
                    SessionInfo = sessionInfo ?? _sessionInfoProvider()
                };
                AnalyticsLog.Log(TAG, "Add event " + eventName + " to the queue (" + dataJson + ")");
                QueuedEvents.Add(queuedEvent);
                return;
            }

            AnalyticsLog.Log(TAG, "Create event " + eventName + " (" + dataJson + ")");
            VanEvent.Create(eventName,
                Parameters,
                dataJson,
                customVariablesJson,
                contextVariablesJson,
                eventType,
                async e => { await Tracker.TrackEvent(e); },
                eventId,
                sessionInfo ?? _sessionInfoProvider());
        }

        private static string CreateContextVariablesJson(Dictionary<string, object> contextVariables)
        {
            string contextVariablesJson = null;
            FillGlobalContextVariables(ref contextVariables);
            if (contextVariables != null) {
                contextVariablesJson = AnalyticsUtil.ConvertDictionaryToContextVarJson(contextVariables);
            }

            return contextVariablesJson;
        }

        private static void FillGlobalContextVariables(ref Dictionary<string, object> contextVariables)
        {
            var parameters = GlobalContext.GetParameters();
            if (parameters.Count == 0) {
                return;
            }

            if (contextVariables == null) {
                contextVariables = new Dictionary<string, object>();
            }

            foreach (KeyValuePair<string, string> pair in parameters) {
                if (!contextVariables.ContainsKey(pair.Key)) {
                    contextVariables.Add(pair.Key, pair.Value);
                }
            }
        }

        private struct QueuedEvent
        {
            public string EventName;
            public string EventDataJson;
            public string EventType;
            public string EventCustomVariablesJson;
            public string EventContextVariablesJson;
            [CanBeNull]
            public string EventId;
            public VanSessionInfo SessionInfo;
        }
    }
}