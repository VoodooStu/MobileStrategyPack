using System;
using System.Threading.Tasks;
using UnityEngine.Events;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Privacy.UI
{
    public interface IPrivacyUIManager
    {
        void Initialize(IPrivacyUIManagerParameters p);

        Task<PrivacyConsent> GetPrivacyConsent(Action startLimitedPlaytime,
                                               Action stopLimitedPlaytime,
                                               bool offerWallEnabled,
                                               bool openOfferWall);

        void OpenSettingsScreen(bool adsConsent,
                                bool analyticsConsent,
                                bool idfaAuthorizationStatusDeniedOrRestricted,
                                string idfa,
                                UnityAction<PrivacyUpdate> updatePrivacy,
                                UnityAction<string> deleteData,
                                UnityAction<UserDetails> accessData,
                                UnityAction tryOpenDebugger,
                                UnityAction closeSettingsScreen);

        void CloseSettingsScreen();

        void CloseDeleteScreen();

        void CloseAccessDataScreen();

        void ClosePopupScreen();

        void OpenPrivacyNotInitializedPopupScreen(UnityAction closeCallback, VoodooSettings voodooSettings, IPrivacyCanvas privacyCanvas);

        void OpenUpdatePrivacySuccessPopupScreen(UnityAction closeCallback);

        void OpenUpdatePrivacyErrorPopupScreen(UnityAction closeCallback);

        void OpenSuccessPopupScreen(UnityAction closeCallback);
        
        void OpenErrorPopupScreen(string errorMessage, UnityAction closeCallback);

        void OpenConnexionErrorPopupScreen(UnityAction closeCallback);

        void OpenEmailErrorPopupScreen(UnityAction closeCallback);
        
        void OpenLoadingScreen();

        void CloseLoadingScreen();
    }
    
    public class PrivacyTexts {
        public string title;
        public string intro_thanks;
        public string outro_thanks;
        public string consent_advertising;
        public string consent_analytics;
        public string pgpd_sixteen;
        public string play;
        public string more;
        public string privacy_policy_url;
        public string title2;
        public string intro2;
        public string game;
        public string ads;
        public string bug;
        public string outro2;
        public string back;
        public string partners;
        public string intro_partners;
        public string advertising;
        public string analytics;
        public string delete_data;
        public string intro_delete;
        public string email;
        public string data_sharing;
        public string intro3;
        public string access_data;
        public string confirm;
        public string close;
        public string error_title;
        public string error;
        public string success;
        public string success_title;
        public string intro4;
        public string send;
        public string idfa;
        public string data_sharing_subtitle;
        public OfferwallTexts offer_wall;
    }

    public class OfferwallTexts
    {
        public string title;
        public string body;
        public string consentButtonText;
        public string purchaseButtonText;
        public string limitedPlayTimeButtonText;
    }
    
    
    public class IPrivacyUIManagerParameters
    {
        public IPrivacyCanvas privacyCanvas;
        public VoodooSettings voodooSettings;
        public string gdprTexts;
    }
    
    public class PrivacyConsent
    {
        public bool isSixteenOrOlder;
        public bool adsConsent;
        public bool analyticsConsent;
    }

    public class PrivacyUpdate
    {
        public bool adsConsent;
        public bool analyticsConsent;
    }
    
    public class UserDetails
    {
        public string email;
    }
}