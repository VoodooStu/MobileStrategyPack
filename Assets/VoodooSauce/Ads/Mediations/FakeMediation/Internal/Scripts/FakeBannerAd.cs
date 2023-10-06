using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    internal sealed class FakeBannerAd : FakeAd
    {
        private void Start()
        {
            if (!PlatformUtils.UNITY_IOS || !DeviceUtils.HasBottomSafeArea()) {
                return;
            }

            GetComponent<RectTransform>().sizeDelta = new Vector2(0, 260);
        }
    }
}