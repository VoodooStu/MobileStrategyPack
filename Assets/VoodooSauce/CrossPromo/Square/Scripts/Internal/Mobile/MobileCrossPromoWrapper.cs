using System;
using UnityEngine;
using Voodoo.Sauce.Internal.CrossPromo.Models;

namespace Voodoo.Sauce.Internal.CrossPromo.Mobile
{
    /// <summary>
    /// Wrapper for mobile native functions
    /// </summary>
    internal static class MobileCrossPromoWrapper
    {
        /// <summary>
        /// Check if user has installed the app
        /// </summary>
        /// <param name="game">Game to check</param>
        /// <returns>true if the user install the app, otherwise return false</returns>
        public static bool IsAppInstalled(GameToPromote game)
        {
            if (Debug.isDebugBuild || Application.isEditor)
                return false;
#if UNITY_ANDROID
            var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            var packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
            try {
                return packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", game.bundle_id) != null;
            } catch (Exception) {
                return false;
            }
#else
            return false;
#endif
        }
    }
}