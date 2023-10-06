using System.Collections.Generic;
using com.adjust.sdk;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    internal class AdjustAnalyticsEvent: BaseAnalyticsEvent
    {
        [CanBeNull]
        private Dictionary<string, object> _parameter;
        
        protected override string GetAnalyticsProviderName() => "Adjust";
        
        public AdjustAnalyticsEvent(string eventName, [CanBeNull] Dictionary<string, object> parameter) : base(eventName) => _parameter = parameter;

        protected override void PerformTrackEvent()
        {
            var trackEvent = new AdjustEvent(EventName);

            if (_parameter != null) {
                foreach (KeyValuePair<string, object> item in _parameter) {
                    trackEvent.addCallbackParameter(item.Key, item.Value.ToString());
                    trackEvent.addPartnerParameter(item.Key, item.Value.ToString());
                }
            }
            
            Adjust.trackEvent(trackEvent);
        }
    }
}