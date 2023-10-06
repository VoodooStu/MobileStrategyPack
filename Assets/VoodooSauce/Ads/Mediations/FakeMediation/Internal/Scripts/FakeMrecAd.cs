using UnityEngine;
using Voodoo.Sauce.Ads;

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    internal class FakeMrecAd : FakeAd
    {
        [SerializeField] private RectTransform _transform;

        public void Setup(float x, float y)
        {
            _transform.position = new Vector3(x, y, 0f);
            _transform.sizeDelta = MrecAdBehaviour.GetMrecPlaceholderSize();
        }
    }
}
