using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.StoreUtility
{
	public static class AppInfo
	{
		/// <summary>
		/// Open the Google Play Store or Apple Store to the app page (depending on the device)
		/// Partially works on Editor
		/// </summary>
		public static void OpenStore()
		{
#if UNITY_ANDROID
	#if UNITY_EDITOR
		Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
    #else
		AndroidAppInfo.OpenStore();
	#endif
#elif UNITY_IOS
	#if UNITY_EDITOR
		Application.OpenURL("https://apps.apple.com");
    #else
		IOSAppInfo.OpenStore();
	#endif
#endif
		}

		/// <summary>
		/// Retrieve the Store info and call the onAppUpdateStatusRetrieved with the update status.
		/// It will return 1 in editor and 3 if the user doesn't have internet access, 
		/// 0:SAME_VERSION, 1:TEST_MODE, 2:NEED_UPDATE, 3:UNKNOWN
		/// </summary>
		/// <param name="onAppUpdateStatusRetrieved"></param>
		public static void GetAppUpdateStatus(Action<AppUpdateStatus> onAppUpdateStatusRetrieved)
		{
#if UNITY_EDITOR
			onAppUpdateStatusRetrieved?.Invoke(AppUpdateStatus.TEST_MODE);
#elif UNITY_ANDROID
			AndroidAppInfo.GetAppUpdateStatus(onAppUpdateStatusRetrieved);
#elif UNITY_IOS
			IOSAppInfo.GetAppUpdateStatus(onAppUpdateStatusRetrieved);
#endif
		}
		
		/// <summary>
		/// Retrieve the "Bundle Version Code" for Android and the "Build Number" for iOS
		/// </summary>
		/// <returns></returns>
		public static int GetVersionCode()
		{
#if UNITY_ANDROID
	#if UNITY_EDITOR
		    return UnityEditor.PlayerSettings.Android.bundleVersionCode;
	#else
			return AndroidAppInfo.GetBundleVersionCode();
	#endif
#elif UNITY_IOS
	#if UNITY_EDITOR
			try { return int.Parse(UnityEditor.PlayerSettings.iOS.buildNumber); }
			catch { return -1; }
	#else
			return IOSAppInfo.GetBundleNumber();
	#endif
#else
	        return -1;
#endif
		}
	}

	public enum AppUpdateStatus
	{
		SAME_VERSION = 0,
		TEST_MODE = 1,
		NEED_UPDATE = 2,
		UNKNOWN = 3
	}
}