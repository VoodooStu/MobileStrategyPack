using System.Collections.Generic;
using JetBrains.Annotations;
using Voodoo.Analytics;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics {
    internal class VoodooAnalyticsLoggerEvent : BaseAnalyticsEvent
    {
        private VanEventName? _eventName;
        [CanBeNull] private readonly Dictionary<string, object> _data;
        [CanBeNull] private readonly string _dataAsString;
        [CanBeNull] private readonly string _eventType;
        [CanBeNull] private readonly Dictionary<string, object> _customVariables;
        [CanBeNull] private readonly Dictionary<string, object> _contextVariables;
        protected override string GetAnalyticsProviderName() => "VoodooAnalytics";

        public VoodooAnalyticsLoggerEvent(VanEventName eventName, 
                                          [CanBeNull] Dictionary<string, object> data, 
                                          [CanBeNull] string eventType, 
                                          [CanBeNull] Dictionary<string, object> customVariables, 
                                          [CanBeNull] Dictionary<string, object> contextVariables) : base(eventName.ToString(), data)
        {
            _eventName = eventName;
            _data = data;
            _eventType = eventType;
            _customVariables = customVariables;
            _contextVariables = contextVariables;
        }
        
        public VoodooAnalyticsLoggerEvent(string eventName, 
                                          [CanBeNull] Dictionary<string, object> data, 
                                          [CanBeNull] string eventType, 
                                          [CanBeNull] Dictionary<string, object> contextVariables) : base(eventName)
        {
            _data = data;
            _eventType = eventType;
            _contextVariables = contextVariables;
        }

        public VoodooAnalyticsLoggerEvent(VanEventName eventName,
                                          [CanBeNull] string dataAsString,
                                          [CanBeNull] string eventType,
                                          [CanBeNull] Dictionary<string, object> customVariables,
                                          [CanBeNull] Dictionary<string, object> contextVariables) : base(eventName.ToString())
        {
            _eventName = eventName;
            _dataAsString = dataAsString;
            _eventType = eventType;
            _customVariables = customVariables;
            _contextVariables = contextVariables;
        }
        
        protected override void PerformTrackEvent()
        {
            if (_eventName != null) {
                if (_data != null) {
                    VoodooAnalyticsManager.TrackEvent(_eventName.Value, _data, _eventType, _customVariables, EventId, _contextVariables);
                } else {
                    VoodooAnalyticsManager.TrackEvent(_eventName.Value, _dataAsString, _eventType, _customVariables, EventId, _contextVariables);
                }
            }
            else {
                VoodooAnalyticsManager.TrackCustomEvent(EventName, _data, _eventType, EventId, _contextVariables);
            }
        }
    }
}