using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Firebase;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    public static class FirebaseAnalyticsWrapper
    {
        private const string TAG = "FirebaseAnalyticsWrapper";

        private static bool _isFirebaseInitialized;
        private static bool _isEnabled = true;
        private static readonly Queue<FirebaseAnalyticsEvent> EventsQueue = new Queue<FirebaseAnalyticsEvent>();

        internal static void SubscribeToFirebaseInitialization(PrivacyCore.GdprConsent consent, Action<bool> callback)
        {
            //Firebase have an issue with accessing any static variable that are located in FirebaseAnalytics class during the initialization
            FirebaseInitializer.Subscribe(initSuccess => {
                _isFirebaseInitialized = initSuccess;
                _isEnabled = initSuccess && consent.ExplicitConsentGivenForAnalytics;
                SetAnalyticsCollection(consent.ExplicitConsentGivenForAnalytics);
                callback?.Invoke(_isEnabled);
                if (_isEnabled) {
                    while (EventsQueue.Count > 0) {
                        EventsQueue.Dequeue().Track();
                    }
                } else {
                    EventsQueue.Clear();
                }
            });
        }

        internal static void TrackEvent(string eventName, Dictionary<string, object> properties = null)
        {
            if (!_isEnabled) return;

            var customEvent = new FirebaseAnalyticsEvent(eventName, properties);
            if (!_isFirebaseInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "FirebaseAnalytics NOT initialized queuing event..." + eventName);
                EventsQueue.Enqueue(customEvent);
            } else {
                var propertiesString = properties == null
                    ? "(empty)"
                    : string.Join("\n", properties.Select(property => $"{property.Key} = {property.Value}"));
                VoodooLog.LogDebug(Module.ANALYTICS, TAG,
                    $"Sending event {eventName} to FirebaseAnalytics with properties: {propertiesString}");
                customEvent.Track();
            }
        }

        internal static void TrackPurchase(VoodooIAPAnalyticsInfo purchaseInfo)
        {
            if (!_isEnabled) return;

            var newProperties = new Dictionary<string, object> {
                [FirebaseAnalyticsConstants.PARAMETER_ITEM_ID] = purchaseInfo.productId,
                [FirebaseAnalyticsConstants.PARAMETER_CURRENCY] = purchaseInfo.currency,
                [FirebaseAnalyticsConstants.PARAMETER_PRICE] = Mathf.RoundToInt((float) purchaseInfo.price * 100f)
            };

            var customEvent = new FirebaseAnalyticsEvent(FirebaseAnalyticsConstants.EVENT_ECOMMERCE_PURCHASE, newProperties);
            if (!_isFirebaseInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG,
                    "FirebaseAnalytics NOT initialized queuing event..." + customEvent.GetEventName());
                EventsQueue.Enqueue(customEvent);
            } else {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending  purchase event {customEvent.GetEventName()} to FirebaseAnalytics");
                customEvent.Track();
            }
        }

        private static void SetAnalyticsCollection(bool consent)
        {
            if (!_isFirebaseInitialized) return;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(consent);
        }
    }
}