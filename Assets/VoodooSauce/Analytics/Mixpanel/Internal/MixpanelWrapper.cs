using System;
using System.Collections.Generic;
using mixpanel;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.GDPR;
using Voodoo.Sauce.Privacy;
using Object = UnityEngine.Object;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal static class MixpanelWrapper
    {
        public const string TAG = "MixpanelWrapper";
        private static bool _isInitialized;
        private static bool _isDisabled;
        private static readonly Queue<MixpanelEvent> QueuedEvents = new Queue<MixpanelEvent>();

        public static void Initialize(string mixpanelToken, PrivacyCore.GdprConsent consent)
        {
            Instantiate(mixpanelToken);
            _isInitialized = true;
            SetConsent(consent.ExplicitConsentGivenForAnalytics);
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Mixpanel initialized, tracking pending events: " + QueuedEvents.Count);
            while (QueuedEvents.Count > 0) {
                QueuedEvents.Dequeue().Track();
            }
        }

        public static void TrackEvent(string eventName, Dictionary<string, object> eventProperties = null)
        {
            if (_isDisabled) return;
            Value properties = GetValuesFromProperties(eventProperties);
            properties["InternetReachability"] = Application.internetReachability.ToString();
            var mixpanelEvent = new CustomMixpanelEvent(eventName, properties);
            if (!_isInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Mixpanel NOT initialized queuing event..." + eventName);
                QueuedEvents.Enqueue(mixpanelEvent);
            } else {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Sending event " + eventName + " to Mixpanel");
                mixpanelEvent.Track();
            }
        }

        public static void TrackTimedEvent(string eventName)
        {
            if (_isDisabled) return;
            var mixpanelEvent = new CustomMixpanelTimedEvent(eventName);
            if (!_isInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Mixpanel NOT initialized queuing event..." + eventName);
                QueuedEvents.Enqueue(mixpanelEvent);
            } else {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending timed event {eventName} to Mixpanel");
                mixpanelEvent.Track();
            }
        }

        private static void SetConsent(bool consent)
        {
            if (_isDisabled || !_isInitialized) return;
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending  consent {consent} to Mixpanel");
            if (consent) {
                Mixpanel.People.Set(MixpanelConstants.IDFA, VoodooSauceCore.GetPrivacy().GetAdvertisingId());
                Mixpanel.People.Set(MixpanelConstants.DeviceUniqueIdentifier, SystemInfo.deviceUniqueIdentifier); 

                Mixpanel.OptInTracking();
            } else {
                Mixpanel.OptOutTracking();
            }
        }

        private static void Instantiate(string mixpanelToken)
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Initializing Mixpanel");
            Mixpanel.Init();
            Mixpanel.SetToken(mixpanelToken);
            if (AnalyticsStorageHelper.Instance.IsFirstAppLaunch() || AbTestManager.IsDebugModeForced()) {
                RegisterMixpanelSuperProperties();
            }
        }

        public static void Disable()
        {
            _isDisabled = true;
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Disabling Mixpanel");
            QueuedEvents.Clear();
        }

        private static void RegisterMixpanelSuperProperties()
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Registering Mixpanel Super Properties");
            Mixpanel.Register(MixpanelConstants.AbTestPropertyName, AbTestManager.GetPlayerCohort());
            Mixpanel.Register(MixpanelConstants.FirstAppVersionPropertyName, Application.version);
            Mixpanel.Register(MixpanelConstants.VoodooSaucePropertyName, VoodooSauce.Version());
            Mixpanel.Register(MixpanelConstants.OSTypePropertyName, GetPlatform());
            Mixpanel.Register(MixpanelConstants.GameBundleId, Application.identifier);
            Mixpanel.Register(MixpanelConstants.CreationDatePropertyName, DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss"));
        }

        private static Value GetValuesFromProperties(Dictionary<string, object> eventProperties)
        {
            var properties = new Value();
            if (eventProperties != null) {
                foreach (var key in eventProperties.Keys) {
                    if (eventProperties[key] is int)
                        properties[key] = (int) eventProperties[key];
                    else if (eventProperties[key] is string)
                        properties[key] = (string) eventProperties[key];
                    else if (eventProperties[key] is float)
                        properties[key] = (float) eventProperties[key];
                    else if (eventProperties[key] is bool)
                        properties[key] = (bool) eventProperties[key];
                    else
                        VoodooLog.LogWarning(Module.ANALYTICS, TAG, "Ignoring unsupported event property type for key \"" + key + "\"!");
                }
            }

            return properties;
        }

        private static string GetPlatform()
        {
#if UNITY_EDITOR
            return "Editor";
#elif UNITY_IOS || UNITY_IPHONE
            return "iOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "Unknown";
#endif
        }
    }
}
