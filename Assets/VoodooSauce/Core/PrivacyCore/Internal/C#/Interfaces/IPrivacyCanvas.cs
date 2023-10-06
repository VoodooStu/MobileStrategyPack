namespace Voodoo.Sauce.Privacy.UI
{
    public interface IPrivacyCanvas
    {
        void OpenPrivacyScreen(PrivacyScreen.Parameters parameters = null);

        void ClosePrivacyScreen();

        void OpenLearnMoreScreen(LearnMoreScreen.Parameters parameters = null);

        void CloseLearnMoreScreen();

        void OpenPartnersScreen(ParternsScreen.Parameters p = null);

        void CloseParternsScreen();
        
        void OpenSettingsScreen(SettingsScreen.Parameters p = null);

        void CloseSettingsScreen();

        void OpenDeleteScreen(DeleteScreen.Parameters p = null);

        void CloseDeleteScreen();

        void OpenAccessDataScreen(AccessDataScreen.Parameters p = null);

        void CloseAccessDataScreen();

        void OpenPopupScreen(PopupScreen.Parameters p = null);

        void ClosePopupScreen();

        void OpenOfferWallScreen(OfferWallScreen.Parameters p = null);

        void CloseOfferWallScreen();

        void OpenLoadingScreen();

        void CloseLoadingScreen();
    }
}