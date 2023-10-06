using System;
using System.Linq;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal class ConversionEventsManager
    {
        private DateTime? _expirationDateTime;
        private ConversionEventsSettings _eventsSettings;

        private static ConversionEventsManager instance;

        internal static ConversionEventsManager Instance => instance ?? (instance = new ConversionEventsManager());

        internal void Initialize(ConversionEventsSettings settings)
        {
            Initialize(settings, AnalyticsStorageHelper.Instance.GetFirstAppLaunchDate());
        }

        internal void Initialize(ConversionEventsSettings eventsSettings, DateTime? installDate)
        {
            if (eventsSettings != null && eventsSettings.IsEnabled() && eventsSettings.HasValidExpirationDate()) {
                _eventsSettings = eventsSettings;
                _expirationDateTime = installDate?.Date.AddDays(eventsSettings.DaysUntilExpiration);
            } else {
                _eventsSettings = null;
                _expirationDateTime = null;
            }
        }

        internal ConversionEventInfo GetInterstitialConversionEvent(AdClosedEventAnalyticsInfo adEvent) =>
            GetConversionAdEvent(adEvent, ConversionEventName.FS);

        internal ConversionEventInfo GetRewardedVideoConversionEvent(AdClosedEventAnalyticsInfo adEvent) =>
            GetConversionAdEvent(adEvent, ConversionEventName.RV);

        private ConversionEventInfo GetConversionAdEvent(AdClosedEventAnalyticsInfo adEvent, ConversionEventName conversionEventName)
        {
            return IsEventsNotExpired()
                ? _eventsSettings?.AdEvents?.FirstOrDefault(eventInfo =>
                    eventInfo.Name == conversionEventName && eventInfo.TargetNumberAdsWatched == adEvent.AdWatchedCount)
                : null;
        }

        private bool IsEventsNotExpired() => _eventsSettings != null && _expirationDateTime != null && DateTime.UtcNow < _expirationDateTime;
    }
}