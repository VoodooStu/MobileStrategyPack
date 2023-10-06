using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public class OdeeoPrivacy: IPrivacyLink
    {
        public string SDKName => "Odeeo";
        public string PrivacyPolicyUrl => "https://odeeo.io/privacy-policy/";
        public PrivacySDKType SDKType => PrivacySDKType.AdNetwork;
    }
}