using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Firebase
{
    [Preserve]
    public class FirebaseCrashlyticsPrivacy : IPrivacyLink
    {
        public string SDKName => "Crashlytics";

        public string PrivacyPolicyUrl => "https://firebase.google.com/terms/crashlytics";

        public PrivacySDKType SDKType => PrivacySDKType.Analytics;
    }
}