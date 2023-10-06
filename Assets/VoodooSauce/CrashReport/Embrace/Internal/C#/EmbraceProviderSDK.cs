using UnityEngine.Scripting;
using Voodoo.Sauce.Firebase.Interfaces;
using Voodoo.Sauce.Internal.SDKs;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    [Preserve]
    public class EmbraceProviderSDK : ICrashlyticsProviderSDK
    {
        public SDK GetSDKInformations() => new SDK {
            name = EmbraceDependency.NAME,
            icon = $"Resources/{EmbraceDependency.NAME.Replace(" ", "")}.png",
            versions = new SDKVersions {
                unity = EmbraceDependency.PACKAGE_VERSION
            }
        };
    }
}