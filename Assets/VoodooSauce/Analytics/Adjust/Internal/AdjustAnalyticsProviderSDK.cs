using com.adjust.sdk;
using UnityEngine.Scripting;
using Voodoo.Sauce.Internal.SDKs;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    public class AdjustAnalyticsProviderSDK : IAnalyticsProviderSDK
    {
        public SDK GetSDKInformations()
        {
            SDK sdk = new SDK();
            sdk.name = "Adjust";
            sdk.icon = "Resources/" + sdk.name.Replace(" ", "") + ".png";
            sdk.versions = new SDKVersions();
            sdk.versions.unity = "4.33.0";
            sdk.versions.android = "4.33.2";
            sdk.versions.ios = "4.33.2";
            return sdk;
        }
    }
}