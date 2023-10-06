using System.Collections.Generic;
using Voodoo.Sauce.Internal.SDKs;

namespace Voodoo.Sauce.Debugger
{
    public class SDKListDebugScreen : Screen
    {
        private void Start()
        {
            InstantiateSdkList(VoodooSauceSDKs.Load().SDKs);
        }

        private void InstantiateSdkList(SDKList sdkList)
        {
            OpenFoldout("Info");
            {
                RowLabel("VS Version", sdkList.vsVersion);
                RowLabel("Last Update Date", sdkList.lastUpdateDate);
            }
            CloseFoldout();

            foreach (MediationSDK mediation in sdkList.ads) {
                OpenFoldout(mediation.name);
                {
                    CreateMediation(mediation);
                }
                CloseFoldout();
            }
            
            OpenFoldout("Analytics");
            {
                RowLabel("SDK name", new[] { "Unity", "iOS", "Android" });
                CreateSdk(sdkList.analytics);
            }
            CloseFoldout();

            OpenFoldout("Crashlytics");
            {
                RowLabel("SDK name", new[] { "Unity", "iOS", "Android" });
                CreateSdk(sdkList.crashlytics);
            }
            CloseFoldout();
            
            OpenFoldout("AudioAds");
            {
                RowLabel("SDK name", new[] { "Unity", "iOS", "Android" });
                CreateSdk(sdkList.audioAds);
            }
            CloseFoldout();
        }

        private void CreateMediation(MediationSDK mediation)
        {
            RowLabel(mediation.name, mediation.GetVersionsAsArray());
            RowLabel("Ad Network", new[] { "iOS", "Adapter", "Android", "Adapter" });
            foreach (AdNetworkSDK adNetwork in mediation.adNetworks) {
                CreateAdNetwork(adNetwork);
            }

            if (mediation.otherUtilitySdk == null || mediation.otherUtilitySdk.Count <= 0) return;
            RowLabel("Other Utility SDK", new[] { "Unity", "iOS", "Android"});
            foreach (SDK otherUtilitySdk in mediation.otherUtilitySdk) {
                CreateMediationOtherUtilitySdk(otherUtilitySdk);
            }
        }

        private void CreateMediationOtherUtilitySdk(SDK otherUtilitySdk)
        {
            RowLabel(otherUtilitySdk.name, otherUtilitySdk.GetVersionsAsArray());
        }

        private void CreateAdNetwork(AdNetworkSDK adNetwork)
        {
            var certified = "";
            if (adNetwork.ios.certified || adNetwork.android.certified) {
                certified = " (c)";
            }
            RowLabel(adNetwork.name + certified, adNetwork.GetVersionsAsArray());
        }

        private void CreateSdk(List<SDK> sdks)
        {
            foreach (SDK sdk in sdks) {
                RowLabel(sdk.name, sdk.versions.GetVersionsAsArray());
            }
        }
    }
}
