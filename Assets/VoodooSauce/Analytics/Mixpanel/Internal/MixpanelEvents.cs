using mixpanel;

namespace Voodoo.Sauce.Internal.Analytics
{
    abstract class MixpanelEvent: BaseAnalyticsEvent
    {
        protected override string GetAnalyticsProviderName() => MixpanelWrapper.TAG;
        protected MixpanelEvent(string eventName) : base(eventName) { }
    }

    class CustomMixpanelEvent : MixpanelEvent
    {
        public Value Props;
        protected override void PerformTrackEvent()
        {
            if (Props != null) Mixpanel.Track(EventName, Props);
            else Mixpanel.Track(EventName);
        }

        public CustomMixpanelEvent(string eventName, Value props) : base(eventName) => Props = props;
    }

    class CustomMixpanelTimedEvent : MixpanelEvent
    {
        protected override void PerformTrackEvent()
        {
            Mixpanel.StartTimedEvent(EventName);
        }

        public CustomMixpanelTimedEvent(string eventName) : base(eventName) { }
    }
}