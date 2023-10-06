using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;

// ReSharper disable ConvertToConstant.Local
namespace Voodoo.Sauce.Privacy
{
    internal partial class PrivacyManager : PrivacyCore
    {
        private static class VendorIdHelperAndroid
        {
            private const string TAG = "VendorIdHelperAndroid";
            private static string _vendorId;
            private delegate void RequestVendorIdCallback(string vendorId);
#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly bool IS_ANDROID = true;
#else
            private static readonly bool IS_ANDROID = false;
#endif
            internal static async Task<string> RequestVendorId()
            {
                if (!IS_ANDROID)
                    return "";
                _vendorId = await GetAndroidVendorIdAsync();
                return _vendorId;
            }

            private static Task<string> GetAndroidVendorIdAsync()
            {
                var tcs = new TaskCompletionSource<string>();
                GetAppSetIdAndroidPlayService((vendorId) => { tcs.TrySetResult(vendorId); });
                return tcs.Task;
            }

            private static void DisposeObjects(List<AndroidJavaObject> androidJavaObjects)
            {
                foreach (var androidJavaObject in androidJavaObjects)
                {
                    androidJavaObject?.Dispose();
                }
            }

            private static void GetAppSetIdAndroidPlayService(RequestVendorIdCallback callBack)
            {
                var disposeList = new List<AndroidJavaObject>();
                try
                {
                    var asyncTaskRunner = new AndroidJavaClass("com.google.android.gms.tasks.Tasks");
                    var appSetIdClientClass = new AndroidJavaClass("com.google.android.gms.appset.AppSet");
                    var ajoCurrentActivity =
                        new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>(
                            "currentActivity");
                    disposeList.Add(asyncTaskRunner);
                    disposeList.Add(appSetIdClientClass);
                    disposeList.Add(ajoCurrentActivity);

                    var applicationContext = ajoCurrentActivity?.Call<AndroidJavaObject>("getApplicationContext");
                    if (applicationContext == null)
                        throw new Exception("applicationContext is null");
                    disposeList.Add(applicationContext);

                    var appSetClientObject =
                        appSetIdClientClass.CallStatic<AndroidJavaObject>("getClient", applicationContext);
                    if (appSetClientObject == null)
                        throw new Exception("appSetClientObject is null");
                    disposeList.Add(appSetClientObject);

                    var taskObject = appSetClientObject.Call<AndroidJavaObject>("getAppSetIdInfo");
                    if (taskObject == null)
                        throw new Exception("taskObject is null");
                    disposeList.Add(taskObject);

                    var appSetIdResult = asyncTaskRunner.CallStatic<AndroidJavaObject>("await", taskObject);
                    if (appSetIdResult != null)
                    {
                        disposeList.Add(appSetIdResult);
                        var vendorId = appSetIdResult.Call<string>("getId");
                        var scope = appSetIdResult.Call<int>("getScope");
                        //Scope 2 is developer / vendor scope, Scope 1 is App scope.
                        //https://developers.google.com/android/reference/com/google/android/gms/appset/AppSetIdInfo#public-static-final-int-scope_developer
                        if (scope != 2)
                        {
                            vendorId = "";
                        }

                        DisposeObjects(disposeList);
                        callBack(vendorId);
                    }
                    else
                    {
                        callBack("");
                        DisposeObjects(disposeList);
                    }
                }
                catch (AndroidJavaException e) //Potentially The device doesnt have GMS
                {
                    VoodooLog.LogError(Module.PRIVACY, TAG,
                        $"[VendorIdHelperAndroid.GetIdfaAndroidPlayService] Class not found with error {e.Message}\n{e.StackTrace}");
                    VoodooSauceCore.GetCrashReport().LogException(e);
                    callBack("");
                }
                catch (Exception e)
                {
                    VoodooLog.LogError(Module.PRIVACY, TAG,
                        $"[VendorIdHelperAndroid.GetIdfaAndroidPlayService] there was an error trying to get advertising id and tracking {e.Message}\n{e.StackTrace}");
                    VoodooSauceCore.GetCrashReport().LogException(e);
                    callBack("");
                }
                finally
                {
                    DisposeObjects(disposeList);
                }
            }
        }
    }
}