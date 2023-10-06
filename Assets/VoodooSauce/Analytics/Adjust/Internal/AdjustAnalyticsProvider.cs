using System;
using System.Collections.Generic;
using com.adjust.sdk;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    internal class AdjustAnalyticsProvider : BaseAnalyticsProviderWithLogger, IAnalyticsAttributionProvider
    {
        private static AdjustParameters _parameters;
        internal override VoodooSauce.AnalyticsProvider GetProviderEnum() => VoodooSauce.AnalyticsProvider.Adjust;

        // Needed for the VoodooGDPRAnalytics class. Do not call it directly.
        public AdjustAnalyticsProvider() { }

        internal AdjustAnalyticsProvider(AdjustParameters parameters)
        {
            _parameters = parameters;
            RegisterEvents();
        }

        public override void Instantiate(string mediation)
        {
            AdjustWrapper.Instantiate();
        }

        public override void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild)
        {
            if (!consent.IsAdjustEnforcement || consent.ExplicitConsentGivenForAds) {
                AdjustWrapper.Initialize(GetAdjustToken(), IsTestMode(), isChinaBuild);
            }
            IsInitialized = true;
        }

        public AttributionData GetAttributionData() => AdjustWrapper.GetAttributionData();

        private static void RegisterEvents()
        {
            AnalyticsManager.OnGameFinishedEvent += OnGameFinished;
            //register for ads event
            AnalyticsManager.OnImpressionTrackedEvent += OnImpressionTracked;
            AnalyticsManager.OnTrackCustomEvent += TrackCustomEvent;
            AnalyticsManager.OnTrackConversionEvent += TrackConversionEvent;
            AnalyticsManager.OnTrackPurchaseEvent += TrackPurchase;
        }

        private static void OnGameFinished(GameFinishedParameters parameters)
        {
            int gameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            if (Array.IndexOf(AdjustConstants.GameCountsToTrack, gameCount) > -1) {
                AdjustWrapper.TrackEvent(gameCount + AdjustConstants.GamePlayedEventName);
            }
        }

        private static void OnImpressionTracked(ImpressionAnalyticsInfo impressionInfo)
        {
            switch (impressionInfo) {
                case MaxImpressionAnalyticsInfo maxImpressionInfo:
                    AdjustWrapper.TrackAdRevenue(new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX) {
                        revenue = maxImpressionInfo.Revenue,
                        currency = maxImpressionInfo.Currency,
                        adRevenueNetwork = maxImpressionInfo.AdNetworkName,
                        adRevenuePlacement = maxImpressionInfo.NetworkPlacementId,
                        adRevenueUnit = maxImpressionInfo.AdUnitId
                    });
                    break;
                case IronSourceImpressionAnalyticsInfo ironSourceImpressionInfo:
                    AdjustWrapper.TrackAdRevenue(new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceIronSource) {
                        revenue = ironSourceImpressionInfo.Revenue,
                        currency = ironSourceImpressionInfo.Currency,
                        adRevenueNetwork = ironSourceImpressionInfo.AdNetworkName,
                        adRevenuePlacement = ironSourceImpressionInfo.NetworkPlacementId,
                        adRevenueUnit = ironSourceImpressionInfo.AdUnitId
                    });
                    break;
            }
        }
        
        private static void TrackConversionEvent(ConversionEventInfo conversionEventInfo)
        {
            AdjustWrapper.TrackEvent(conversionEventInfo.AdjustEventToken);
        }

        private static void TrackPurchase(VoodooIAPAnalyticsInfo purchaseInfo)
        {
            string eventName = ProductPurchaseEvent.GetAdjustName(purchaseInfo.productId, purchaseInfo.purchaseType, _parameters.NoAdsBundleId);
                
            AdjustEvent adjustEvent = new AdjustEvent(eventName);
            adjustEvent.setRevenue(purchaseInfo.price, purchaseInfo.currency);
            adjustEvent.setTransactionId(purchaseInfo.purchaseTransactionId);
            adjustEvent.addCallbackParameter("product_id", purchaseInfo.productId);
            adjustEvent.addCallbackParameter("transaction_id", purchaseInfo.purchaseTransactionId);
            adjustEvent.addCallbackParameter("revenue_debug", purchaseInfo.price.ToString());
            adjustEvent.addCallbackParameter("currency_debug", purchaseInfo.currency);
            Adjust.trackEvent(adjustEvent);
        }

        private static void TrackCustomEvent(string eventName,
                                             Dictionary<string, object> customVariables,
                                             string eventType,
                                             List<VoodooSauce.AnalyticsProvider> analyticsProviders,
                                             Dictionary<string, object> contextVariables = null)
        {
            if (analyticsProviders.Contains(VoodooSauce.AnalyticsProvider.Adjust)) {
                AdjustWrapper.TrackEvent(eventName, customVariables);
            }
        }

        private static string GetAdjustToken()
        {
#if UNITY_IOS
            return _parameters.AdjustIosAppToken;
#elif UNITY_ANDROID
            return _parameters.AdjustAndroidAppToken;
#else
            return null;
#endif
        }

        private static bool IsTestMode() => Debug.isDebugBuild || AdjustConstants.TestDeviceIdfas.Contains(Adjust.getIdfa()) ||
            AdjustConstants.TestDeviceIdfas.Contains(SystemInfo.deviceUniqueIdentifier);
    }
}