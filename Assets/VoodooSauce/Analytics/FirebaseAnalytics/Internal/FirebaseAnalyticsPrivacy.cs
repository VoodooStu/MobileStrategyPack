using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    public class FirebaseAnalyticsPrivacy : IPrivacyLink
    {
        public string SDKName => "Firebase";

        public string PrivacyPolicyUrl => "https://firebase.google.com/support/privacy";

        public PrivacySDKType SDKType => PrivacySDKType.Analytics;
    }
}
