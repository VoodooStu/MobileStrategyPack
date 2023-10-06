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
     RequireComponent(typeof(RectTransform)),
     RequireComponent(typeof(ContentSizeFitter)),
     RequireComponent(typeof(LayoutElement)),
     RequireComponent(typeof(Image)),
    ]
    public class MrecAdBehaviour : MonoBehaviour
    {
        private const string TAG = "MrecBehaviour";
        private static readonly Vector2 MrecAdSize = new Vector2(300, 250);
        private static readonly Vector2 MediumDeviceSize = new Vector2(360, 780);

        [SerializeField]
        private string adPlacement;
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private ContentSizeFitter contentSizeFitter;
        [SerializeField]
        private LayoutElement layoutElement;
        [SerializeField]
        private Text textComponent;
        private Canvas _canvas;
        private string _displayedAdPlacement;
        private Vector2 _nativeScreenResolution; 
        
        private void Awake()
        {
            adPlacement = adPlacement.Trim();
            SetupSize();
            UpdateText();

            if (!Application.isPlaying) {
                return;
            }

            GetComponent<Image>().enabled = PlatformUtils.UNITY_EDITOR;
            textComponent.gameObject.SetActive(PlatformUtils.UNITY_EDITOR);


            if (string.IsNullOrEmpty(adPlacement)) {
                VoodooLog.LogError(Module.ADS, TAG, "The adPlacement of this Mrec is empty. This ad isn't working.");
            }
            
        }

        private Canvas ParentCanvas()
        {
            if (_canvas == null) {
                _canvas = rectTransform.GetComponentInParent<Canvas>();
            }

            return _canvas;
        }

        private void SetupSize()
        {
            Vector2 mrecSize = GetMrecPlaceholderSize();
            layoutElement.preferredWidth = mrecSize.x;
            layoutElement.preferredHeight = mrecSize.y;

            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UpdateText();
        }

        internal static Vector2 GetMrecPlaceholderSize() 
        {
            float width = Screen.width / MediumDeviceSize.x * MrecAdSize.x;
            float height = Screen.height / MediumDeviceSize.y * MrecAdSize.y;

            return new Vector2(width, height);
        }

        private void UpdateText()
        {
            _displayedAdPlacement = adPlacement;
            if (Application.isPlaying && !AdsManager.Mrec.IsEnabled()) {
                textComponent.text = "Mrec:\nDisabled.";
            } else if (adPlacement.Length == 0) {
                textComponent.text = "Mrec\nError: AdPlacement is empty.";
            } else {
                textComponent.text = "Mrec:\n" + adPlacement;
            }
        }

        private void OnRenderObject()
        {
            if (!Application.isPlaying) {
                SetupSize();
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
            if (!AdsManager.Mrec.IsEnabled() || gameObject == null || !gameObject.activeInHierarchy) {
                return;
            }

            if (AdsManager.AreFakeAdsEnabled()) {
                Vector3 position = rectTransform.position;
                AdsManager.Mrec.Show(position.x, position.y, adPlacement);
                return;
            }

            _canvas = ParentCanvas();
            Vector3 center = rectTransform.position;
            if (_canvas.renderMode == RenderMode.WorldSpace || _canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                center = _canvas.worldCamera.WorldToScreenPoint(center);
            }

            Vector2 resolutionChangeRatio = GetResolutionScaledToOriginalRatio();
            
            // origin is bottom left in Android and iOS vs top left in editor.
            //so we need to invert y 
            AdsManager.Mrec.Show(center.x * resolutionChangeRatio.x,
                (Screen.height - center.y) * resolutionChangeRatio.y, adPlacement);
        }

        private Vector2 GetResolutionScaledToOriginalRatio()
        {
            Resolution res = Screen.currentResolution;
            if (res.height == 0 || res.width == 0)
                return new Vector2(1, 1);
            
            
            FetchAndCacheNativeScreenSize();
            
            if (_nativeScreenResolution.x == 0 || _nativeScreenResolution.y == 0) 
                return new Vector2(1, 1);
            
            return new Vector2(_nativeScreenResolution.x / res.width, _nativeScreenResolution.y / res.height);
        }
        
        private void FetchAndCacheNativeScreenSize()
        {
            if (_nativeScreenResolution.x != 0 && _nativeScreenResolution.y != 0)
                return;
            int w = Display.main.systemWidth;
            int h = Display.main.systemHeight;
            _nativeScreenResolution = new Vector2(w, h);
        }
        
        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            VoodooSauce.UnSubscribeOnInitFinishedEvent(ShowAfterVoodooSauceInit);
            if (AdsManager.Mrec.IsEnabled()) {
                AdsManager.Mrec.Hide();
            }
        }
    }
}