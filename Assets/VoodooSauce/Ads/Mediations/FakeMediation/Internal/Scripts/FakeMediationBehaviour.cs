using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    public class FakeMediationBehaviour : MonoBehaviour
    {
        public FakeMediationCallbacks Callbacks { get; private set; }

        // Banner 
        [FormerlySerializedAs("_bannerVideoAd"), SerializeField]
        private FakeBannerAd bannerAd;

        // Interstitial
        [FormerlySerializedAs("_interstitialAd"), SerializeField]
        private FakeInterstitialAd interstitialAd;

        // Rewarded
        [FormerlySerializedAs("_rewardedAd"), SerializeField]
        private FakeRewardedVideoAd rewardedAd;

        // AppOpen
        [SerializeField]
        private FakeAppOpenAd appOpenAd;

        // NativeAds
        [SerializeField]
        private FakeNativeAd nativeAd;

        // Mrec ad
        [SerializeField]
        private FakeMrecAd mrecAd;

        private float _prevTimeScale;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Callbacks = new FakeMediationCallbacks();
            // Click Events
            interstitialAd.onClick += () => Callbacks.InvokeOnInterstitialAdClicked();
            rewardedAd.onClick += () => Callbacks.InvokeOnRewardedAdClicked();
            appOpenAd.onClick += () => Callbacks.InvokeOnAppOpenAdClicked();
            bannerAd.onClick += () => Callbacks.InvokeOnBannerAdClicked();
            mrecAd.onClick += () => Callbacks.InvokeOnMRecAdClicked();
            nativeAd.onClick += () => Callbacks.InvokeOnNativeAdClicked();

            // Close the interstitial
            interstitialAd.onClose += () => {
                ResumeTime();
                Callbacks.InvokeOnInterstitialAdHidden();
            };

            // Close the RV
            rewardedAd.onCloseWithReward += () => {
                ResumeTime();
                Callbacks.InvokeOnRewardedAdHidden(true);
            };

            rewardedAd.onCloseWithoutReward += () => {
                ResumeTime();
                Callbacks.InvokeOnRewardedAdHidden(false);
            };

            // Close the app open
            appOpenAd.onClose += () => {
                ResumeTime();
                Callbacks.InvokeOnAppOpenAdHidden();
            };
        }

        public void CreateBanner()
        {
            Callbacks.InvokeOnBannerAdLoaded();
        }
        public void ShowBanner()
        {
            bannerAd.StartAd();
            Callbacks.InvokeOnBannerAdRevenuePaid();
        }

        public void HideBanner()
        {
            bannerAd.StopAd();
        }

        public void LoadInterstitial()
        {
            Callbacks.InvokeOnInterstitialAdLoaded();
        }
        public void ShowInterstitial()
        {
            PauseTime();
            interstitialAd.StartAd();
            Callbacks.InvokeOnInterstitialAdRevenuePaid();
        }
        
        public void LoadRewarded()
        {
            Callbacks.InvokeOnRewardedAdLoaded();
        }
        public void ShowRewarded()
        {
            PauseTime();
            rewardedAd.StartAd();
            Callbacks.InvokeOnRewardedAdRevenuePaid();
        }

        public void LoadAppOpen()
        {
            Callbacks.InvokeOnAppOpenAdLoaded();
        }
        public void ShowAppOpen()
        {
            PauseTime();
            appOpenAd.StartAd();
            Callbacks.InvokeOnAppOpenAdRevenuePaid();
        }

        public void LoadNativeAd()
        {
            Callbacks.InvokeOnNativeAdLoaded();
        }
        public void ShowNativeAd(string layoutName, float x, float y, float width, float height) 
        {
            nativeAd.Setup(layoutName, x, y, width, height);
            nativeAd.StartAd();
            Callbacks.InvokeOnNativeAdDisplayed();
            Callbacks.InvokeOnNativeAdRevenuePaid();
        }

        public void HideNative()
        {
            nativeAd.StopAd();
            Callbacks.InvokeOnNativeAdHidden();
        }

        public void CreateMRec()
        {
            Callbacks.InvokeOnMRecAdLoaded();
        }

        public void ShowMrec(float x, float y)
        {
            mrecAd.Setup(x, y);
            mrecAd.StartAd();
            Callbacks.InvokeOnMRecAdRevenuePaid();
        }

        public void HideMrec()
        {
            mrecAd.StopAd();
        }
        
        
        private void PauseTime()
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        private void ResumeTime()
        {
            Time.timeScale = _prevTimeScale;
        }

    }
}