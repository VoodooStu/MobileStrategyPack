using System;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Internal.CrossPromo.Models
{
    [Serializable]
    public class AssetModel
    {
        public string id;
        public string file_path;
        public string file_url;
        public string tracking_link;
        public string tracking_impression;
        public string cp_format;
        
        public string store_ios_url;

        public GameToPromote game;

        public CrossPromoAnalyticsInfo ToAnalyticsModel(int gameCount, int adsCount, string impressionId) => new CrossPromoAnalyticsInfo {
            Format = cp_format,
            AssetPath = file_path,
            GameName = game.name,
            GameBundleId = game.bundle_id,
            CampaignId = id,
            GameCount = gameCount,
            AdCount = adsCount,
            ImpressionId = impressionId
        };
    }

    [Serializable]
    public class GameToPromote
    {
        public string id;
        public string name;
        public long apple_id;
        public string bundle_id;
        public string os_type;
    }
}