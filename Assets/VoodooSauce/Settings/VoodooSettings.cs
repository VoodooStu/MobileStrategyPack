using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo;
using Voodoo.Sauce.Internal.EditorCustomAttributes;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    [Serializable,
     CreateAssetMenu(fileName = "Assets/Resources/VoodooSettings", menuName = "VoodooSauce/Settings file",
         order = 1000)]
    public class VoodooSettings : ScriptableObject, IVoodooSettings
    {
#region constants

        public const string NAME = "VoodooSettings";

        // The different stores available in Kitchen.
        public const string STORE_WORLDWIDE = "worldwide";
        public const string STORE_CHINA = "china";

        public const string DEFAULT_STORE = STORE_WORLDWIDE;

#endregion

#region Methods
     
        public static VoodooSettings Load() => Resources.Load<VoodooSettings>(NAME);

        public bool IsChinaStore() => Store == STORE_CHINA;
        
        [CanBeNull]
        public AmazonKey AmazonKey {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return amazonAndroidKey;
                }

                if (PlatformUtils.UNITY_IOS)
                {
                    return amazonIosKey;
                }
                return null;
            }
        }

        public bool IsIAPEnabled {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return AndroidIAPEnabled;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return iOSIAPEnabled; 
                }
                return false;
            }
        }

        public string GetAdjustAppToken()
        {
            if (PlatformUtils.UNITY_ANDROID) { 
                return AdjustAndroidAppToken;
            }
            if (PlatformUtils.UNITY_IOS) {
                return AdjustIosAppToken;
            }
            return "";
        }
        
        public string MaxSdkKey {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return MaxAndroidSdkKey;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return MaxIosSdkKey;
                }
                return "";
            }
        }
        
        public string GetIronSourceMediationAppKey()
        {
            if (PlatformUtils.UNITY_IOS) {
                return IronSourceMediationIosAppKey;
            }
            if (PlatformUtils.UNITY_ANDROID) {
                return IronSourceMediationAndroidAppKey;
            }
            return null;   
        }
        
        public string MixpanelProdToken
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return MixpanelAndroidProdToken;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return MixpanelIosProdToken;
                }
                return "";
            }
        }
        
        public ConversionEventsSettings ConversionEvents {
         get {
          if (PlatformUtils.UNITY_ANDROID) {
           return ConversionAndroidEvents;
          }
          if (PlatformUtils.UNITY_IOS) {
           return ConversionIosEvents;
          }
          return null;
         }
        }

        public bool UseFirebaseAnalytics
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return UseAndroidFirebaseAnalytics;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return UseIosFirebaseAnalytics;
                }
                return true;
            }
        }
        
        public bool UseRemoteConfig
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return UseAndroidRemoteConfig;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return UseIosRemoteConfig;
                }
                return true;
            }
        }

        public bool EnableReplaceRewardedOnCpm 
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return EnableAndroidReplaceRewardedOnCpm;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return EnableIosReplaceRewardedOnCpm;
                }
                return false;
            }
        }

        public bool EnableReplaceRewardedIfNotLoaded 
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return EnableAndroidReplaceRewardedIfNotLoaded;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return EnableIosReplaceRewardedIfNotLoaded;
                }
                return false;
            }
        }
        
        public bool UseVoodooAnalytics
        {
             get {
                 if (PlatformUtils.UNITY_ANDROID) {
                     return UseAndroidVoodooAnalytics;
                 }
                 if (PlatformUtils.UNITY_IOS) {
                     return UseIosVoodooAnalytics;
                 }
                 return true;
             }
        }

        public bool BannerCloseButtonEnabled {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return AndroidBannerCloseButtonEnabled;
                }

                if (PlatformUtils.UNITY_IOS) {
                    return iOSBannerCloseButtonEnabled;
                }

                return true;
            }
        }

        public float EmbraceUserPercentage
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return embraceAndroidUserPercentage;
                }

                if (PlatformUtils.UNITY_IOS) {
                    return embraceIosUserPercentage;
                }

                return 0.0f;
            }
        }

        public string EmbraceAppId
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return embraceAndroidAppId;
                }

                if (PlatformUtils.UNITY_IOS) {
                    return embraceIosAppId;
                }

                return "";
            }
        }

        public string EmbraceApiToken
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return embraceAndroidApiToken;
                }

                if (PlatformUtils.UNITY_IOS) {
                    return embraceIosApiToken;
                }

                return "";
            }
        }

        public static bool IsAdUnitFieldName(string fieldName) => fieldName.Contains("AdsKeys");
        
        public bool EnableAppOpenAdSoftLaunch
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return EnableAndroidAppOpenAdSoftLaunch;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return EnableIosAppOpenAdSoftLaunch;
                }
                return EnableAndroidAppOpenAdSoftLaunch;
            }
        }
        
        public AppOpenAdConfig AppOpenAdConfig
        {
            get {
                if (PlatformUtils.UNITY_ANDROID) {
                    return AndroidAppOpenAdConfig;
                }
                if (PlatformUtils.UNITY_IOS) {
                    return iOSAppOpenAdConfig;
                }
                return new AppOpenAdConfig();
            }
        }

		public string NoAdsBundleId => PlatformUtils.UNITY_ANDROID ? AndroidNoAdsProductId : iOSNoAdsProductId;
		
#endregion

        public enum Position
        {
            Right,
            Left
        }

#region Properties

        [HideInInspector]
        public string Store = "";

        [CustomLabel("Voodoo Server Access Token"),
         ExplanationText(
             "This token is mandatory to retrieve the VoodooSauce settings of your game. Ask your Voodoo contact to give you this token.")]
        public string AccessTokenID = "";

        //[CustomLabelAndValue("VoodooSauce Settings Last Update", true)]
        [ReadOnly,
         LargeHeader("Settings that you must edit yourself to control AB Tests, IAP, and Splash Screen",
             "Manual Settings"
         ),
         Separator]
        public string LastUpdateDate = "09/08/2019";

        // Manual settings begin here.
        
        //Android Build Target Config
        // [Header("Android build Target"), WarningMessage(
        //      "Starting from 31st Aug 2023, Google play won't accept new or updated apps with API below 33"
        //      + "\n\nIf Target API 33 is selected, VoodooSauce will perform additional check during Validation to "
        //      + "ensure that you have everything configured correctly."
        //      + "\n\nDefault value is API 33"), Space]
        [Header("Android build Target"), Space]
        public VoodooSauceAndroidBuildTargetEnum AndroidBuildTargetEnum = VoodooSauceAndroidBuildTargetEnum.BuildTargetApi33;

        [FormerlySerializedAs("EnableFakeInUnityAds"), Header("In Editor Ads")]
        public bool EnableInEditorUnityAds = true;
        public bool EnableInEditorRVAds = true;
        public bool EnableInEditorFSAds = true;
        public bool EnableInEditorAOAds = true;

        [Header("Super Premium Mode")]
        [Tooltip("[ONLY ON DEVELOPMENT MODE] Super Premium mode will automatically enable Premium mode, and Rewarded will always be ready and"
            + " can trigger reward without watching the ads")]
        public bool EnableSuperPremiumMode = false;

        public VoodooFunnelsSettings VoodooFunnels; 
        
        public string LegacyAbTestName {
            get {
                if (PlatformUtils.UNITY_IOS) {
                    return LegacyIosABTestName;
                }
                if (PlatformUtils.UNITY_ANDROID) {
                    return LegacyAndroidABTestName;
                }
                return "";
            }
        }
        
        [Header("Legacy A/B Tests iOS")]
        [Tooltip(
             "iOS A/B testing is deactivated for countries other than the 'US'. Please contact Voodoo to activate this flag to test in others countries."),
         ReadOnly]
        public bool EnableCustomIosABTestsCountryCodes = false;

        [ReadOnly]
        public float MaxPercentOfTotalIosCohorts = 0.25f;

        [SerializeField]
        private string LegacyIosABTestName; 

        [AbTestsPercentRange(RuntimePlatform.IPhonePlayer, "iOS Users Percent Per Cohort")]
        public float MixpanelUsersPercentPerCohort = 0.05f;

        [FormerlySerializedAs("RunningABTests"),
         Tooltip("The list of currently active iOS A/B tests. Each entry represents a cohort")]
        public string[] RunningIosABTests;

        [Tooltip("The list of custom country codes in which iOS A/B tests are activated (US, JP, GB, FR, ...)")]
        public string[] CustomIosABTestsCountryCodes = {"US"};

        [Tooltip("This value will be used as a iOS cohort in debug / editor mode.")]
        public DebugForcedCohortIos DebugForcedCohort;

        [Header("Legacy A/B Tests Android")]
        [Tooltip(
             "Android A/B testing is deactivated for countries other than the 'US'. Please contact Voodoo to activate this flag to test in others countries."),
         ReadOnly]
        public bool EnableCustomAndroidABTestsCountryCodes;

        [ReadOnly]
        public float MaxPercentOfTotalAndroidCohorts = 0.25f;
        
        [SerializeField]
        private string LegacyAndroidABTestName; 

        [AbTestsPercentRange(RuntimePlatform.Android)]
        public float AndroidUsersPercentPerCohort = 0.05f;

        [Tooltip("The list of currently active Android A/B tests. Each entry represents a cohort")]
        public string[] RunningAndroidABTests;

        [Tooltip("The list of custom country codes in which Android A/B tests are activated (US, JP, GB, FR, ...)")]
        public string[] CustomAndroidABTestsCountryCodes = {"US"};

        [Tooltip("This value will be used as a Android cohort in debug / editor mode.")]
        public DebugForcedCohortAndroid DebugAndroidForcedCohort;

        [Header("In-app Purchases"),
         ReadOnly]
        public bool AndroidIAPEnabled = true;

        [ReadOnly]
        public bool iOSIAPEnabled = true;

        [FormerlySerializedAs("NoAdsBundleId"),
         Tooltip("Your No-Ads in-app purchase product id for Android. You need to use the NoAdsButton component if you fill that field.")]
        public string AndroidNoAdsProductId;
        
        [FormerlySerializedAs("NoAdsBundleId"),
         Tooltip("Your No-Ads in-app purchase product id for iOS. You need to use the NoAdsButton component if you fill that field.")]
        public string iOSNoAdsProductId;
        
        [Tooltip("The list of all products available for sale in the app")]
        public ProductDescriptor[] Products;

        [Header("In-app subscriptions, Apple's App-Specific Shared Secret"),
         Tooltip("Get Verification Credentials from the app store connect. This key allows you to verify the auto-renewable subscriptions")]
        public string SubscriptionSharedKey;

        [Header("SplashScreen"),
         Tooltip("Please provide your White studio logo on a transparent background with no margin.")]
        public Texture2D StudioLogoForSplashScreen;

        [Tooltip("Disable VS management of your Unity Splash Screen settings.  Any existing settings will not be reset.")]
        public bool DisableVSManagedSplashScreen;

        [Header("App Rater"),
         Tooltip("Apple Store Id of the game"),
         ReadOnly]
        public string AppleStoreId;

        [ReadOnly]
        public bool iOSAppRaterEnabled = true;

        [ReadOnly]
        public bool AndroidAppRaterEnabled = true;

        [Tooltip("Minimum delay when the App Rater can be displayed after the launch of the game (in seconds)")]
        public int AppRaterDelayAfterLaunchInSeconds = 60;

        [Tooltip("Minimum delay between two App Rater displays (in seconds)")]
        public int AppRaterPostponeCooldownInSeconds = 86400;

        [Tooltip("Minimum number of games at which the App Rater will starts to be shown." +
            "If 0, the App Rater will never be shown after a game played")]
        public int AppRaterMinimumNumberOfGamesPlayed = 5;

        [Header("Misc"),
         Tooltip("The primary color that will show on the GDPR popup")]
        public Color GdprPrimaryColor = new Color(0.254f, 0.509f, 0.894f);

        [Header("Cross Promotion")]
        public CrossPromoSettings CrossPromo;

        [ReadOnly]
        public bool iOSCrossPromotionEnabled = true;

        [ReadOnly]
        public bool AndroidCrossPromotionEnabled = true;

        [Header("Banner")]
        [ReadOnly]
        public bool iOSBannerCloseButtonEnabled;
        
        [ReadOnly]
        public bool AndroidBannerCloseButtonEnabled;

        [Tooltip("Color of the banner background")]
        public Color BannerBackgroundColor = Color.white;

        [Tooltip("Sprite of the banner close button, if not set use the default one")]
        public Sprite BannerCloseButtonSprite;

        [Tooltip("Position of the banner close button")]
        public Position BannerCloseButtonPosition;

        [Tooltip("If enabled the banner background and close button position will automatically being adjusted based on Banner height from Mediation")]
        public bool EnableAutomaticBannerHeightAdjustment;
        
        [LargeHeader(
            "Settings sent from Voodoo Servers. Update by clicking 'Update from Online'",
            "Automatic Settings"
        )]

        // This is "manual setting" but Editor GUI glitch means it needs to be here 
        [WarningMessage(
             "By checking the checkbox above, you understand that you need to obfuscate your google public key before building for Android release. Check the toggle above to ignore the error. (CHECK ONLY FOR DEBUG OR TEST PURPOSES)"),
         Tooltip("If checked, the Android build won't be stopped because the Google Public Key is missing.")]
        public bool IgnoreTheGoogleKeyError;

        [Header("Bundle identifiers"),
         ReadOnly]
        public string IOSBundleID = "";
        [ReadOnly]
        public string AndroidBundleID = "";

        [Header("Remote A/B Tests"),
         Tooltip("Use Legacy A/B Tests or Remote A/B Tests via VoodooTune Config for Android"),
         ReadOnly]
        public bool UseAndroidRemoteConfig;

        [Tooltip("Use Legacy A/B Tests or Remote A/B Tests via VoodooTune Config for iOS"),
         ReadOnly]
        public bool UseIosRemoteConfig;

        [Header("Rewarded Ads Config"),
         Tooltip("Display Interstitial instead of Rewarded Video if CPM Higher for iOS"),
         ReadOnly]
        public bool EnableIosReplaceRewardedOnCpm;

        [Tooltip("Display Interstitial instead of Rewarded Video if CPM Higher for Android"),
         ReadOnly]
        public bool EnableAndroidReplaceRewardedOnCpm;
        
        [Header("Rewarded Ads Config"),
         Tooltip("Display Interstitial instead of Rewarded Video if RV is not loaded for iOS"),
         ReadOnly]
        public bool EnableIosReplaceRewardedIfNotLoaded;

        [Tooltip("Display Interstitial instead of Rewarded Video if RV is not loaded for Android"),
         ReadOnly]
        public bool EnableAndroidReplaceRewardedIfNotLoaded;
        
        [Header("Voodoo Analytics"),
         Tooltip("Use Voodoo Analytics for iOS"),
         ReadOnly]
        public bool UseIosVoodooAnalytics;

        [Tooltip("Use Voodoo Analytics for Android"),
         ReadOnly]
        public bool UseAndroidVoodooAnalytics;

        [Header("Firebase Analytics"),
         Tooltip("Use Firebase Analytics for iOS"),
         ReadOnly]
        public bool UseIosFirebaseAnalytics;

        [Tooltip("Use Firebase Analytics for Android"),
         ReadOnly]
        public bool UseAndroidFirebaseAnalytics;
        
        [Header("Embrace Analytics"),
         Tooltip("Percentage of users on iOS using Embrace (0 = disabled, maximum 20.0)"),
         ReadOnly]
        public float embraceIosUserPercentage;

        [Tooltip("Application ID for this iOS application"),
         ReadOnly]
        public string embraceIosAppId;

        [Tooltip("API token for this iOS application"),
         ReadOnly]
        public string embraceIosApiToken;
        
        [Tooltip("Percentage of users on Android using Embrace (0 = disabled, maximum 20.0)"),
        ReadOnly]
        public float embraceAndroidUserPercentage;

        [Tooltip("Application ID for this Android application"),
         ReadOnly]
        public string embraceAndroidAppId;

        [Tooltip("API token for this Android application"),
         ReadOnly]
        public string embraceAndroidApiToken;
        
        [HideInInspector]
        public float embraceMaxPercentage = 20.0f;

        [Header("Mixpanel")]
        [FormerlySerializedAs("EnableIosABTests"), ReadOnly]
        public bool UseMixpanelIos = true;
        [FormerlySerializedAs("EnableAndroidABTests"), ReadOnly]
        public bool UseMixpanelAndroid;
        [Tooltip("The token of the Mixpanel iOS project. Leaving it empty will disable Mixpanel."),
         ReadOnly]
        public string MixpanelIosProdToken;

        [Tooltip("The token of the Mixpanel Android project. Leaving it empty will disable Mixpanel."),
         ReadOnly]
        public string MixpanelAndroidProdToken;

        [Header("GameAnalytics"),
         Tooltip("Your GameAnalytics iOS Game Key - copy/paste from the GA website"),
         ReadOnly]
        public string GameAnalyticsIosGameKey;

        [Tooltip("Your GameAnalytics iOS Secret Key - copy/paste from the GA website"),
         ReadOnly]
        public string GameAnalyticsIosSecretKey;

        [Tooltip("Your GameAnalytics Android Game Key - copy/paste from the GA website"),
         ReadOnly]
        public string GameAnalyticsAndroidGameKey;

        [Tooltip("Your GameAnalytics Android Secret Key - copy/paste from the GA website"),
         ReadOnly]
        public string GameAnalyticsAndroidSecretKey;
        
        [Header("MaxAds - Ads keys"),
         Tooltip("Leave the field blank if you don't want a certain kind of ads in your game"),
         ReadOnly]
        public AdsKeys MaxAdsIosAdsKeys;
        
        [Tooltip("Leave the field blank if you don't want a certain kind of ads in your game"),
         ReadOnly]
        public AdsKeys MaxAdsAndroidAdsKeys;

        [ReadOnly]
        public string MaxIosSdkKey;
        
        [ReadOnly]
        public string MaxAndroidSdkKey;
        
        [Header("IronSource Mediation - keys"),
         Tooltip("Leave the field blank if you don't want a certain kind of ads in your game"),
         ReadOnly]
        public string IronSourceMediationIosAppKey;
        [ReadOnly]
        public string IronSourceMediationAndroidAppKey;

        // [VS-3649] AdMob identifiers must be provided:
        // if keys are added in this section please complete the classes `GoogleIdIntegrationCheck`
        [Header("AdMob"),
         Tooltip("Ask your Voodoo agents for AdMob Android App Id"),
         ReadOnly]
        public string AdMobAndroidAppId;
        
        [Tooltip("Ask your Voodoo agents for AdMob iOS App Id"),
         ReadOnly]
        public string AdMobIosAppId;
        
        [Header("Amazon APS"), ReadOnly]
        public AmazonKey amazonIosKey;
        [ReadOnly]
        public AmazonKey amazonAndroidKey;
        
        // Adjust 		
        [Header("Adjust"),
         Tooltip("Adjust iOS App Token"),
         ReadOnly]
        public string AdjustIosAppToken;

        [Tooltip("Adjust Android App Token"),
         ReadOnly]
        public string AdjustAndroidAppToken;
        
        [Tooltip("Conversion iOS Events"),
         ReadOnly]
        public ConversionEventsSettings ConversionIosEvents;

        [Tooltip("Conversion Android Events"),
         ReadOnly]
        public ConversionEventsSettings ConversionAndroidEvents;
        
        // AppOpen Ads
        [Header("AppOpen Ads"),
         ReadOnly]
        public bool EnableIosAppOpenAdSoftLaunch;
        [ReadOnly]
        public bool EnableAndroidAppOpenAdSoftLaunch;
        [ReadOnly]
        public AppOpenAdConfig iOSAppOpenAdConfig;
        [ReadOnly]
        public AppOpenAdConfig AndroidAppOpenAdConfig;
        
        [Header("Audio Ads"), ReadOnly] public AudioAdConfig iOSAudioAdConfig;
        public AudioAdConfig GetIosAudioAdConfig => iOSAudioAdConfig;
        [ReadOnly] public AudioAdConfig AndroidAudioAdConfig;
        public AudioAdConfig GetAndroidAudioAdConfig => AndroidAudioAdConfig;

        [Header("Odeeo"), ReadOnly] public OdeeoConfig iOSOdeeoConfig;
        [ReadOnly] public OdeeoConfig AndroidOdeeoConfig;
#endregion

    }
}
