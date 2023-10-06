using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    
    public  class AdAnalyticsInfo
    {
        [CanBeNull]
        public string WaterfallTestName;
        [CanBeNull]
        public string WaterfallName;
        [CanBeNull]
        public string AdTag;
        public int? AdLoadingTime;
        [CanBeNull]
        public string AdNetworkName;
        [CanBeNull]
        public string Creative;
        public bool? IsFsShownInsteadOfRv;
        [CanBeNull]
        public string ReasonFsShownInsteadOfRv;
        [CanBeNull]
        public string ImpressionId;
    }

    public class AudioAdAnalyticsInfo
    {
        public string networkName;
        public string impressionId;
        public int adCount;
        public int gameCount;
        public string audioAdType;
    }

    public class AudioAdFailedAnalyticsInfo : AudioAdAnalyticsInfo
    {
        public string errorCode;
    }
    
    public class AudioAdTriggerAnalyticsInfo : AudioAdAnalyticsInfo
    {
        public bool AdLoaded;
    }

    public class AudioAdImpressionAnalyticsInfo : AudioAdAnalyticsInfo
    {
        public string adUnitFormat;
        public double revenue;
    }
    
    public class AdEventAnalyticsInfo: AdAnalyticsInfo
    {
        [CanBeNull]
        public string AdUnit;
        public int? GameCount;
        public double? InterstitialCpm;
        public double? RewardedVideoCpm;
    }

    public class AdTriggeredEventAnalyticsInfo: AdEventAnalyticsInfo
    {
        public int AdCount;
        public bool AdLoaded;
    } 
    
    public class AdShownEventAnalyticsInfo: AdEventAnalyticsInfo
    {
        public int AdCount;
    }
    
    public class AdClickEventAnalyticsInfo: AdEventAnalyticsInfo
    {
        public int AdCount;
    }
    
    public class AdClosedEventAnalyticsInfo: AdEventAnalyticsInfo
    {
        public int AdCount;
        public int AdWatchedCount;
        public int AdDuration;
        public bool? AdReward;
    }
    
    public class RewardButtonShownEventAnalyticsInfo: AdEventAnalyticsInfo
    {
        public int AdCount;
        public bool AdLoaded;
    }

    public class AdShowFailedEventAnalyticsInfo : AdEventAnalyticsInfo
    {
        public int AdCount;
        public int AdDuration;
        public int? ErrorCode;
        [CanBeNull]
        public string ErrorMessage;
        public int? AdNetworkErrorCode;
        [CanBeNull]
        public string AdNetworkErrorString;
    }

}