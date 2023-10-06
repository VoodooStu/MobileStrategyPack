using System;

namespace Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models
{
    [Serializable]
    public struct BackupAdsAnalyticsInfo
    {
        public string bundleId;
        public string filePath;
        public string adjustAppId;
        public string adGuid;
        public string campaignId;

        public string clickUrl;
        public string redirectionUrl;
        public string impressionUrl;
        public string mercuryCampaignId;
        public int adCount;
        public int gameCount;
        public string format;

        public BackupAdsAnalyticsInfo(BackupAdsInfo info)
        {
            adGuid = "";
            bundleId = info.bundleId;
            adjustAppId = info.adjustAppId;
            campaignId = info.adjustCampaignId;

            clickUrl = info.clickUrl;
            redirectionUrl = info.redirectionUrl;
            impressionUrl = info.impressionUrl;
            mercuryCampaignId = info.mercuryCampaignId;
            adCount = 0;
            gameCount = 0;

            format = info.adType.ToString();
            
            if (info.videoClip) {
                filePath = info.videoClip.name;
            } else {
                filePath = info.videoUrl;
            }
        }
    }
}