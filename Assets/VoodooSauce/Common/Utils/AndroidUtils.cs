using UnityEngine;
using Voodoo.Sauce.Common.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Utils
{
    public static class AndroidUtils
    {
        public static AndroidJavaClass GetUnityPlayerClass() => new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        public static AndroidJavaObject GetCurrentActivity() => GetUnityPlayerClass().GetStatic<AndroidJavaObject>("currentActivity");
    }
}