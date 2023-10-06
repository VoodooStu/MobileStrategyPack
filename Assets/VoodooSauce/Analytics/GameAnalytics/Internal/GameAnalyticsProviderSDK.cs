using UnityEngine.Scripting;
using Voodoo.Sauce.Internal.SDKs;

namespace Voodoo.Sauce.Internal.Analytics.Editor
{
    [Preserve]
    public class GameAnalyticsProviderSDK : IAnalyticsProviderSDK
    {
        public SDK GetSDKInformations()
        {
            SDK sdk = new SDK();
            sdk.name = "GameAnalytics";
            sdk.icon = "Resources/" + sdk.name.Replace(" ", "") + ".png";
            sdk.versions = new SDKVersions();
            sdk.versions.unity = "7.3.23";
            return sdk;
        }
    }
}