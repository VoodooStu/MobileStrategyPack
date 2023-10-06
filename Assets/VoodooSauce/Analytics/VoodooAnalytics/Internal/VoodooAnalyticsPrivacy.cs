using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    public class VoodooAnalyticsPrivacy : IPrivacyLink
    {
        public string SDKName => "Voodoo";

        public string PrivacyPolicyUrl => "https://www.voodoo.io/app-privacy-policy";

        public PrivacySDKType SDKType => PrivacySDKType.Analytics;
    }
}
