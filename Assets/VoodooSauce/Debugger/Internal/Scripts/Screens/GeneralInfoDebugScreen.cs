using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.StoreUtility;
using Voodoo.Sauce.LoadingTime;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Debugger
{
    public class GeneralInfoDebugScreen : Screen
    {
        private void OnEnable()
        {
            ClearDisplay();

            VoodooSettings settings = VoodooSettings.Load();

            ShowVSInfo(settings);
            ShowDevInfo();
            ShowDeviceInfo();
            ShowLoadingTimes();
        }

        private void ShowVSInfo(VoodooSettings settings)
        {
            OpenFoldout("VoodooSauce");
            CopyToClipboard("Version", VoodooSauce.Version());
#if UNITY_IOS
                Label("App Tracking Transparency", "enabled");
#endif
            Label("AutoSettingsUpdate", settings.LastUpdateDate);
            Label("UserAskedToBeForgotten", VoodooSauce.UserRequestedToBeForgotten.ToString());
            Label("CrashReporter", VoodooSauceCore.GetCrashReport().GetCrashReporter().ToString());
            CloseFoldout();
        }


        private void ShowDevInfo()
        {
            OpenFoldout("Development");
            CopyToClipboard("Unity version", Application.unityVersion);
            CopyToClipboard("Bundle ID", Application.identifier);
            CopyToClipboard("Version", Application.version);
            CopyToClipboard("Build", AppInfo.GetVersionCode().ToString());
            CloseFoldout();
        }

        private void ShowDeviceInfo()
        {
            PrivacyCore privacy = VoodooSauceCore.GetPrivacy();

            OpenFoldout("Device");
            CopyToClipboard("IDFA", privacy.GetAdvertisingId());
#if UNITY_IOS
            CopyToClipboard("IDFA authorization status", privacy.GetAuthorizationStatus().ToString());
            CopyToClipboard("IDFV", privacy.GetVendorId());
#endif
            CopyToClipboard("Voodoo User ID", AnalyticsUserIdHelper.GetUserId());
            CopyToClipboard("Device UID", SystemInfo.deviceUniqueIdentifier);
            CopyToClipboard("Operating System", SystemInfo.operatingSystem);
            CloseFoldout();
        }

        private void ShowLoadingTimes()
        {
            OpenFoldout("Loading Times");

            LoadingTimerManager loadingTimes = AnalyticsManager.LoadingTimes;

            if (loadingTimes.IsGlobalLoadingTimerStopped())
            {
                Label("Global", FormattedLoadingTime(loadingTimes.GetRealGlobalLoadingTimeDuration()));
            }
            else
            {
                Label("VoodooSauce.OnGameInteractable() is never called. Please read the documentation.", null, true);
            }

            Label("VoodooSauce", FormattedLoadingTime(loadingTimes.GetVoodooSauceSDKRealLoadingTimeDuration()));
            Label("Unity Engine", FormattedLoadingTime(loadingTimes.GetUnityLoadingTimeDuration()));
            CloseFoldout();
        }

        private static string FormattedLoadingTime(long time) => time >= 1000 ? $"{(time / 1000.0):0.##} s" : $"{time} ms";
    }
}
