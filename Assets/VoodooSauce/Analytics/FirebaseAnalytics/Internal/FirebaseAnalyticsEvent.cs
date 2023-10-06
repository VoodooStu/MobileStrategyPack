using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal class FirebaseAnalyticsEvent : BaseAnalyticsEvent
        {
            private Dictionary<string, object> _properties;
            public string GetEventName() => EventName ?? "";
            protected override string GetAnalyticsProviderName() => "FirebaseAnalytics";

            public FirebaseAnalyticsEvent(string eventName, Dictionary<string, object> properties) : base(eventName)
            {
                _properties = properties;
            }

            protected override void PerformTrackEvent()
            {
                UpdateEventNameToFirebaseSpecific();
                UpdateParamKeysToFirebaseSpecific();
                var parameters = ToParameters();
                
                if (_properties != null && parameters.Count > 0) {
                    FirebaseAnalytics.LogEvent(EventName, parameters.ToArray());
                } else {
                    FirebaseAnalytics.LogEvent(EventName);
                }
            }

            private void UpdateEventNameToFirebaseSpecific()
            {
                var firebaseKey = GetFirebaseKey(EventName);
                if (!string.IsNullOrEmpty(firebaseKey)) EventName = firebaseKey;
            }

            private void UpdateParamKeysToFirebaseSpecific()
            {
                if (_properties == null) return;
                var temporaryDictionary = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> pair in _properties) {
                    temporaryDictionary[GetFirebaseKey(pair.Key)] = pair.Value;
                }
                _properties = temporaryDictionary;
            }
            
            private List<Parameter> ToParameters()
            {
                var parameters = new List<Parameter>();
                if (_properties == null) return parameters;
                foreach (KeyValuePair<string, object> pair in _properties) {
                    if (pair.Value is double) {
                        parameters.Add(new Parameter(pair.Key, (double)pair.Value));
                    }
                    else if (pair.Value is long) {
                        parameters.Add(new Parameter(pair.Key, (long)pair.Value));
                    } else {
                        parameters.Add(new Parameter(pair.Key, pair.Value.ToString()));
                    }
                }

                return parameters;
            }
            
            
            //Maintain mapping between local temporary constant that are maintained in FirebaseAnalyticsConstant
            //and Firebase's constant
            private static string GetFirebaseKey(string key)
            {
                switch (key) {
                    case FirebaseAnalyticsConstants.EVENT_ECOMMERCE_PURCHASE:
                        return FirebaseAnalytics.EventPurchase;
                    case FirebaseAnalyticsConstants.EVENT_LEVEL_END:
                        return FirebaseAnalytics.EventLevelEnd;
                    case FirebaseAnalyticsConstants.EVENT_LEVEL_START:
                        return FirebaseAnalytics.EventLevelStart;
                    case FirebaseAnalyticsConstants.EVENT_LEVEL_UP:
                        return FirebaseAnalytics.EventLevelUp;
                    case FirebaseAnalyticsConstants.PARAMETER_CURRENCY:
                        return FirebaseAnalytics.ParameterCurrency;
                    case FirebaseAnalyticsConstants.PARAMETER_LEVEL:
                        return FirebaseAnalytics.ParameterLevel;
                    case FirebaseAnalyticsConstants.PARAMETER_PRICE:
                        return FirebaseAnalytics.ParameterPrice;
                    case FirebaseAnalyticsConstants.PARAMETER_SCORE:
                        return FirebaseAnalytics.ParameterScore;
                    case FirebaseAnalyticsConstants.PARAMETER_VALUE:
                        return FirebaseAnalytics.ParameterValue;
                    case FirebaseAnalyticsConstants.PARAMETER_ITEM_ID:
                        return FirebaseAnalytics.ParameterItemId;
                    default:
                        return key;
                }
            }
        }
}