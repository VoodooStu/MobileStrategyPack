using Voodoo.Sauce.Internal.SDKs;

namespace Voodoo.Sauce.Internal.Analytics
{
    //Interface dedicated to gather Analytics SDK informations (name, version ...)
    public interface IAnalyticsProviderSDK
    {
        SDK GetSDKInformations();
    }
}