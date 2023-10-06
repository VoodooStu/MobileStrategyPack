using Voodoo.Sauce.Internal.SDKs;

namespace Voodoo.Sauce.Internal.Ads
{
    //Interface dedicated to gather Mediation SDK informations (name, version, ad networks ...)
    public interface IMediationAdapterSDK
    {
        MediationSDK GetSDKInformations();
    }
}