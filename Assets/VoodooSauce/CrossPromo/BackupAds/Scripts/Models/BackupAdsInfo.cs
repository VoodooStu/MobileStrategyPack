using System;
using UnityEngine.Video;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Models;

namespace Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models
{
    [Serializable]
    public class BackupAdsInfo
    {
        public string gameName;

        public VideoClip videoClip;
        public string videoUrl;

        public string bundleId;

        public string adjustAppId;
        public string adjustCampaignId;

        public string clickUrl;
        public string redirectionUrl;
        public string impressionUrl;
        public int videoIndex;
        public string mercuryCampaignId;
        
        public BackupAdType adType;

        public enum BackupAdType
        {
            BackupFS, BackupRV
        }

        private BackupAdsAnalyticsInfo _currentAnalyticsInfo;
        private bool _restrictedPrivacy;

        public BackupAdsInfo(MercuryPromotedAsset asset, BackupAdType adType, bool restrictedPrivacy)
        {
            _restrictedPrivacy = restrictedPrivacy;

            gameName = asset.game.name;
            bundleId = asset.game.bundle_id;
            videoUrl = asset.videoUrl;
            clickUrl = asset.tracking_link;
            impressionUrl = asset.tracking_impression;
            redirectionUrl = asset.store_ios_url;
            videoIndex = asset.videoIndex;
            mercuryCampaignId = asset.id;
            this.adType = adType;
            
            InitializeAnalyticsInfo();
        }

        private void InitializeAnalyticsInfo()
        {
            _currentAnalyticsInfo = new BackupAdsAnalyticsInfo(this) {
                adGuid = Guid.NewGuid().ToString(),
            };
            RefreshAnalyticsCounterInfo();
        }

        public void TriggerAnalyticsImpression()
        {
            if (adType == BackupAdType.BackupFS) {
                AnalyticsStorageHelper.Instance.IncrementShowBackupFsCount();
            }

            if (adType == BackupAdType.BackupRV) {
                AnalyticsStorageHelper.Instance.IncrementShowBackupRvCount();
            }

            RefreshAnalyticsCounterInfo();
            BackupAdsAnalytics.TriggerInterstitialShown(_currentAnalyticsInfo);
        }

        public void TriggerAnalyticsClick()
        {
            RefreshAnalyticsCounterInfo();
            BackupAdsAnalytics.TriggerInterstitialClicked(_currentAnalyticsInfo);
        }

        public void TriggerAdjustImpression()
        {
            if (!_restrictedPrivacy) {
                BackupAdsAdjustTracking.TriggerImpression(_currentAnalyticsInfo);
            }
        }

        public void TriggerAdjustClick()
        {
            if (!_restrictedPrivacy) {
                BackupAdsAdjustTracking.TriggerClick(_currentAnalyticsInfo);
            }
        }

        public void TriggerAdjustRedirection()
        {
            BackupAdsAdjustTracking.TriggerRedirection(_currentAnalyticsInfo);
        }

        private void RefreshAnalyticsCounterInfo()
        {
            if (adType == BackupAdType.BackupFS) {
                _currentAnalyticsInfo.adCount = AnalyticsStorageHelper.Instance.GetShowBackupFsCount();
            }
            
            if (adType == BackupAdType.BackupRV) {
                _currentAnalyticsInfo.adCount = AnalyticsStorageHelper.Instance.GetShowBackupRvCount();
            }
            
            _currentAnalyticsInfo.gameCount = AnalyticsStorageHelper.Instance.GetGameCount();
        }
    }
}