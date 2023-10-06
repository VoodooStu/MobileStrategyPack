using System.Collections.Generic;
using GameAnalyticsSDK;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    internal abstract class GameAnalyticsEvent : BaseAnalyticsEvent
    {
        protected override string GetAnalyticsProviderName() => GameAnalyticsWrapper.TAG;
        protected GameAnalyticsEvent(string eventName) : base(eventName) { }
    }

    internal class ProgressEvent : GameAnalyticsEvent
    {
        private readonly GAProgressionStatus _status;
        private readonly string _progress;
        private readonly int? _score;
        private readonly Dictionary<string, object> _customFields;

        protected override void PerformTrackEvent()
        {
            if (_score != null) {
                if (_customFields != null) {
                    GameAnalytics.NewProgressionEvent(_status, _progress, (int)_score, _customFields);
                } else {
                    GameAnalytics.NewProgressionEvent(_status, _progress, (int)_score);
                }
            } else {
                if (_customFields != null) {
                    GameAnalytics.NewProgressionEvent(_status, _progress, _customFields);
                } else {
                    GameAnalytics.NewProgressionEvent(_status, _progress);
                }
            }
        }

        public ProgressEvent(GAProgressionStatus status, string progress, int? score, Dictionary<string, object> customFields) : base(status.ToString())
        {
            _status = status;
            _progress = string.IsNullOrEmpty(progress) == false ? progress : "0";
            _score = score;
            _customFields = customFields;
        }
    }

    internal class DesignEvent : GameAnalyticsEvent
    {
        private readonly Dictionary<string, object> _customFields;
        
        protected override void PerformTrackEvent()
        {
            if (_customFields != null) {
                GameAnalytics.NewDesignEvent(EventName, _customFields);
            } else {
                GameAnalytics.NewDesignEvent(EventName);
            }
        }

        public DesignEvent(string eventName, Dictionary<string, object> customFields) : base(eventName) => _customFields = customFields;
    }
}