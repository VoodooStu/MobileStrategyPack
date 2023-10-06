namespace Voodoo.Sauce.Internal.Analytics
{
    internal static class FirebaseAnalyticsConstants
    {
        internal static readonly int[] GameCountsToTrack = {1, 2, 3, 5, 8, 10, 15, 20, 25, 30, 50, 100, 1000};

        // Events name
        internal const string GAME_PROGRESS_EVENT_NAME = "game_progress";
        internal const string GAMES_PLAYED_EVENT_NAME = "games_played";
        internal const string BANNER_CLICKED_EVENT_NAME = "banner_click";
        internal const string FS_CLICKED_EVENT_NAME = "fs_click";
        internal const string RV_CLICKED_EVENT_NAME = "rv_click";
        internal const string FS_SHOWN_EVENT_NAME = "fs_shown";
        internal const string RV_SHOWN_EVENT_NAME = "rv_shown";
        internal const string ILRD_EVENT_NAME = "ad_revenue";
        internal const string ILRD_EVENT_NAME_V2 = "ad_impression";

        // Parameters
        internal const string GAME_STATUS_PARAMETER = "game_status";
        internal const string START_STATUS = "game_start";
        internal const string SUCCESS_STATUS = "game_success";
        internal const string FAILURE_STATUS = "game_failure";
        internal const string SCORE_PARAMETER = "score";
        internal const string LEVEL_PARAMETER = "level";

        // ILRD
        internal const string ILRD_AD_PLATFORM = "ad_platform";
        internal const string ILRD_AD_SOURCE = "ad_source";
        internal const string ILRD_AD_UNIT_NAME = "ad_unit_name";
        internal const string ILRD_AD_FORMAT = "ad_format";
        internal const string ILRD_PRECISION = "precision";
        internal const string ILRD_AD_UNIT_ID = "ad_unit_id";
        internal const string ILRD_APP_VERSION = "app_version";
        internal const string ILRD_AD_GROUP_ID = "ad_group_id";
        internal const string ILRD_AD_GROUP_NAME = "ad_group_name";
        internal const string ILRD_AD_GROUP_TYPE = "ad_group_type";
        internal const string ILRD_PRIORITY = "ad_group_priority";
        internal const string ILRD_COUNTRY = "country";
        
        internal const string FULLSCREEN = "Fullscreen";
        internal const string REWARDED_VIDEO = "Rewarded Video";
        internal const string BANNER = "Banner";

        //If you need to use firebase static constant, please add it here and use the one that are
        //maintained here. And add the switch case and map back to firebase constant in
        //FirebaseAnalyticsEvent.GetFirebaseKey
        // Temporary Param Key
        internal const string PARAMETER_LEVEL = "ParameterLevel";
        internal const string PARAMETER_SCORE = "ParameterScore";
        internal const string PARAMETER_VALUE = "ParameterValue";
        internal const string PARAMETER_CURRENCY = "ParameterCurrency";
        internal const string PARAMETER_ITEM_ID = "ParameterItemId";
        internal const string PARAMETER_PRICE = "ParameterPrice";
        // Temporary Event Key
        internal const string EVENT_LEVEL_UP = "EventLevelUp";
        internal const string EVENT_LEVEL_END = "EventLevelEnd";
        internal const string EVENT_LEVEL_START = "EventLevelStart";
        internal const string EVENT_ECOMMERCE_PURCHASE = "EventEcommercePurchase";
        
        // CI/CD Integration Test Event Key
        internal const string EVENT_LEVEL_UP_CICD = "level_up";
        internal const string EVENT_LEVEL_END_CICD = "level_end";
        internal const string EVENT_LEVEL_START_CICD = "level_start";
        internal const string EVENT_ECOMMERCE_PURCHASE_CICD = "ecommerce_purchase";
    }
}