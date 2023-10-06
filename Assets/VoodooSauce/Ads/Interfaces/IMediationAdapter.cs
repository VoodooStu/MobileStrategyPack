using System;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public enum MediationType
    {
        Fake,
        MaxAds,
        IronSource
    }

    public interface IMediationAdapter
    {
        // Name of the mediation
        MediationType GetMediationType();
        void Initialize(AdsKeys keys, bool hasPaidToHideAds, bool consent, bool isCcpaApplicable, Action sdkInitialized);
        
        bool IsSdkInitialized();
        void SetConsent(bool consent, bool isCcpaApplicable);
        void SetAdUnit(AdUnitType type, string adUnit);

        VoodooAdInfo GetBannerInfo();
        VoodooAdInfo GetInterstitialInfo();
        VoodooAdInfo GetRewardedVideoInfo();
        VoodooAdInfo GetAppOpenInfo();
        VoodooAdInfo GetMrecInfo();
        VoodooAdInfo GetNativeAdsInfo();

        void InitializeBanner();
        void ShowBanner();
        void HideBanner();
        void DestroyBanner();
        float GetNativeBannerHeight();
        float GetScreenDensity();
        
        /// <summary>
        /// Each adapter should display a white background behind the banner. It's a google requirement.
        /// If the mediation doesn't have any method for that, <see cref="BannerBackground"/> can be used.
        /// </summary>
        void SetBannerBackgroundVisibility(bool visibility,Color color);

        void InitializeInterstitial();
        void ShowInterstitial(bool isInterstitialShownInsteadOfRewarded = false);
        
        void InitializeRewardedVideo();
        void ShowRewardedVideo();
        
        void InitializeAppOpen();
        void ShowAppOpen();
        
        void InitializeMrec();
        void ShowMrec(float x, float y);
        void HideMrec();
        void DestroyMrec();
        
        void InitializeNativeAds();
        void ShowNativeAd(string layoutName, float x, float y, float width, float height);
        void HideNativeAd();

        bool HasDebugger();
        void ShowDebugger();
        
        void OnApplicationPause(bool pauseStatus);

        bool EnableAmazonTesting();
    }
}