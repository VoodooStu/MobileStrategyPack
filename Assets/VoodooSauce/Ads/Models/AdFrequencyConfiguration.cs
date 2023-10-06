using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    [Serializable]
    public class AdFrequencyConfiguration
    {
        public int delayInSecondsBeforeFirstInterstitialAd = 30;
        public int delayInSecondsBeforeSessionFirstInterstitialAd = -1;
        public int delayInSecondsBetweenInterstitialAds = 30;
        public int maxGamesBetweenInterstitialAds = 3;
        public int delayInSecondsBetweenRewardedVideoAndInterstitial = 5;
        public bool enableReplaceRewardedIfInterstitialCpmHigher = false;
        public bool enableReplaceRewardedIfNotLoaded = false;
        
        public override string ToString() => "- delay before first interstitial at the first-ever session: " + delayInSecondsBeforeFirstInterstitialAd +
            "- delay before session's first interstitial: " + delayInSecondsBeforeSessionFirstInterstitialAd +
            "\n- delay between interstitials: " + delayInSecondsBetweenInterstitialAds +
            "\n- max games between interstitials: " + maxGamesBetweenInterstitialAds +
            "\n- delay between rewarded video and interstitial: " + delayInSecondsBetweenRewardedVideoAndInterstitial +
            "\n- Replace rewarded with interstitial if FS CPM higher: " + enableReplaceRewardedIfInterstitialCpmHigher +
            "\n- Replace rewarded with interstitial if RV is not loaded: " + enableReplaceRewardedIfNotLoaded;
    }
}