using System.Runtime.InteropServices;
using AOT;

namespace Voodoo.Sauce.Internal.Ads
{
    public class MaxNativeAdsSdkiOS
    {
        private delegate void ALUnityNativeAdsCallback(string args);
        static MaxNativeAdsSdkiOS()
        {
            MaxNativeAdsSdk.InitCallbacks();
        }

#if UNITY_IOS
        internal static void Initialize(string adUnitId)
        {
            if (adUnitId == null) return;
            _InitializeNativeAd(adUnitId, NativeAdsCallback);
        }

        public static void LoadAd() => _LoadNativeAd();
     
        internal static bool IsAdReadyToShow() => _IsNativeAdAvailable();
        
        internal static void ShowAd(string adPlacement, float x, float y, float width, float height) => _ShowNativeAd(adPlacement, x, y, width, height);
        
        internal static void HideAd() => _HideNativeAd();
        
        internal static void SetCustomData(string customData) => _SetCustomData(customData);
        
        [DllImport("__Internal")]
        private static extern void _InitializeNativeAd(string adUnitIdentifier, ALUnityNativeAdsCallback callback);
        
        [DllImport("__Internal")]
        private static extern void _LoadNativeAd();

        [DllImport("__Internal")]
        private static extern void _ShowNativeAd(string adFormat, float x, float y, float width, float height);

        [DllImport("__Internal")]
        private static extern void _HideNativeAd();

        [DllImport("__Internal")]
        private static extern bool _IsNativeAdAvailable();
        
        [DllImport("__Internal")]
        private static extern void _SetCustomData(string customData);


        [MonoPInvokeCallback(typeof(ALUnityNativeAdsCallback))]
        internal static void NativeAdsCallback(string propsStr)
        {
            MaxNativeAdsSdkCallbacks.ForwardEvent(propsStr);
        }
#endif

    }
}