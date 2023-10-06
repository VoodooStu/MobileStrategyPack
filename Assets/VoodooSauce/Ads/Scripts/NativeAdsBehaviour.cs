using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Ads;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Ads
{
    [ExecuteInEditMode,
     RequireComponent(typeof(RectTransform), typeof(AspectRatioFitter), typeof(Image))]
    public class NativeAdsBehaviour : MonoBehaviour
    {
        private const string TAG = "NativeAdsBehaviour";

        [SerializeField]
        private string adPlacement;
        [SerializeField]
        private NativeAdLayout layout;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private NativeAdLayout? _appliedLayout;
        private string _displayedAdPlacement;
        private Guid _id;

        private void Awake()
        {
            adPlacement = adPlacement.Trim();
            SetupAspectRatio();
            UpdateText();

            if (!Application.isPlaying) {
                return;
            }

            GetComponent<Image>().enabled = PlatformUtils.UNITY_EDITOR;
            GetComponentInChildren<Text>().gameObject.SetActive(PlatformUtils.UNITY_EDITOR);
            
            if (string.IsNullOrEmpty(adPlacement)) {
                VoodooLog.LogError(Module.ADS, TAG, "The adPlacement of this NativeAd is empty. This ad isn't working.");
            }
            
            _rectTransform = GetComponent<RectTransform>();
            _canvas = _rectTransform.GetComponentInParent<Canvas>();
            _id = Guid.NewGuid();
        }

        private void SetupAspectRatio()
        {
            if (_appliedLayout == layout) return;
            var aspectRatioFitter = GetComponent<AspectRatioFitter>();
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            aspectRatioFitter.aspectRatio = layout.GetRatio();
            _appliedLayout = layout;
            UpdateText();
        }

        private void UpdateText()
        {
            _displayedAdPlacement = adPlacement;
            if (Application.isPlaying && !AdsManager.NativeAds.IsEnabled()) {
                GetComponentInChildren<Text>().text = "NativeAd:\nDisabled.";
            } else if (adPlacement.Length == 0) {
                GetComponentInChildren<Text>().text = "NativeAd\nError: AdPlacement is empty.";
            } else {
                GetComponentInChildren<Text>().text = "NativeAd:\n" + adPlacement + '(' + layout + ')';
            }
        }

        private void OnRenderObject()
        {
            if (!Application.isPlaying) {
                SetupAspectRatio();
                if (_displayedAdPlacement != adPlacement) {
                    UpdateText();
                }
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying) {
                VoodooSauce.SubscribeOnInitFinishedEvent(ShowAfterVoodooSauceInit);
            }
        }

        private void ShowAfterVoodooSauceInit(VoodooSauceInitCallbackResult result)
        {
            if (!AdsManager.NativeAds.IsEnabled() || gameObject == null || !gameObject.activeInHierarchy) {
                return;
            }

            if (AdsManager.AreFakeAdsEnabled())
            {
                Rect rect = _rectTransform.rect;
                Vector3 position = _rectTransform.position;
                var finalRect = new Rect(position.x, position.y, rect.width, rect.height);
                AdsManager.NativeAds.Show(layout, finalRect, adPlacement, _id);
                return;
            }

            // World position of the bounds of this ad
            var corners = new Vector3[4]; // index is 0: bottom left, 1: top left , 2: top right , 3: bottom right
            _rectTransform.GetWorldCorners(corners); 
            if (_canvas.renderMode == RenderMode.WorldSpace || _canvas.renderMode == RenderMode.ScreenSpaceCamera) 
            {
                // If the canvas is linked to a camera, we need to convert the world positions to screen positions
                for (var index = 0; index < corners.Length; ++index) {
                    corners[index] = _canvas.worldCamera.WorldToScreenPoint(corners[index]);
                }   
            }

            // origin is bottom left in Android and iOS vs top left in editor.
            //so we need to invert y            
            var bounds = new Rect(corners[1].x, 
                                  Screen.height - corners[1].y, 
                                  corners[2].x - corners[0].x, 
                                  corners[2].y - corners[0].y);

             AdsManager.NativeAds.Show(layout, bounds, adPlacement, _id);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            VoodooSauce.UnSubscribeOnInitFinishedEvent(ShowAfterVoodooSauceInit);
            if (AdsManager.NativeAds.IsEnabled()) {
                AdsManager.NativeAds.Hide(_id);
            }
        }
    }
}