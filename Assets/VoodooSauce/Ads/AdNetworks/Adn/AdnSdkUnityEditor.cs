// This script is used for Unity Editor and non Android or iOS platforms.
#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IOS)
using System;
namespace Voodoo.Sauce.Internal.Ads.Adn
{
    public class AdnSdkUnityEditor
    {
        public static void SubscribeOnSdkInitializedEvent(Action sdkInitializedEvent)
        {
            // Adn is disabled in editor mode 
        }

        public static void SetBidTokenExtraParams(string extraParamsJson)
        {
            // Adn is disabled in editor mode 
        }
    }
}

#endif