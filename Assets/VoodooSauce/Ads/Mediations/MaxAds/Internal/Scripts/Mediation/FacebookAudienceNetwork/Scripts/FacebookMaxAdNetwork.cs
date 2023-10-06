using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;

// ReSharper disable once CheckNamespace
namespace  Voodoo.Sauce.Internal.Ads
{
    public static class FacebookMaxAdNetwork
    {
        private const string TAG = "FacebookMaxAdNetwork";

        public static void SetConsent(bool consent)
        {
            if (PlatformUtils.UNITY_IOS) {
                _setFANConsent(consent);
            }
        }
        
        public static void SetDoNotSell(bool doNotSell)
        {
            if (PlatformUtils.UNITY_IOS) {
                _setCCPADataProcessing(doNotSell);
            } else if (PlatformUtils.UNITY_ANDROID) {
                var adSettings = new AndroidJavaClass("com.facebook.ads.AdSettings");
                if (doNotSell) {
                    VoodooLog.LogDebug(Module.ADS, TAG, "Enable FAN CCPA Data Processing");
                    adSettings.CallStatic("setDataProcessingOptions", new[]{"LDU"}, 1, 1000);
                } else {
                    VoodooLog.LogDebug(Module.ADS, TAG, "Disable FAN CCPA Data Processing");
                    adSettings.CallStatic("setDataProcessingOptions", new string[]{}, 0, 0);
                }
            }
        }
        
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _setFANConsent(bool userConsent);
        
        [DllImport("__Internal")]
        private static extern void _setCCPADataProcessing(bool enable);
#else
        private static void _setFANConsent(bool userConsent) { }
        private static void _setCCPADataProcessing(bool enable) { }
#endif
    }
}