using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.CrossPromo
{
    public class CrossPromoResizer : MonoBehaviour
    {
        [SerializeField]
        private GameObject _voodooCrossPromo;

        private void Start()
        {
            ResizeDefaultFormat();
        }

        private void ResizeDefaultFormat()
        {
            const float r = 2.55f;
            const float defaultFormatRatio = 288.0f / 332.0f;
            const float refWidth = 1080.0f;
            const float refHeight = 1920.0f;
            const float width = refWidth / r;

            GetComponent<CanvasScaler>().referenceResolution =
                (float) Screen.width / Screen.height <= 1.0f
                    ? new Vector2(refWidth, refHeight)
                    : new Vector2(refHeight, refWidth);

            _voodooCrossPromo.GetComponent<RectTransform>().sizeDelta = new Vector2(width, width / defaultFormatRatio);
        }
    }
}