using System;
using System.Threading.Tasks;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Core.Model;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Privacy
{
    internal partial class PrivacyManager : PrivacyCore
    {
        public struct AdvertisingStatus
        {
            public string Idfa;
            public bool AdTrackingEnabled;
        }
        
        private static class IdfaHelper
        {
#pragma warning disable 1998
            // The warning is disabled here because some code is unavailable on some platforms.
            public static async Task<AdvertisingStatus> RequestAdvertisingId(VoodooSettings voodooSettings, PrivacyCore privacy)
#pragma warning restore 1998
            {
                string idfa = LIMITED_AD_TRACKING_ID;
                bool adTrackingEnabled = false;
#if UNITY_EDITOR
                string editorIdfa = EditorIdfa.Get();
                idfa = string.IsNullOrEmpty(editorIdfa) ? LIMITED_AD_TRACKING_ID : editorIdfa;
                adTrackingEnabled = true;
#elif UNITY_ANDROID
            bool waiting = true;
            var status = Application.RequestAdvertisingIdentifierAsync((advertisingId, adTracking, error)
                => { idfa = advertisingId;
                adTrackingEnabled = adTracking;
                waiting = false;
            });
            if(status) {
                while (waiting) await Task.Yield();
            } else {
                AdvertisingStatus advertisingStatus = await IdfaHelperAndroid.GetIdfaAndroidAsync();
                idfa = advertisingStatus.Idfa;
                adTrackingEnabled = advertisingStatus.AdTrackingEnabled;
            }
#elif UNITY_IOS
            adTrackingEnabled = privacy.GetAuthorizationStatus() == IdfaAuthorizationStatus.Authorized;
            idfa = UnityEngine.iOS.Device.advertisingIdentifier;
#endif
                return new AdvertisingStatus {Idfa = idfa, AdTrackingEnabled = adTrackingEnabled};
            }
        }
        
#if UNITY_ANDROID
        private static class IdfaHelperAndroid
        {
            private const string TAG = "IdfaHelperAndroid";

            public delegate void RequestAdvertisingIdCallback(string advertisingId, bool trackingEnabled);

            public static Task<AdvertisingStatus> GetIdfaAndroidAsync()
            {
                var tcs = new TaskCompletionSource<AdvertisingStatus>();
                GetIdfaAndroidPlayService((advertisingId, trackingEnabled) => {
                    tcs.TrySetResult(new AdvertisingStatus {Idfa = advertisingId, AdTrackingEnabled = trackingEnabled});
                });
                return tcs.Task;
            }

            private static void GetIdfaAndroidPlayService(RequestAdvertisingIdCallback callBack)
            {
                AndroidJavaClass ajcPlayServiceAdvertisingClient = null;
                AndroidJavaObject ajoCurrentActivity = null;
                AndroidJavaObject appInfo = null;
                try {
                    ajcPlayServiceAdvertisingClient =
                        new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                    ajoCurrentActivity =
                        new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>(
                            "currentActivity");
                    var applicationContext = ajoCurrentActivity?.Call<AndroidJavaObject>("getApplicationContext");

                    if (applicationContext == null) {
                        callBack(LIMITED_AD_TRACKING_ID, false);
                        ajcPlayServiceAdvertisingClient.Dispose();
                        ajoCurrentActivity?.Dispose();
                        return;
                    }

                    appInfo = ajcPlayServiceAdvertisingClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", applicationContext);

                    if (appInfo != null) {
                        var advertisingId = appInfo.Call<string>("getId");
                        var adTrackingEnabled = !appInfo.Call<bool>("isLimitAdTrackingEnabled");
                        callBack(advertisingId, adTrackingEnabled);
                    } else {
                        VoodooLog.LogError(Module.PRIVACY, TAG,
                            "[IdfaHelper.RequestAdvertisingId] appInfo NULL trying to get advertising id and tracking");
                        callBack(LIMITED_AD_TRACKING_ID, false);
                    }
                } catch (AndroidJavaException e) //Potentially The device doesnt have GMS
                {
                    VoodooSauceCore.GetCrashReport().LogException(e);
                    VoodooLog.LogError(Module.PRIVACY, TAG, 
                        $"[IdfaHelper.RequestAdvertisingId] Class not found with error {e.Message}\n{e.StackTrace}");
                    callBack(LIMITED_AD_TRACKING_ID, false);
                } catch (Exception e) {
                    VoodooSauceCore.GetCrashReport().LogException(e);
                    VoodooLog.LogError(Module.PRIVACY, TAG,
                        $"[IdfaHelper.RequestAdvertisingId] there was an error trying to get advertising id and tracking {e.Message}\n{e.StackTrace}");
                    callBack(LIMITED_AD_TRACKING_ID, false);
                } finally {
                    ajcPlayServiceAdvertisingClient?.Dispose();
                    ajoCurrentActivity?.Dispose();
                    appInfo?.Dispose();
                }
            }
        }
#endif
    }
}
