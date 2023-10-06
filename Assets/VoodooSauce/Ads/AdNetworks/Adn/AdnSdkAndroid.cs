#if UNITY_ANDROID
using System;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Ads.Adn
{
    public class AdnSdkAndroid
    {
        private const string ADN_SDK_PACKAGE = "io.voodoo.adn.sdk.open";
        private static readonly AndroidJavaClass adnSdkClass = AdnSdkClassInstance();
        private static readonly InitCallbackProxy initCallbackProxy = InitCallbackProxyInstance();
        private static Action onSdkInitializedEvent;

        public static void SubscribeOnSdkInitializedEvent(Action sdkInitializedEvent)
        {
            if (adnSdkClass != null && initCallbackProxy != null) {
                onSdkInitializedEvent = sdkInitializedEvent;
                adnSdkClass.CallStatic("subscribeOnSdkInitialized", initCallbackProxy);
            }
        }

        public static void SetBidTokenExtraParams(string extraParamsJson)
        {
            adnSdkClass?.CallStatic("setBidTokenExtraParams", extraParamsJson);
        }

        private static AndroidJavaClass AdnSdkClassInstance()
        {
            try {
                return new AndroidJavaClass($"{ADN_SDK_PACKAGE}.AdnSdk");
            } catch (Exception) {
                return null;
            }
        }

        private static InitCallbackProxy InitCallbackProxyInstance()
        {
            try {
                return new InitCallbackProxy();
            } catch (Exception) {
                return null;
            }
        }

        private class InitCallbackProxy : AndroidJavaProxy
        {
            public InitCallbackProxy() : base($"{ADN_SDK_PACKAGE}.OnSdkInitializedListener") { }

            public void onComplete()
            {
                onSdkInitializedEvent?.Invoke();
            }
        }
    }
}
#endif