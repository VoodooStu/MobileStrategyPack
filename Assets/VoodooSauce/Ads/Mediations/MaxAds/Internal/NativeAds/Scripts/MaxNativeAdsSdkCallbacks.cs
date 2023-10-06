using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Ads
{
    public class MaxNativeAdsSdkCallbacks : MonoBehaviour
    {
        private const string TAG = "MaxNativeAdsSdkCallbacks";
        public static MaxNativeAdsSdkCallbacks Instance { get; private set; }

        private static Action<string, MaxSdkBase.AdInfo> _onNativeAdLoadedEvent;
        private static Action<string, MaxSdkBase.ErrorInfo> _onNativeAdAdLoadFailedEvent;
        private static Action<string, MaxSdkBase.AdInfo> _onNativeAdDisplayedEvent;
        private static Action<string, MaxSdkBase.ErrorInfo> _onNativeAdFailedToDisplayEvent;
        private static Action<string, MaxSdkBase.AdInfo> _onNativeAdClickedEvent;
        private static Action<string, MaxSdkBase.AdInfo> _onNativeAdRevenuePaidEvent;
        private static Action<string, MaxSdkBase.AdInfo> _onNativeAdHiddenEvent;

        public static event Action<string, MaxSdkBase.AdInfo> OnAdLoadedEvent {
            add => _onNativeAdLoadedEvent += value;
            remove => _onNativeAdLoadedEvent -= value;
        }

        public static event Action<string, MaxSdkBase.ErrorInfo> OnAdLoadFailedEvent {
            add => _onNativeAdAdLoadFailedEvent += value;
            remove => _onNativeAdAdLoadFailedEvent -= value;
        }

        public static event Action<string, MaxSdkBase.AdInfo> OnAdDisplayedEvent {
            add => _onNativeAdDisplayedEvent += value;
            remove => _onNativeAdDisplayedEvent -= value;
        }
        
        public static event Action<string, MaxSdkBase.ErrorInfo> OnAdFailedToDisplayEvent {
            add => _onNativeAdFailedToDisplayEvent += value;
            remove => _onNativeAdFailedToDisplayEvent -= value;
        }

        public static event Action<string, MaxSdkBase.AdInfo> OnAdClickedEvent {
            add => _onNativeAdClickedEvent += value;
            remove => _onNativeAdClickedEvent -= value;
        }

        public static event Action<string, MaxSdkBase.AdInfo> OnAdRevenuePaidEvent {
            add => _onNativeAdRevenuePaidEvent += value;
            remove => _onNativeAdRevenuePaidEvent -= value;
        }

        public static event Action<string, MaxSdkBase.AdInfo> OnAdHiddenEvent {
            add => _onNativeAdHiddenEvent += value;
            remove => _onNativeAdHiddenEvent -= value;
        }

        public static void ForwardEvent(string eventPropsStr)
        {
            if (!(JsonConvert.DeserializeObject(eventPropsStr) is Dictionary<string, object> eventProps)) {
                VoodooLog.LogError(Module.ADS, TAG, "Failed to forward event for serialized event data: " + eventPropsStr);
                return;
            }

            string eventName = MaxNativeAdsSdkUtils.GetStringFromDictionary(eventProps, "name");
            var adInfo = new MaxSdkBase.AdInfo(eventProps);
            string adUnitIdentifier = MaxNativeAdsSdkUtils.GetStringFromDictionary(eventProps, "adUnitId");

            switch (eventName) {
                case "onNativeAdLoaded":
                    InvokeEvent(_onNativeAdLoadedEvent, adUnitIdentifier, adInfo);
                    break;
                case "onNativeAdLoadFailed": {
                    var errorInfo = new MaxSdkBase.ErrorInfo(eventProps);
                    InvokeEvent(_onNativeAdAdLoadFailedEvent, adUnitIdentifier, errorInfo);
                    break;
                }
                case "onNativeAdShown":
                    InvokeEvent(_onNativeAdDisplayedEvent, adUnitIdentifier, adInfo);
                    break;
                case "onNativeAdShowFailed": {
                    var errorInfo = new MaxSdkBase.ErrorInfo(eventProps);
                    InvokeEvent(_onNativeAdFailedToDisplayEvent, adUnitIdentifier, errorInfo);
                    break;
                }
                case "onNativeAdClicked":
                    InvokeEvent(_onNativeAdClickedEvent, adUnitIdentifier, adInfo);
                    break;
                case "onNativeAdRevenuePaid":
                    InvokeEvent(_onNativeAdRevenuePaidEvent, adUnitIdentifier, adInfo);
                    break;
                case "onNativeAdHidden":
                    InvokeEvent(_onNativeAdHiddenEvent, adUnitIdentifier, adInfo);
                    break;
                default:
                    VoodooLog.LogWarning(Module.ADS, TAG, "Unknown MAX Native Ads event fired: " + eventName);
                    break;
            }
        }
        

        private static void InvokeEvent<T1, T2>(Action<T1, T2> evt, T1 param1, T2 param2)
        {
            if (!CanInvokeEvent(evt)) return;
            evt(param1, param2);
        }
        
        private static bool CanInvokeEvent(Delegate evt)
        {
            if (evt == null) return false;

            // Check that publisher is not over-subscribing
            if (evt.GetInvocationList().Length > 5) {
                VoodooLog.LogWarning(Module.ADS, TAG,
                    "Native Ads Event (" + evt + ") has over 5 subscribers. Please make sure you are properly un-subscribing to actions!!!");
            }

            return true;
        }

        void Awake()
        {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}