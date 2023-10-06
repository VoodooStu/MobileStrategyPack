using UnityEngine;

namespace Voodoo.Sauce.Internal.Ads
{
    public class MaxNativeAdsSdkAndroid
    {
        private static readonly AndroidJavaClass
            MaxNativeAdsPluginClass = new AndroidJavaClass("io.voodoo.nativeads.unityplugin.MaxNativeAdsUnityPlugin");

        private static readonly NativeAdsCallbackProxy NativeAdsCallback = new NativeAdsCallbackProxy();

        static MaxNativeAdsSdkAndroid()
        {
            MaxNativeAdsSdk.InitCallbacks();
        }

        public static void Initialize(string adUnitId)
        {
            if (adUnitId == null) return;
            MaxNativeAdsPluginClass.CallStatic("initialize", adUnitId, NativeAdsCallback);
        }

        public static void LoadAd() => MaxNativeAdsPluginClass.CallStatic("loadAd");

        public static bool IsAdReadyToShow() => MaxNativeAdsPluginClass.CallStatic<bool>("isAdReadyToShow");

        public static void ShowAd(string adPlacement, float x, float y, float width, float height) =>
            MaxNativeAdsPluginClass.CallStatic("showAd", adPlacement, x, y, width, height);

        public static void HideAd() => MaxNativeAdsPluginClass.CallStatic("hideAd");

        public static void SetCustomData(string customData)
        {
            MaxNativeAdsPluginClass.CallStatic("setCustomData", customData);
        }

        private class NativeAdsCallbackProxy : AndroidJavaProxy
        {
            public NativeAdsCallbackProxy() : base("io.voodoo.nativeads.unityplugin.MaxNativeAdsUnityCallback") { }

            public void onEvent(string propsStr)
            {
                MaxNativeAdsSdkCallbacks.ForwardEvent(propsStr);
            }
        }
    }
}