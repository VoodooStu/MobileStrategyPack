using System;
using UnityEngine;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Internal.Ads
{
    public struct VoodooAdInfo
    {
        private const string USD_CURRENCY = "USD";
        public string Id;
        public string AdUnit;
        public string AdNetworkName;
        public string AdNetworkPlacementId;
        public double? Revenue;
        public string Country;
        public string WaterfallTestName;
        public string WaterfallName;
        public string Creative;
        public string Precision;
        
        // Added for IronSource
        public string Ab;
        public string SegmentName;
        public string InstanceId;
        public string InstanceName;
        public double? LifetimeRevenue;
        public int? ConversionValue;
        public string EncryptedCPM;
        public string JsonRepresentation;

        public ImpressionAnalyticsInfo ToInfoType(ImpressionInfoType type)
        {
            switch (type) {
                case ImpressionInfoType.Fake:
                    return ToFakeInfo();
                case ImpressionInfoType.MaxAds:
                    return ToMaxInfo();
                case ImpressionInfoType.IronSource:
                    return ToIronSourceInfo();
            }
            return ToFakeInfo();
        }

        private FakeImpressionAnalyticsInfo ToFakeInfo() => new FakeImpressionAnalyticsInfo {
            Id = Id,
            AdUnitId = AdUnit,
            Currency = USD_CURRENCY,
            Revenue = Revenue,
            AdNetworkName = AdNetworkName,
            WaterfallTestName = WaterfallTestName,
            Country = Country,
            NetworkPlacementId = AdNetworkPlacementId,
            Creative = Creative,
            Precision = Precision,
        };

        private MaxImpressionAnalyticsInfo ToMaxInfo() => new MaxImpressionAnalyticsInfo {
            Id = Id,
            AdUnitId = AdUnit,
            Currency = USD_CURRENCY,
            Revenue = Revenue,
            AdNetworkName = AdNetworkName,
            WaterfallTestName = WaterfallTestName,
            Country = Country,
            NetworkPlacementId = AdNetworkPlacementId,
            Creative = Creative,
            Precision = Precision,
        };

        private IronSourceImpressionAnalyticsInfo ToIronSourceInfo() => new IronSourceImpressionAnalyticsInfo {
            Id = Id,
            AdUnitId = AdUnit,
            Currency = USD_CURRENCY,
            Revenue = Revenue,
            AdNetworkName = AdNetworkName,
            WaterfallTestName = WaterfallTestName,
            Country = Country,
            NetworkPlacementId = AdNetworkPlacementId,
            Creative = Creative,
            Precision = Precision,
            Ab = Ab,
            ConversionValue = ConversionValue,
            InstanceId = InstanceId,
            InstanceName = InstanceName,
            JsonRepresentation = JsonRepresentation,
            LifetimeRevenue = LifetimeRevenue,
            SegmentName = SegmentName,
            EncryptedCPM = EncryptedCPM
        };
    }
}