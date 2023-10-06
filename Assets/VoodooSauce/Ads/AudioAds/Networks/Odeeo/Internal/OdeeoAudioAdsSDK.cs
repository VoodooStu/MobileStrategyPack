using UnityEngine.Scripting;
using Voodoo.Sauce.Analytics.Common.Internal;
using Voodoo.Sauce.Internal.SDKs;

namespace Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class OdeeoAudioAdsSDK: IAudioAdsSDK
    {
        public SDK GetSDKInformations()
        {
            SDK sdk = new SDK();
            sdk.name = "Odeeo";
            sdk.icon = "Resources/" + sdk.name.Replace(" ", "") + ".png";
            sdk.versions = new SDKVersions();
            sdk.versions.unity = PlayOnSDK.SDK_VERSION;
            
            //Currently the PlayOnSDK are packaged altogether with the unitypackage
            sdk.versions.android = PlayOnSDK.SDK_VERSION;
            sdk.versions.ios = PlayOnSDK.SDK_VERSION;
            return sdk;
        }
    }
}