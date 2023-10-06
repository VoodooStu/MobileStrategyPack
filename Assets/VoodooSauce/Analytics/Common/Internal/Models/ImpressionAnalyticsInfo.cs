// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class ImpressionAnalyticsInfo : AdAnalyticsInfo
    {
        public string Id;
        public string Precision;
        public string AdUnitId;
        public ImpressionAdUnitFormat? AdUnitFormat;
        public string Currency;
        public double? Revenue;
        public string NetworkPlacementId;
        public string Country;
        public int? AdCount;
        public string AppVersion;
    }

    public class MaxImpressionAnalyticsInfo : ImpressionAnalyticsInfo { }

    public class FakeImpressionAnalyticsInfo : ImpressionAnalyticsInfo { }
    
    public class IronSourceImpressionAnalyticsInfo : ImpressionAnalyticsInfo
    {
        public string Ab;
        public string SegmentName;
        public string InstanceName;
        public string InstanceId;
        public double? LifetimeRevenue;
        public string EncryptedCPM;
        public int? ConversionValue;
        public string JsonRepresentation;
    }

    public enum ImpressionAdUnitFormat
    {
        RewardedVideo,
        Interstitial,
        Banner,
        Mrec,
        NativeAds,
        AppOpen
    }

    public enum ImpressionInfoType
    {
        Fake,
        MaxAds,
        IronSource
    }
}