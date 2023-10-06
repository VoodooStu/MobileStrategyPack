#if UNITY_IOS

using System;
using System.Threading.Tasks;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.StoreUtility
{
    internal static class IOSAppInfo
    {
        private static string _storeVersion = string.Empty;
        private static AppUpdateStatus _appUpdateStatus;
        private static int? _bundleVersion;

        private static async Task<string> GetStoreVersion()
        {
            int maxIteration = 100;
            string currentVersion = "NoVersion";
            
            UnityGamingAppUpdaterProxy.RequestAppUpdaterDelegatesConfiguration(delegate(int status, string version)
            {
                currentVersion = version;
            });

            while (currentVersion == "NoVersion" && maxIteration > 0)
            {
                maxIteration--;
                await Task.Yield();
            }
            
            return currentVersion == "NoVersion" ? string.Empty : currentVersion;
        }
        
        public static async void GetAppUpdateStatus(Action<AppUpdateStatus> onAppUpdateStatusRetrieved)
        {
            if (_storeVersion == String.Empty)
            {
                _storeVersion = await GetStoreVersion();
                OnStoreVersionRetrieved();
            }
            
            onAppUpdateStatusRetrieved?.Invoke(_appUpdateStatus);
        }
        
        private static void OnStoreVersionRetrieved()
        {
            if (_storeVersion == string.Empty)
            {
                _appUpdateStatus = AppUpdateStatus.UNKNOWN;
                return;
            }

            Version.TryParse(Application.version, out Version currentVersion);
            Version.TryParse(_storeVersion, out Version storeVersionTemp);
            
            if (storeVersionTemp > currentVersion)
            {
                _appUpdateStatus = AppUpdateStatus.NEED_UPDATE;
            }
            else if (storeVersionTemp < currentVersion)
            {
                _appUpdateStatus = AppUpdateStatus.TEST_MODE;
            }
            else
            {
                _appUpdateStatus = AppUpdateStatus.SAME_VERSION;
            }
        }
        
        public static void OpenStore()
        {
            UnityGamingAppUpdaterProxy.OpenAppStoreProxy();
        }

        internal static int GetBundleNumber()
        {
            if (_bundleVersion == null) {
                _bundleVersion = getBundleVersion();
            }

            return (int)_bundleVersion;
        }
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int getBundleVersion();
    }
}

#endif