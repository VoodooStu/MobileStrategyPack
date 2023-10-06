using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Models;

namespace Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Analytics
{
    public class BackupAdsAnalytics
    {
        public static void TriggerMercuryInitFinished(MercuryRequestInfo info)
        {
            AnalyticsManager.TrackBackupAdsInit(new CrossPromoInitInfo {
                DownloadTime = info.downloadTime,
                HasTimeout = info.hasTimeout,
                GamesPromoted = info.gamesPromoted,
                HttpResponse = info.httpResponse,
                StrategyId = info.waterfall.strategy_id
            });
        }

        public static void TriggerInterstitialShown(BackupAdsAnalyticsInfo info)
        {
            AnalyticsManager.TrackBackupAdsShown(info);
        }

        public static void TriggerInterstitialClicked(BackupAdsAnalyticsInfo info)
        {
            AnalyticsManager.TrackBackupAdsClick(info);
        }
    }
}