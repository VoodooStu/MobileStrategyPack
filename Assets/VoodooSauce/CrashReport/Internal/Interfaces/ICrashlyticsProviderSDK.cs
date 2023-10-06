using Voodoo.Sauce.Internal.SDKs;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Firebase.Interfaces
{
    //Interface dedicated to gather Crashlytics SDK informations (name, version ...)
    public interface ICrashlyticsProviderSDK
    {
        SDK GetSDKInformations();
    }
}