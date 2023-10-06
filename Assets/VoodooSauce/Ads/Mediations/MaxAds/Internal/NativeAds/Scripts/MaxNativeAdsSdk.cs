using System;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Ads
{
    public class MaxNativeAdsSdk :
#if UNITY_EDITOR
        MaxNativeAdsSdkUnityEditor
#elif UNITY_ANDROID
    MaxNativeAdsSdkAndroid
#elif UNITY_IPHONE || UNITY_IOS
    MaxNativeAdsSdkiOS
#else
    MaxNativeAdsSdkUnityEditor
#endif
    {
        // Allocate the MaxNativeAdsSdkCallbacks singleton, which receives all callback events from the native ads SDKs.
        internal static void InitCallbacks()
        {
            Type type = typeof(MaxNativeAdsSdkCallbacks);
            // Its Awake() method sets Instance.
            var gameObject = new GameObject("MaxNativeAdsSdkCallbacks", type).GetComponent<MaxNativeAdsSdkCallbacks>();
            if (MaxNativeAdsSdkCallbacks.Instance != gameObject)
            {
                VoodooLog.LogWarning(Module.ADS, "MaxNativeAdsSdk", "It looks like you have the " + type.Name + " on a GameObject in your scene. Please remove the script from your scene.");
            }
        }
    }
}