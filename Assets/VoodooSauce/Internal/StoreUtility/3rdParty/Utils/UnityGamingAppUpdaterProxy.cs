using AOT;
using UnityEngine;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace Voodoo.Sauce.Internal.StoreUtility
{
	internal static class UnityGamingAppUpdaterProxy
    {
        // The status should be the same as the AppTrackingStatus (from native Side)
        public delegate void RequestAppUpdateCallback(int status, string version);

        // You can use this attribute to listen to the callback 
        private static RequestAppUpdateCallback _requestAppUpdaterCallback;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _setRequestTrackingDelegatesConfiguration(RequestAppUpdateCallback delegateParameters);
  
    [DllImport("__Internal")]
    private static extern void _openStoreApple();
#endif

        #region [NATIVE Callbacks]

        [MonoPInvokeCallback(typeof(RequestAppUpdateCallback))]
        public static void RequestAppUpdaterReceived(int response, string version)
        {
            _requestAppUpdaterCallback?.Invoke(response, version);
            Debug.Log(" VA - Request App Updater response received " + response + "version number" + version);
            _requestAppUpdaterCallback = null;
        }

        #endregion

        //RequestAppUpdateCallback callback

        #region [NATIVE method]

        public static void RequestAppUpdaterDelegatesConfiguration(RequestAppUpdateCallback callback)
        {
#if UNITY_IOS
       _requestAppUpdaterCallback += callback;
       _setRequestTrackingDelegatesConfiguration(RequestAppUpdaterReceived);
#endif
        }

        public static void OpenAppStoreProxy()
        {
#if UNITY_IOS
        _openStoreApple();
#endif
        }

        #endregion
    }
}