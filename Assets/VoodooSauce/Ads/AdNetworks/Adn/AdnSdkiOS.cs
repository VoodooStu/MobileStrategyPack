#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;

namespace Voodoo.Sauce.Internal.Ads.Adn
{
    public class AdnSdkiOS
    {
        private delegate void SdkInitCallback();

        private static Action onSdkInitializedEvent;

        [DllImport("__Internal")]
        private static extern void _AdnSubscribeOnSdkInitialized(SdkInitCallback callback);

        public static void SubscribeOnSdkInitializedEvent(Action sdkInitializedEvent)
        {
            onSdkInitializedEvent = sdkInitializedEvent;
            _AdnSubscribeOnSdkInitialized(OnSdkInitialized);
        }

        [DllImport("__Internal")]
        private static extern void _AdnSetBidTokenExtraParams(string extraParamsJson);

        public static void SetBidTokenExtraParams(string extraParamsJson)
        {
            _AdnSetBidTokenExtraParams(extraParamsJson);
        }

        [MonoPInvokeCallback(typeof(SdkInitCallback))]
        private static void OnSdkInitialized()
        {
            onSdkInitializedEvent?.Invoke();
        }
    }
}
#endif