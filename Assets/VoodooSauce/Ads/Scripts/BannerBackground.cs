using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public class BannerBackground : MonoBehaviour
    {
        private const int IOS_BANNER_HEIGHT = 150;
        private const int ANDROID_BANNER_HEIGHT = 168;
        
        private int _bannerHeight = 0;
        
        private static GameObject _instance;
        private static BannerBackground _bannerBackground;
        
        public Image background;
        public GameObject purchasePopup;
        public Button leftCloseButton;
        public Button rightCloseButton;
        public Button purchaseButton;
        public Button cancelButton;
        public Text bodyText;
        public Text purchaseButtonText;
        public Text cancelButtonText;
        private static Vector2 _nativeScreenResolution;
        private static bool _enableAutomaticBannerHeightAdjustment = false;
        private static RectTransform _closeButtonRectTransform;
        private void Awake()
        {
            if (_instance != null) {
                Destroy(gameObject);
            }

            if (_bannerHeight == 0) {
                _bannerHeight = PlatformUtils.UNITY_IOS ? IOS_BANNER_HEIGHT : ANDROID_BANNER_HEIGHT;
            }
            
        }

        internal static void Hide()
        {
            if (_instance != null)
            {
                _instance.SetActive(false);
            }
        }

        internal static void Show(Color color)
        {
            if (_instance == null) {
                Initialize(color);
            }
            _instance.SetActive(true);
        }

        internal static void UpdateHeight(float height, float screenDensity)
        {
            if (!_enableAutomaticBannerHeightAdjustment) return;
            if (height <= 0f) return;
            Vector2 resolutionRatio = ScreenSizeUtils.GetResolutionNativeToUnityRatio();
            _bannerBackground._bannerHeight = (int)(height * resolutionRatio.y * screenDensity);
            if (PlatformUtils.UNITY_IOS) {
                _bannerBackground._bannerHeight += (int)Screen.safeArea.position.y;
            }
            _bannerBackground.background.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, _bannerBackground._bannerHeight);
            AdaptCloseButtonSizeToRatio(resolutionRatio);
        }

        private static void Initialize(Color color)
        {
            _instance = VoodooSauceBehaviour.InstantiateBannerBackgroundPrefab();
            VoodooSettings voodooSettings = VoodooSettings.Load();
            _enableAutomaticBannerHeightAdjustment = voodooSettings.EnableAutomaticBannerHeightAdjustment;
            Transform parent = _instance.transform.parent;
            DontDestroyOnLoad(parent != null ? parent.gameObject : _instance);
            _bannerBackground = _instance.GetComponent<BannerBackground>();
            _bannerBackground.SetBannerBackground(color);
            if (string.IsNullOrEmpty(voodooSettings.NoAdsBundleId) || !voodooSettings.UseRemoteConfig && !voodooSettings.BannerCloseButtonEnabled
                || voodooSettings.UseRemoteConfig && !VoodooSauce.GetItemOrDefault<BannerConfiguration>().bannerCloseButtonEnabled) {
                _bannerBackground.leftCloseButton.gameObject.SetActive(false);
                _bannerBackground.rightCloseButton.gameObject.SetActive(false);
                return;
            }
            VoodooSettings.Position position = voodooSettings.BannerCloseButtonPosition;
            if (PlatformUtils.UNITY_IOS && DeviceUtils.HasNotch()) {
                if (Input.deviceOrientation == DeviceOrientation.LandscapeRight) {
                    position = VoodooSettings.Position.Left;
                } else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft) {
                    position = VoodooSettings.Position.Right;
                }
            }
            _bannerBackground.SetCloseButton(position, voodooSettings.BannerCloseButtonSprite);
            _bannerBackground.purchaseButton.onClick.AddListener(() => _bannerBackground.OnPurchaseButtonClicked(voodooSettings));
            _bannerBackground.cancelButton.onClick.AddListener(_bannerBackground.OnCancelButtonClick);
        }

        private void OnPurchaseButtonClicked(VoodooSettings voodooSettings)
        {
            Action onPurchaseComplete = OnPurchaseComplete;
            VoodooSauce.RegisterPurchaseDelegate(new NoAdsPurchaseDelegate(onPurchaseComplete, null));
            VoodooSauce.Purchase(voodooSettings.NoAdsBundleId);
            AnalyticsManager.TrackCloseBannerPurchase();
        }

        private void OnPurchaseComplete()
        {
            VoodooSauce.EnablePremium();
            purchasePopup.gameObject.SetActive(false);
        }

        private void SetBannerBackground(Color color)
        {
            background.color = color;
            int height = ANDROID_BANNER_HEIGHT;
            if (PlatformUtils.UNITY_IOS) {
                height = IOS_BANNER_HEIGHT + (int)Screen.safeArea.position.y;
            }
            background.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, height);
        }

        private static void AdaptCloseButtonSizeToRatio(Vector2 ratio)
        {
            if (_closeButtonRectTransform == null) return;
            var scaleRatio = new Vector3(ratio.x, ratio.y, 1);
            _closeButtonRectTransform.localScale = scaleRatio;
            
            Vector3 position = _closeButtonRectTransform.anchoredPosition;
            _closeButtonRectTransform.anchoredPosition =
                new Vector3(position.x, _closeButtonRectTransform.rect.height * 0.5f * scaleRatio.y);
        }
        
        private void SetCloseButton(VoodooSettings.Position position, Sprite sprite)
        {
            Button usedButton = position == VoodooSettings.Position.Left ? leftCloseButton : rightCloseButton;
            usedButton.onClick.AddListener(OnCloseButtonClick);
            if (sprite != null) usedButton.image.sprite = sprite;
            leftCloseButton.gameObject.SetActive(usedButton == leftCloseButton);
            rightCloseButton.gameObject.SetActive(usedButton == rightCloseButton);
            _closeButtonRectTransform = position == VoodooSettings.Position.Left
                ? leftCloseButton.GetComponent<RectTransform>()
                : rightCloseButton.GetComponent<RectTransform>();
        }

        private void OnCloseButtonClick()
        {
            bodyText.text = BannerPopupLocalisation.Body;
            cancelButtonText.text = BannerPopupLocalisation.CancelButtonText;
            purchaseButtonText.text = BannerPopupLocalisation.PurchaseButtonText;
            purchasePopup.gameObject.SetActive(true);
            AnalyticsManager.TrackCloseBannerClick();
        }
        
        private void OnCancelButtonClick()
        {
            purchasePopup.gameObject.SetActive(false);
            AnalyticsManager.TrackCloseBannerCancel();
        }
    }
}