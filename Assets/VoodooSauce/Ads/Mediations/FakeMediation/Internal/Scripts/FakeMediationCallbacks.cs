using System;

namespace Voodoo.Sauce.Internal.Ads.FakeMediation {
    public class FakeMediationCallbacks
    {
        public event Action OnInterstitialAdLoadedEvent;
        public event Action OnInterstitialAdClickedEvent;
        public event Action OnInterstitialAdHiddenEvent;
        public event Action OnInterstitialAdRevenuePaidEvent;

        public event Action OnRewardedAdLoadedEvent;
        public event Action OnRewardedAdClickedEvent;
        public event Action<bool> OnRewardedAdHiddenEvent;
        public event Action OnRewardedAdRevenuePaidEvent;

        public event Action OnAppOpenAdLoadedEvent;
        public event Action OnAppOpenAdClickedEvent;
        public event Action OnAppOpenAdHiddenEvent;
        public event Action OnAppOpenAdRevenuePaidEvent;
        
        public event Action OnBannerAdLoadedEvent;
        public event Action OnBannerAdClickedEvent;
        public event Action OnBannerAdRevenuePaidEvent;

        public event Action OnMRecAdLoadedEvent;
        public event Action OnMRecAdClickedEvent;
        public event Action OnMRecAdRevenuePaidEvent;

        public event Action OnNativeAdLoadedEvent;
        public event Action OnNativeAdDisplayedEvent;
        public event Action OnNativeAdClickedEvent;
        public event Action OnNativeAdRevenuePaidEvent;
        public event Action OnNativeAdHiddenEvent;


        public void InvokeOnInterstitialAdLoaded() => OnInterstitialAdLoadedEvent?.Invoke();
        public void InvokeOnInterstitialAdClicked() => OnInterstitialAdClickedEvent?.Invoke();
        public void InvokeOnInterstitialAdHidden() => OnInterstitialAdHiddenEvent?.Invoke();
        public void InvokeOnInterstitialAdRevenuePaid() => OnInterstitialAdRevenuePaidEvent?.Invoke();
        
        public void InvokeOnRewardedAdLoaded() => OnRewardedAdLoadedEvent?.Invoke();
        public void InvokeOnRewardedAdClicked() => OnRewardedAdClickedEvent?.Invoke();
        public void InvokeOnRewardedAdHidden(bool watched) => OnRewardedAdHiddenEvent?.Invoke(watched);
        public void InvokeOnRewardedAdRevenuePaid() => OnRewardedAdRevenuePaidEvent?.Invoke();

        public void InvokeOnAppOpenAdLoaded() => OnAppOpenAdLoadedEvent?.Invoke();
        public void InvokeOnAppOpenAdClicked() => OnAppOpenAdClickedEvent?.Invoke();
        public void InvokeOnAppOpenAdHidden() => OnAppOpenAdHiddenEvent?.Invoke();
        public void InvokeOnAppOpenAdRevenuePaid() => OnAppOpenAdRevenuePaidEvent?.Invoke();
        
        public void InvokeOnBannerAdLoaded() => OnBannerAdLoadedEvent?.Invoke();
        public void InvokeOnBannerAdClicked() => OnBannerAdClickedEvent?.Invoke();
        public void InvokeOnBannerAdRevenuePaid() => OnBannerAdRevenuePaidEvent?.Invoke();

        public void InvokeOnMRecAdLoaded() => OnMRecAdLoadedEvent?.Invoke();
        public void InvokeOnMRecAdClicked() => OnMRecAdClickedEvent?.Invoke();
        public void InvokeOnMRecAdRevenuePaid() => OnMRecAdRevenuePaidEvent?.Invoke();
        
        public void InvokeOnNativeAdLoaded() => OnNativeAdLoadedEvent?.Invoke();
        public void InvokeOnNativeAdDisplayed() => OnNativeAdDisplayedEvent?.Invoke();
        public void InvokeOnNativeAdClicked() => OnNativeAdClickedEvent?.Invoke();
        public void InvokeOnNativeAdHidden() => OnNativeAdHiddenEvent?.Invoke();
        public void InvokeOnNativeAdRevenuePaid() => OnNativeAdRevenuePaidEvent?.Invoke();
    }
}