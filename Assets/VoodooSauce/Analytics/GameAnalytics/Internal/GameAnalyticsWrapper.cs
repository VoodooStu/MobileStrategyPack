using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public static class GameAnalyticsWrapper
    {
        public const string TAG = "GameAnalyticsWrapper";
        private static bool _isInitialized;
        private static bool _isDisabled;
        private static readonly Queue<GameAnalyticsEvent> QueuedEvents = new Queue<GameAnalyticsEvent>();

        internal static bool Initialize(PrivacyCore.GdprConsent consent)
        {
            if (!consent.ExplicitConsentGivenForAnalytics) {
                Disable();
                return _isInitialized;
            }

            InstantiateGameAnalytics();
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"GameAnalytics initialized, tracking pending events: {QueuedEvents.Count}");
            while (QueuedEvents.Count > 0) {
                QueuedEvents.Dequeue().Track();
            }

            _isInitialized = true;
            return _isInitialized;
        }

        internal static void TrackProgressEvent(GAProgressionStatus status, string progress, int? score, Dictionary<string, object> customFields)
        {
            if (_isDisabled) return;

            var progressEvent = new ProgressEvent(status, progress, score, customFields);
            if (!_isInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"GameAnalytics NOT initialized queuing event...{status}");
                QueuedEvents.Enqueue(progressEvent);
            } else {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending event {status} to GameAnalytics");
                progressEvent.Track();
            }
        }

        internal static void TrackDesignEvent(string eventName, Dictionary<string, object> customFields)
        {
            if (_isDisabled) return;

            var designEvent = new DesignEvent(eventName, customFields);
            if (!_isInitialized) {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"GameAnalytics NOT initialized queuing event...{eventName}");
                QueuedEvents.Enqueue(designEvent);
            } else {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending event {eventName} to GameAnalytics");
                designEvent.Track();
            }
        }
        
        internal static void TrackPurchase(VoodooIAPAnalyticsInfo purchaseInfo)
        {
            if (_isDisabled || !_isInitialized) return;

            string productId = purchaseInfo.productId;
            string currencyCode = purchaseInfo.currency;
            int amount = Mathf.RoundToInt((float) purchaseInfo.price * 100f);
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, $"Sending  purchase event {productId} to GameAnalytics");
            GameAnalytics.NewBusinessEvent(currencyCode, amount, productId, productId, "");
        }
        
        private static void InstantiateGameAnalytics()
        {
            var gameAnalyticsComponent = Object.FindObjectOfType<GameAnalytics>();
            if (gameAnalyticsComponent == null) {
                var gameAnalyticsGameObject = new GameObject("GameAnalytics");
                gameAnalyticsGameObject.AddComponent<GameAnalytics>();
                gameAnalyticsGameObject.SetActive(true);
            } else {
                gameAnalyticsComponent.gameObject.name = "GameAnalytics";
            }

            SetBuildVersion($"{Application.version} {VoodooSauce.GetPlayerCohortName()}");

            GameAnalytics.Initialize();
        }

        private static void SetBuildVersion(string buildVersion)
        {
            int platformIndex = -1;
            if (PlatformUtils.UNITY_ANDROID)
            {
                platformIndex = GameAnalytics.SettingsGA.Platforms.IndexOf(RuntimePlatform.Android);
            }
            else if (PlatformUtils.UNITY_IOS)
            {
                platformIndex = GameAnalytics.SettingsGA.Platforms.IndexOf(RuntimePlatform.IPhonePlayer);
            }

            if (platformIndex < 0) {
                return;
            }

            if (buildVersion.Length > 32)
            {
                buildVersion = buildVersion.Substring(0, 32);
                VoodooLog.LogWarning(Module.ANALYTICS, TAG,
                    $"The GameAnalytics build name exceeds 32 characters and has been shortened to \"{buildVersion}\"");
            } else {
                VoodooLog.LogDebug(Module.ANALYTICS, TAG,
                    $"The GameAnalytics build name is \"{buildVersion}\"");
            }

            GameAnalytics.SettingsGA.Build[platformIndex] = buildVersion;
        }

        private static void Disable()
        {
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Disabling GameAnalytics No User Consent.");
            _isDisabled = true;
            QueuedEvents.Clear();
        }
    }
}