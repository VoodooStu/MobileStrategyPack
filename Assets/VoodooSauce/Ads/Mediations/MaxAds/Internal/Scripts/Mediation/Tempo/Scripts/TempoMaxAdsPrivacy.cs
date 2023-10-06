using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Ads
{
	[Preserve]
	public class TempoMaxAdsPrivacy : IPrivacyLink
	{
		public string SDKName => "Tempo";

		public string PrivacyPolicyUrl => "https://assets.tempoplatform.com/PrivacyPolicy.pdf";

		public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
	}
}