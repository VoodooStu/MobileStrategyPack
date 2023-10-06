namespace Voodoo.Sauce.Internal.Ads
{
    public static class AdsConstants
    {
        public const string MEDIATION_TYPE = "VoodooAds_MediationType";
        public const string ROOT_FOLDER_PATH = "Assets/VoodooSauce/Ads";
        private const string MEDIATIONS_FOLDER_PATH = ROOT_FOLDER_PATH + "/Mediations";
        private const string TEST_FOLDER_PATH = "Assets/IntegrationTest/Editor/Ads";
        
        public const string MAX_MEDIATION_FOLDER_PATH = MEDIATIONS_FOLDER_PATH + "/MaxAds";
        public const string MAX_TEST_FOLDER_PATH = TEST_FOLDER_PATH + "/MaxAds";
        //SDK PATH used to change Mediation SDK root path
        public const string MAX_ADS_SDK_FOLDER_PATH = MAX_MEDIATION_FOLDER_PATH + "/3rdParty";
        
        public const string IRONSOURCE_MEDIATION_FOLDER_PATH = MEDIATIONS_FOLDER_PATH + "/IronSource";
        public const string IRONSOURCE_SDK_FOLDER_PATH = IRONSOURCE_MEDIATION_FOLDER_PATH + "/3rdParty";
        public const string IRONSOURCE_TEST_FOLDER_PATH = TEST_FOLDER_PATH + "/IronSource";
    }
}