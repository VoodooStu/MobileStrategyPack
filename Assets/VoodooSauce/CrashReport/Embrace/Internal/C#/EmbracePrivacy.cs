using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    [Preserve]
    public class EmbracePrivacy : IPrivacyLink
    {
        public string SDKName => "Embrace";

        public string PrivacyPolicyUrl => "https://embrace.io/docs/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.Analytics;
    }
}