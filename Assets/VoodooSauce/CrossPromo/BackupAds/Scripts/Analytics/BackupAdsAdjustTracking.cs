using UnityEngine;
using UnityEngine.Networking;
using Voodoo.Sauce.Core;
using QueryParameters = Voodoo.Sauce.Internal.Common.Utils.QueryParameters;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Analytics
{
    public static class BackupAdsAdjustTracking
    {
        private const string CLICK_URL = "https://adjust.com/";
        private const string IMPRESSION_URL = "https://view.adjust.com/impression/";
        private const string STORE_URL = "https://app.adjust.com/";
        private static PrivacyCore Privacy => VoodooSauceCore.GetPrivacy();

        public static void TriggerClick(BackupAdsAnalyticsInfo info)
        {
            string url = CreateUrl(CLICK_URL, info);
            if (!string.IsNullOrEmpty(info.clickUrl)) {
                url = info.clickUrl;
            }

            SendQuery(url);
        }

        public static void TriggerImpression(BackupAdsAnalyticsInfo info)
        {
            string url = CreateUrl(IMPRESSION_URL, info);
            if (!string.IsNullOrEmpty(info.impressionUrl)) {
                url = info.impressionUrl;
            }

            SendQuery(url);
        }

        public static void TriggerRedirection(BackupAdsAnalyticsInfo info)
        {
            string url = CreateUrl(STORE_URL, info);
            if (!string.IsNullOrEmpty(info.redirectionUrl)) {
                url = info.redirectionUrl;
            }

            Application.OpenURL(url);
        }

        private static string CreateUrl(string url, BackupAdsAnalyticsInfo info)
        {
            var queryParams = new QueryParameters($"{url}{info.adjustAppId}");
            queryParams.Add("campaign", info.campaignId);
            queryParams.Add("creative", info.filePath);
            queryParams.Add("label", info.adGuid);
            queryParams.Add("adgroup", Application.identifier);
            queryParams.Add("idfv", SystemInfo.deviceUniqueIdentifier);
            queryParams.Add("idfa", Privacy.GetAdvertisingId());
            queryParams.Add("gps_adid", Privacy.GetAdvertisingId());
            var finalUrl = queryParams.GetFormattedUrl();
            return finalUrl;
        }

        private static void SendQuery(string url)
        {
            var webRequest = new UnityWebRequest(url);
            webRequest.SendWebRequest();
        }

        public static string GetClickUrl(BackupAdsAnalyticsInfo info) => CreateUrl(CLICK_URL, info);
        public static string GetRedirectionUrl(BackupAdsAnalyticsInfo info) => CreateUrl(STORE_URL, info);
        public static string GetImpressionUrl(BackupAdsAnalyticsInfo info) => CreateUrl(IMPRESSION_URL, info);
    }
}