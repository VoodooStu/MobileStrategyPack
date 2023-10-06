#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine.iOS;
#endif

namespace Voodoo.Sauce.Internal.CrossPromo.Mobile
{
    internal static class IOSCrossPromoWrapper
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _loadNativeStore(long appId);

        [DllImport("__Internal")]
        private static extern void _openNativeStore(long appI);

        private const string NATIVE_STORE_MIN_VERSION = "11.3";

        public static void LoadNativeStore(long appId)
        {
            _loadNativeStore(appId);
        }

        public static void OpenNativeStore(long appId)
        {
            _openNativeStore(appId);
        }

        public static bool CheckGoodIosVersion()
        {
            try {
                var currentIOSVersion = new Version(Device.systemVersion);
                var limitVersion = new Version(NATIVE_STORE_MIN_VERSION);
                return currentIOSVersion.CompareTo(limitVersion) >= 0;
            } catch (Exception) {
                return false;
            }
        }
#else
        public static void LoadNativeStore(long appId) { }

        public static void OpenNativeStore(long appId) { }

        public static bool CheckGoodIosVersion() => true;
#endif
    }
}