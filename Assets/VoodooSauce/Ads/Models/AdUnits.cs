using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Ads
{
    [Serializable]
    public class AdUnits
    {
        [Tooltip("Leave blank if you don't want banner ads in your game")]
        public string bannerAdUnit;

        [Tooltip("Leave blank if you don't want interstitial ads in your game")]
        public string interstitialAdUnit;

        [Tooltip("Leave blank if you don't want rewarded video ads in your game")]
        public string rewardedVideoAdUnit;

        [Tooltip("Leave blank if you don't want Mrec ads in your game")]
        public string mrecAdUnit;
        
        [Tooltip("Leave blank if you don't want Native ads in your game")]
        public string nativeAdsAdUnit;

        [Tooltip("Leave blank if you don't want AppOpen ads in your game")]
        public string appOpenAdUnit;

        public enum AdUnit
        {
            Banner,
            Interstitial,
            RewardedVideo,
            Mrec,
            NativeAds,
            AppOpen
        }

        public AdUnits()
        {
            bannerAdUnit = "";
            interstitialAdUnit = "";
            rewardedVideoAdUnit = "";
            mrecAdUnit = "";
            nativeAdsAdUnit = "";
            appOpenAdUnit = "";
        }

        public string[] ExportToStringList(bool isPremium)
        {
            var result = new List<string>();
            if(!string.IsNullOrEmpty(bannerAdUnit))
                result.Add(bannerAdUnit);
            if(!string.IsNullOrEmpty(interstitialAdUnit))
                result.Add(interstitialAdUnit);
            if(!string.IsNullOrEmpty(rewardedVideoAdUnit))
                result.Add(rewardedVideoAdUnit);
            if(!string.IsNullOrEmpty(mrecAdUnit))
                result.Add(mrecAdUnit);
            if(!string.IsNullOrEmpty(nativeAdsAdUnit))
                result.Add(nativeAdsAdUnit);
            if(!string.IsNullOrEmpty(appOpenAdUnit) && !isPremium)
                result.Add(appOpenAdUnit);
            return result.ToArray();
        }

        public bool IsEmpty() => string.IsNullOrEmpty(bannerAdUnit) &&
                                 string.IsNullOrEmpty(interstitialAdUnit) &&
                                 string.IsNullOrEmpty(rewardedVideoAdUnit);

        public override bool Equals(object obj)
        {
            var other = (AdUnits) obj;

            return other != null &&
                string.Equals(other.bannerAdUnit, bannerAdUnit) &&
                string.Equals(other.interstitialAdUnit, interstitialAdUnit) &&
                string.Equals(other.rewardedVideoAdUnit, rewardedVideoAdUnit) &&
                string.Equals(other.mrecAdUnit, mrecAdUnit) &&
                string.Equals(other.nativeAdsAdUnit, nativeAdsAdUnit) &&
                string.Equals(other.appOpenAdUnit, appOpenAdUnit);
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = (bannerAdUnit != null ? bannerAdUnit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (interstitialAdUnit != null ? interstitialAdUnit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (rewardedVideoAdUnit != null ? rewardedVideoAdUnit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (mrecAdUnit != null ? mrecAdUnit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (nativeAdsAdUnit != null ? nativeAdsAdUnit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (appOpenAdUnit != null ? appOpenAdUnit.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() => $"BannerAdUnit: {bannerAdUnit} \n " + $"InterstitialAdUnit: {interstitialAdUnit} \n"
            + $" RewardedVideoAdUnit: {rewardedVideoAdUnit} \n" + $" MrecAdUnit: {mrecAdUnit} \n" + $" NativeAdsAdUnit: {nativeAdsAdUnit} \n"
            + $" AppOpenAdUnit: {appOpenAdUnit} \n";
    }
}