using System;
using UnityEngine;
using Voodoo.Sauce.Internal.Analytics;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    public class FakeMediationAdapter : IMediationAdapter
    {
#region Constants
        
        private const string FAKE_VALUE = "fake";

        private const double INTERSTITIAL_AD_REVENUE = 0.0000000005;
        private const double NATIVE_AD_REVENUE = 0.0000000006;
        private const double MREC_AD_REVENUE = 0.00000000008;
        private const double REWARDED_AD_REVENUE = 0.000000007;
        private const double BANNER_AD_REVENUE = 0.000000000003;
        private const double APP_OPEN_AD_REVENUE = 0.000000000004;

#endregion

        internal static GameObject fakeAdsPrefab;
        private FakeMediationBehaviour _fakeAds;
        
        private AdsKeys _keys;
        private bool _sdkInitialized;
        private bool _bannerInitialized;
        private bool _interstitialInitialized;
        private bool _rewardedVideoInitialized;
        private bool _appOpenInitialized;
        private bool _mrecInitialized;
        private bool _nativeAdInitialized;

        public MediationType GetMediationType() => MediationType.Fake;

        public void Initialize(AdsKeys keys, bool hasPaidToHideAds, bool consent, bool isCcpaApplicable, Action sdkInitialized)
        {
            if (!keys.enableFakeAds) return;
            _keys = keys;
            _fakeAds = Object.Instantiate(fakeAdsPrefab).GetComponent<FakeMediationBehaviour>();
            _sdkInitialized = true;
            sdkInitialized?.Invoke();
        }

        public bool IsSdkInitialized() => _sdkInitialized;
        public void SetConsent(bool consent, bool isCcpaApplicable) { }
        public void SetAdUnit(AdUnitType type, string adUnit) { }
        public bool HasDebugger() => false;
        public void ShowDebugger() { }
        public void OnApplicationPause(bool pauseStatus) { }

        public VoodooAdInfo GetBannerInfo() => BuildFakeAdInfo(BANNER_AD_REVENUE);
        public VoodooAdInfo GetInterstitialInfo() => BuildFakeAdInfo(INTERSTITIAL_AD_REVENUE);
        public VoodooAdInfo GetRewardedVideoInfo() => BuildFakeAdInfo(REWARDED_AD_REVENUE);
        public VoodooAdInfo GetAppOpenInfo() => BuildFakeAdInfo(APP_OPEN_AD_REVENUE);

        public VoodooAdInfo GetMrecInfo() => BuildFakeAdInfo(MREC_AD_REVENUE);
        public VoodooAdInfo GetNativeAdsInfo() => BuildFakeAdInfo(NATIVE_AD_REVENUE);

#region Banner
        private bool IsBannerEnabled() => _keys?.enableFakeAds == true;

        public float GetNativeBannerHeight()
        {
            var bannerHeightForItsCanvas = 160;
            // The 1920 comes from the canvas that is used for the fake banner
            var bannerHeightPixelForGivenDevice = bannerHeightForItsCanvas * (Screen.height / 1920f); 
            return bannerHeightPixelForGivenDevice;
        }

        public float GetScreenDensity() => 1f;

        public void InitializeBanner()
        {
            if (!IsBannerEnabled()) {
                AdsManager.Banner.OnInitialized(false);
                return;
            }

            if (!_bannerInitialized) {
                _bannerInitialized = true;
                _fakeAds.Callbacks.OnBannerAdLoadedEvent += OnBannerAdLoadedEvent;
                _fakeAds.Callbacks.OnBannerAdClickedEvent += OnBannerAdClickedEvent;
                _fakeAds.Callbacks.OnBannerAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            }

            AdsManager.Banner.OnInitialized(true);
            _fakeAds.CreateBanner();
        }

        public void ShowBanner()
        {
            if (!_bannerInitialized) return;
            _fakeAds.ShowBanner();
        }

        public void HideBanner()
        {
            if (!_bannerInitialized) return;
            _fakeAds.HideBanner();
            BannerBackground.Hide();
        }

        public void DestroyBanner() { }

        public void SetBannerBackgroundVisibility(bool visibility, Color color)
        {
            if (!_bannerInitialized) return;
            if (visibility) {
                BannerBackground.Show(color);
            } else {
                BannerBackground.Hide();
            }
        }
        
        private static void OnBannerAdLoadedEvent()
        {
            AdsManager.Banner.OnLoadSuccess();
        }

        private static void OnBannerAdClickedEvent()
        {
            AdsManager.Banner.OnClicked();
        }
        
        private static void OnBannerAdRevenuePaidEvent()
        {
            AdsManager.Banner.OnImpression(ImpressionInfoType.Fake);
        }
#endregion
        
        
#region Interstitial
        private bool IsInterstitialEnabled() => _keys?.enableFakeAds == true && _keys.enableFakeInterstitialAds;

        public void InitializeInterstitial()
        {
            if (!IsInterstitialEnabled()) {
                AdsManager.Interstitial.OnInitialized(false);
                return;
            }

            if (!_interstitialInitialized) {
                _interstitialInitialized = true;
                _fakeAds.Callbacks.OnInterstitialAdLoadedEvent += OnInterstitialAdLoadedEvent;
                _fakeAds.Callbacks.OnInterstitialAdClickedEvent += OnInterstitialAdClickedEvent;
                _fakeAds.Callbacks.OnInterstitialAdHiddenEvent += OnInterstitialAdHiddenEvent;
                _fakeAds.Callbacks.OnInterstitialAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;
            }

            AdsManager.Interstitial.OnInitialized(true);
            //load first Interstitial
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            if (!_interstitialInitialized) return;
            AdsManager.Interstitial.OnLoading();
            _fakeAds.LoadInterstitial();
        }

        public void ShowInterstitial(bool isInterstitialShownInsteadOfRewarded = false)
        {
            if (!_interstitialInitialized) return;
            _fakeAds.ShowInterstitial();
        }
        
        private static void OnInterstitialAdLoadedEvent()
        {
            AdsManager.Interstitial.OnLoadSuccess();
        }
        
        private static void OnInterstitialAdClickedEvent()
        {
            AdsManager.Interstitial.OnClicked();
        }
        
        private void OnInterstitialAdHiddenEvent()
        {
            AdsManager.Interstitial.OnDismissed();
            LoadInterstitial();
        }
        
        private static void OnInterstitialAdRevenuePaidEvent()
        {
            AdsManager.Interstitial.OnImpression(ImpressionInfoType.Fake);
        }
#endregion
        
        
#region AppOpen
        private bool IsAppOpenEnabled() => _keys?.enableFakeAds == true && _keys.enableFakeAppOpenAds;

        public void InitializeAppOpen()
        {
            if (!IsAppOpenEnabled()) {
                AdsManager.AppOpen.OnInitialized(false);
                return;
            }

            if (!_appOpenInitialized) {
                _appOpenInitialized = true;
                _fakeAds.Callbacks.OnAppOpenAdLoadedEvent += OnAppOpenAdLoadedEvent;
                _fakeAds.Callbacks.OnAppOpenAdClickedEvent += OnAppOpenAdClickedEvent;
                _fakeAds.Callbacks.OnAppOpenAdHiddenEvent += OnAppOpenAdHiddenEvent;
                _fakeAds.Callbacks.OnAppOpenAdRevenuePaidEvent += OnAppOpenAdRevenuePaidEvent;
            }

            AdsManager.AppOpen.OnInitialized(true);
            
            // Load first AppOpen
            LoadAppOpen();
        }

        private void LoadAppOpen()
        {
            if (!_appOpenInitialized) return;
            AdsManager.AppOpen.OnLoading();
            _fakeAds.LoadAppOpen();
        }

        public void ShowAppOpen()
        {
            if (!_appOpenInitialized) return;
            _fakeAds.ShowAppOpen();
        }
        
        private static void OnAppOpenAdLoadedEvent()
        {
            AdsManager.AppOpen.OnLoadSuccess();
        }
        
        private static void OnAppOpenAdClickedEvent()
        {
            AdsManager.AppOpen.OnClicked();
        }
        
        private void OnAppOpenAdHiddenEvent()
        {
            AdsManager.AppOpen.OnDismissed();
            LoadAppOpen();
        }
        
        private static void OnAppOpenAdRevenuePaidEvent()
        {
            AdsManager.AppOpen.OnImpression(ImpressionInfoType.Fake);
        }
#endregion
        
        
#region RewardedVideo      
        private bool IsRewardedVideoEnabled() => _keys?.enableFakeAds == true && _keys.enableFakeRewardedVideoAds;

        public void InitializeRewardedVideo()
        {
            if (!IsRewardedVideoEnabled()) {
                AdsManager.RewardedVideo.OnInitialized(false);
                return;
            }

            if (!_rewardedVideoInitialized) {
                _rewardedVideoInitialized = true;
                _fakeAds.Callbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
                _fakeAds.Callbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
                _fakeAds.Callbacks.OnRewardedAdHiddenEvent += OnRewardedAdHiddenEvent;
                _fakeAds.Callbacks.OnRewardedAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            }
            //load first RewardedVideo
            AdsManager.RewardedVideo.OnInitialized(true);
            LoadRewardedVideo();
        }
        
        private void LoadRewardedVideo()
        {
            if (!_rewardedVideoInitialized) return;
            AdsManager.RewardedVideo.OnLoading();
            _fakeAds.LoadRewarded();
        }

        public void ShowRewardedVideo()
        {
            if (!_rewardedVideoInitialized) return;
            _fakeAds.ShowRewarded();
        }

        private static void OnRewardedAdLoadedEvent()
        {
            AdsManager.RewardedVideo.OnLoadSuccess();
        }
        private static void OnRewardedAdClickedEvent()
        {
            AdsManager.RewardedVideo.OnClicked();
        }

        private void OnRewardedAdHiddenEvent(bool watched)
        {
            AdsManager.RewardedVideo.OnDismissed(watched);
            LoadRewardedVideo();
        }
        
        private static void OnRewardedAdRevenuePaidEvent()
        {
            AdsManager.RewardedVideo.OnImpression(ImpressionInfoType.Fake);
        }
#endregion
        
        
#region Mrec
        public void InitializeMrec()
        {
            if (!_mrecInitialized) {
                _mrecInitialized = true;
                _fakeAds.Callbacks.OnMRecAdLoadedEvent += OnMRecAdLoadedEvent;
                _fakeAds.Callbacks.OnMRecAdClickedEvent += OnMRecAdClickedEvent;
                _fakeAds.Callbacks.OnMRecAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
            }
            AdsManager.Mrec.OnInitialized(true);
            _fakeAds.CreateMRec();
        }

        public void ShowMrec(float x, float y)
        {
            if (!_mrecInitialized) return;
            _fakeAds.ShowMrec(x,y);
        }

        public void HideMrec()
        {
            if (!_mrecInitialized) return;
            _fakeAds.HideMrec();
        }

        public void DestroyMrec() { }
        
        private static void OnMRecAdLoadedEvent()
        {
            AdsManager.Mrec.OnLoadSuccess();
        }

        private static void OnMRecAdClickedEvent()
        {
            AdsManager.Mrec.OnClicked();
        }

        private static void OnMRecAdRevenuePaidEvent()
        {
            AdsManager.Mrec.OnImpression(ImpressionInfoType.Fake);
        }
#endregion   
        
        
#region NativeAds
        public void InitializeNativeAds()
        {
            if (!_nativeAdInitialized) {
                _nativeAdInitialized = true;
                _fakeAds.Callbacks.OnNativeAdLoadedEvent += OnNativeAdLoadedEvent;
                _fakeAds.Callbacks.OnNativeAdDisplayedEvent += OnNativeAdDisplayedEvent;
                _fakeAds.Callbacks.OnNativeAdClickedEvent += OnNativeAdClickedEvent;
                _fakeAds.Callbacks.OnNativeAdHiddenEvent += OnNativeAdHiddenEvent;
                _fakeAds.Callbacks.OnNativeAdRevenuePaidEvent += OnNativeAdRevenuePaidEvent;
            }
            AdsManager.NativeAds.OnInitialized(true);
            //load first NativeAds
            LoadNativeAds();
        }

        private  void LoadNativeAds()
        {
            AdsManager.NativeAds.OnLoading();
            _fakeAds.LoadNativeAd();
        }

        public void ShowNativeAd(string layoutName, float x, float y, float width, float height)
        {
            if (!_nativeAdInitialized) return;
             _fakeAds.ShowNativeAd(layoutName, x, y, width, height);

        }

        public void HideNativeAd()
        {
            if (!_nativeAdInitialized) return;
            _fakeAds.HideNative();
        }
        
        private static void OnNativeAdLoadedEvent()
        {
            AdsManager.NativeAds.OnLoadSuccess();
        }

        private static void OnNativeAdDisplayedEvent()
        {
            AdsManager.NativeAds.OnShown();
        }

        private static void OnNativeAdClickedEvent()
        {
            AdsManager.NativeAds.OnClicked();
        }

        private static void OnNativeAdHiddenEvent()
        {
            AdsManager.NativeAds.OnDismissed();
        }

        private static void OnNativeAdRevenuePaidEvent()
        {
            AdsManager.NativeAds.OnImpression(ImpressionInfoType.Fake);
        }
#endregion

#region Amazon Ads

        public bool EnableAmazonTesting() => false;

#endregion
        
#region Private Methods
        
        private static VoodooAdInfo BuildFakeAdInfo(double adRevenue) => new VoodooAdInfo {
            Id = Guid.NewGuid().ToString(),
            AdUnit = FAKE_VALUE,
            AdNetworkName =  FAKE_VALUE,
            Creative = FAKE_VALUE,
            Revenue = adRevenue,
            Country = FAKE_VALUE,
            WaterfallTestName = FAKE_VALUE,
            AdNetworkPlacementId = FAKE_VALUE,
        };

#endregion
    }
}