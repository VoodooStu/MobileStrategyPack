#if UNITY_ANDROID

using System;
using System.Threading.Tasks;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.StoreUtility
{
	internal static class AndroidAppInfo
	{
		private static AppUpdateCallback _appUpdateCallback;
		private static int _storeVersion = -1;
		private static AppUpdateStatus _appUpdateStatus;
		private static PackageInfo _packageInfo;

		private static async Task<int> GetStoreVersion()
		{
			int maxIteration = 100;
			
			if (_appUpdateCallback == null)
			{
				GetAppUpdateCallback();
				while (_appUpdateCallback?.appUpdateInfo == null && maxIteration > 0)
				{
					maxIteration--;
					await Task.Yield();
				}
			}

			if (_appUpdateCallback == null)
			{
				Debug.LogWarning("Didn't succeed in getting an AppUpdateCallback ");
				return -1;
			}
			
			if (_appUpdateCallback.appUpdateInfo == null)
			{
				Debug.LogWarning("Didn't succeed in getting an AppUpdateInfo ");
				return -1;
			}

			return _appUpdateCallback.appUpdateInfo.availableVersionCode;
		}

		public static async void GetAppUpdateStatus(Action<AppUpdateStatus> onAppUpdateStatusRetrieved)
		{
			if (_storeVersion == -1)
			{
				_storeVersion = await GetStoreVersion();
				OnStoreVersionRetrieved();
			}

			onAppUpdateStatusRetrieved?.Invoke(_appUpdateStatus);
		}

		private static void OnStoreVersionRetrieved()
		{
			int currentVersion = GetBundleVersionCode();

			if (_storeVersion == -1)
			{
				_appUpdateStatus = AppUpdateStatus.UNKNOWN;
			}
			else if (_storeVersion > currentVersion)
			{
				_appUpdateStatus = AppUpdateStatus.NEED_UPDATE;
			}
			else if (_storeVersion < currentVersion)
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
		    Application.OpenURL("market://details?id=" + Application.identifier);
	    }

		internal static int GetBundleVersionCode()
	    {
		    PackageInfo packageInfo = GetPackageInfo();
		    return packageInfo.versionCode;
	    }

	    private static PackageInfo GetPackageInfo()
	    { 
		    if (_packageInfo != null) {
			    return _packageInfo;
		    }
		    
		    AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		    AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
		    AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
		    string packageName = currentActivity.Call<string>("getPackageName");
		    AndroidJavaObject info = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
		    _packageInfo = new PackageInfo {
			    firstInstallTime = info.Get<long>("firstInstallTime"),
			    lastUpdateTime   = info.Get<long>("lastUpdateTime"),
			    packageName      = info.Get<string>("packageName"),
			    versionCode      = info.Get<int>("versionCode"),
			    versionName      = info.Get<string>("versionName"),
		    };

		    return _packageInfo;
	    }

		private static void GetAppUpdateCallback()
		{
			if (_appUpdateCallback?.appUpdateInfo != null)
			{
				return;
			}
			
			try
			{
				AndroidJavaClass appUpdatePlugin = new AndroidJavaClass("io.voodoo.appupdate.AppUpdatePlugin");
				_appUpdateCallback = new AppUpdateCallback();
				appUpdatePlugin.CallStatic("getAppUpdateInfo", _appUpdateCallback);
			}
			catch (Exception e)
			{
				// ignored
				_appUpdateCallback = null;
			}
		}
	}
	
	public class AppUpdateCallback: AndroidJavaProxy
	{
		public AppUpdateInfo appUpdateInfo;
				
		public AppUpdateCallback(): base("io.voodoo.appupdate.AppUpdateCallback")
		{
		}
			
		public void onSuccess(string data)
		{
			appUpdateInfo = JsonUtility.FromJson<AppUpdateInfo>(data);
		}

		public void onFailure(string data)
		{
			appUpdateInfo = new AppUpdateInfo
			{
				availableVersionCode = -2
			};
		}
	}

	public class PackageInfo
	{
		public long firstInstallTime;
		public long lastUpdateTime;
		public string packageName;
		public int versionCode;
		public string versionName;
	}

	public class AppUpdateInfo
	{
		public int availableVersionCode;
		public string packageName;
		public int updateAvailability;
		public int updatePriority;
		public bool isUpdateTypeAllowed;
	}
}

#endif