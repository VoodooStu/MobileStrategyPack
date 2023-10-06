using UnityEngine;

namespace Voodoo.Sauce.Privacy.UI
{
    public class PrivacyCanvas : MonoBehaviour, IPrivacyCanvas
    {
        [SerializeField] private PrivacyScreen _privacyScreenPrefab;
        [SerializeField] private LearnMoreScreen _learnMoreScreenPrefab;
        [SerializeField] private ParternsScreen _parternsScreenPrefab;
        [SerializeField] private SettingsScreen _settingsScreenPrefab;
        [SerializeField] private DeleteScreen _deleteScreenPrefab;
        [SerializeField] private AccessDataScreen _accessDataScreenPrefab;
        [SerializeField] private PopupScreen _popupScreenPrefab;
        [SerializeField] private LoadingScreen _loadingScreenPrefab;
        [SerializeField] private OfferWallScreen offerWallScreenPrefab;

        private PrivacyScreen _privacyScreen;
        private LearnMoreScreen _learnMoreScreen;
        private ParternsScreen _parternsScreen;
        private SettingsScreen _settingsScreen;
        private DeleteScreen _deleteScreen;
        private AccessDataScreen _accessDataScreen;
        private PopupScreen _popupScreen;
        private LoadingScreen _loadingScreen;
        private OfferWallScreen _offerWallScreen;

        public void OpenPrivacyScreen(PrivacyScreen.Parameters p = null)
        {
            if (_privacyScreen != null) _privacyScreen.gameObject.SetActive(true);
            else _privacyScreen = Instantiate(_privacyScreenPrefab, transform);
            if (p != null) _privacyScreen.Initialize(p);
        }

        public void ClosePrivacyScreen()
        {
            if (_privacyScreen != null) _privacyScreen.gameObject.SetActive(false);
        }


        public void OpenLearnMoreScreen(LearnMoreScreen.Parameters p = null)
        {
            if (_learnMoreScreen != null) _learnMoreScreen.gameObject.SetActive(true);
            else _learnMoreScreen = Instantiate(_learnMoreScreenPrefab, transform);
            if (p != null) _learnMoreScreen.Initialize(p);
        }

        public void CloseLearnMoreScreen()
        {
            if (_learnMoreScreen != null) _learnMoreScreen.gameObject.SetActive(false);
        }

        public void OpenPartnersScreen(ParternsScreen.Parameters p = null)
        {
            if (_parternsScreen != null) _parternsScreen.gameObject.SetActive(true);
            else _parternsScreen = Instantiate(_parternsScreenPrefab, transform);
            if (p != null) _parternsScreen.Initialize(p);
        }
        
        public void CloseParternsScreen()
        {
            if (_parternsScreen != null) _parternsScreen.gameObject.SetActive(false);
        }

        public void OpenSettingsScreen(SettingsScreen.Parameters p = null)
        {
            if (_settingsScreen != null) _settingsScreen.gameObject.SetActive(true);
            else _settingsScreen = Instantiate(_settingsScreenPrefab, transform);
            if (p != null) _settingsScreen.Initialize(p);
        }

        public void CloseSettingsScreen()
        {
            if (_settingsScreen != null) _settingsScreen.gameObject.SetActive(false);
        }

        public void OpenDeleteScreen(DeleteScreen.Parameters p = null)
        {
            if (_deleteScreen != null) _deleteScreen.gameObject.SetActive(true);
            else _deleteScreen = Instantiate(_deleteScreenPrefab, transform);
            if (p != null) _deleteScreen.Initialize(p);
        }

        public void CloseDeleteScreen()
        {
            if (_deleteScreen != null) _deleteScreen.gameObject.SetActive(false);
        }

        public void OpenAccessDataScreen(AccessDataScreen.Parameters p = null)
        {
            if (_accessDataScreen != null) _accessDataScreen.gameObject.SetActive(true);
            else _accessDataScreen = Instantiate(_accessDataScreenPrefab, transform);
            if (p != null) _accessDataScreen.Initialize(p);
        }

        public void CloseAccessDataScreen()
        {
            if (_accessDataScreen != null) _accessDataScreen.gameObject.SetActive(false);
        }

        public void OpenPopupScreen(PopupScreen.Parameters p = null)
        {
            if (_popupScreen != null) _popupScreen.gameObject.SetActive(true);
            else _popupScreen = Instantiate(_popupScreenPrefab, transform);
            if (p != null) _popupScreen.Initialize(p);
        }

        public void ClosePopupScreen()
        {
            if (_popupScreen != null) _popupScreen.gameObject.SetActive(false);
        }
        
        public void OpenOfferWallScreen(OfferWallScreen.Parameters p = null)
        {
            if (_offerWallScreen != null) _offerWallScreen.gameObject.SetActive(true);
            else _offerWallScreen = Instantiate(offerWallScreenPrefab, transform);
            if (p != null) _offerWallScreen.Initialize(p);
        }

        public void CloseOfferWallScreen()
        {
            if (_offerWallScreen != null) _offerWallScreen.gameObject.SetActive(false);
        }

        public void OpenLoadingScreen()
        {
            if (_loadingScreen == null) _loadingScreen = Instantiate(_loadingScreenPrefab, transform.parent);
            _loadingScreen.gameObject.SetActive(true);
        }

        public void CloseLoadingScreen()
        {
            if (_loadingScreen != null) _loadingScreen.gameObject.SetActive(false);
        }
    }
}