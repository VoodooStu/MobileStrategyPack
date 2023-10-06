using com.adjust.sdk.purchase;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Internal.SDKs;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    public class MixpanelAnalyticsProviderSDK : IAnalyticsProviderSDK
    {
        public SDK GetSDKInformations()
        {
            SDK sdk = new SDK();
            sdk.name = "Mixpanel";
            sdk.icon = "Resources/" + sdk.name.Replace(" ", "") + ".png";
            sdk.versions = new SDKVersions();
            sdk.versions.unity = JSON.Parse(System.IO.File.ReadAllText(Application.dataPath + "/VoodooSauce/Analytics/Mixpanel/3rdParty/mixpanel-unity/package.json"))["version"];
            return sdk;
        }
    }
}