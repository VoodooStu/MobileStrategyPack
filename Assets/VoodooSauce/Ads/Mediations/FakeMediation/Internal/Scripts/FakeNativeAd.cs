using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Ads;

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    internal class FakeNativeAd : FakeAd
    {
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _squarreImage;
        [SerializeField] private Sprite _rectangleImage;
        [SerializeField] private RectTransform _transform;

        public void Setup(string layoutName, float x, float y, float width, float height) 
        {
            _transform.position = new Vector3(x, y, 0f);
            _transform.sizeDelta = new Vector2(width, height);

            _image.sprite = layoutName == NativeAdLayout.Square.GetKey() ? _squarreImage : _rectangleImage;
        }
    }
}
