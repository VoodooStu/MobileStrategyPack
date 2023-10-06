using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Internal.Extension;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Serializable]
    public class ConversionEventsSettings
    {
        private const int EVENT_NAME_DATA_INDEX = 0;
        private const int EVENT_TRIGGER_DATA_INDEX = 1;
        private const int EVENT_TOKEN_DATA_INDEX = 2;
        private const char DATA_SEPARATOR = '/';

        public bool UseConversionEvents;
        public int DaysUntilExpiration;
        public ConversionAdEventInfo[] AdEvents;
        public ConversionEventInfo[] UnknownEvents;

        public bool IsEnabled() => UseConversionEvents && HasAnyEvents();
        public bool HasAnyEvents() => AdEvents?.Any() == true || UnknownEvents?.Any() == true;
        public bool HasKnownEvents() => AdEvents?.Any() == true;
        public bool HasValidExpirationDate() => DaysUntilExpiration > 0;

        public List<string> GetInvalidEventsData()
        {
            var invalidEvents = new List<string>();
            //Add unknown events
            if (UnknownEvents != null) {
                invalidEvents.AddRange(UnknownEvents.Select(eventInfo => eventInfo.OriginalData));
            }

            //Add invalid ad events
            if (AdEvents != null) {
                invalidEvents.AddRange(AdEvents.Where(eventInfo => !eventInfo.IsValid()).Select(eventInfo => eventInfo.OriginalData));
            }

            return invalidEvents;
        }
        
        public List<string> GetDuplicatedEventsData()
        {
            var duplicatedEvents = new List<string>();
            
            //Add duplicated ad events
            if (AdEvents != null) {
                duplicatedEvents.AddRange(AdEvents.GroupBy(eventInfo => eventInfo.OriginalData)
                                                  .Where(grouping => grouping.Count() > 1)
                                                  .Select(grouping => grouping.Key));
            }
            return duplicatedEvents;
        }

        public static ConversionEventsSettings Build(bool useConversionEvents, string daysUntilExpirationValue, IEnumerable<string> eventsValues)
        {
            var adEvents = new List<ConversionAdEventInfo>();
            var unknownEvents = new List<ConversionEventInfo>();
            foreach (string eventValue in eventsValues) {
                if (!string.IsNullOrWhiteSpace(eventValue)) {
                    ConversionEventInfo eventInfo = BuildEvent(eventValue.RemoveSpace());
                    if (eventInfo is ConversionAdEventInfo adEventInfo) {
                        adEvents.Add(adEventInfo);
                    } else {
                        unknownEvents.Add(eventInfo);
                    }
                }
            }

            int.TryParse(daysUntilExpirationValue?.RemoveSpace(), out int daysUntilExpiration);
            return new ConversionEventsSettings {
                UseConversionEvents = useConversionEvents,
                DaysUntilExpiration = daysUntilExpiration,
                AdEvents = adEvents.Any() ? adEvents.ToArray() : null,
                UnknownEvents = unknownEvents.Any() ? unknownEvents.ToArray() : null
            };
        }

        private static ConversionEventInfo BuildEvent(string eventValue)
        {
            //event information ( Event name, Trigger, Adjust Event Token) are set in the same field separated by ‘/’ ( example fs/5/obkq08 …) 
            try {
                string[] fields = eventValue.Split(DATA_SEPARATOR);
                ConversionEventName eventName = ParseEventName(fields[EVENT_NAME_DATA_INDEX]);
                string eventToken = fields[EVENT_TOKEN_DATA_INDEX];
                switch (eventName) {
                    case ConversionEventName.FS:
                    case ConversionEventName.RV:
                        int.TryParse(fields[EVENT_TRIGGER_DATA_INDEX], out int numberOfAds);
                        return new ConversionAdEventInfo {
                            OriginalData = eventValue, Name = eventName, AdjustEventToken = eventToken, TargetNumberAdsWatched = numberOfAds
                        };
                    case ConversionEventName.Unknown:
                        return new ConversionEventInfo {
                            OriginalData = eventValue, Name = eventName, AdjustEventToken = eventToken
                        };
                }
            } catch {
                // ignored
            }

            return new ConversionEventInfo {Name = ConversionEventName.Unknown, OriginalData = eventValue};
        }

        private static ConversionEventName ParseEventName(string value) =>
            Enum.TryParse(value, true, out ConversionEventName eventName) ? eventName : ConversionEventName.Unknown;
    }

    /// <summary>
    /// Data representation for conversion event
    /// </summary>
    [Serializable]
    public class ConversionEventInfo
    {
        public string AdjustEventToken;
        public ConversionEventName Name;
        [HideInInspector]
        public string OriginalData;

        public virtual bool IsValid() => !string.IsNullOrEmpty(AdjustEventToken) && Name != ConversionEventName.Unknown;
    }

    [Serializable]
    public class ConversionAdEventInfo : ConversionEventInfo
    {
        public int TargetNumberAdsWatched;

        public override bool IsValid() => base.IsValid() && TargetNumberAdsWatched > 0;
    }

    public enum ConversionEventName
    {
        FS,
        RV,
        Unknown
    }
}