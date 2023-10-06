using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinyJson;
using UnityEngine;
using UnityEngine.Events;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Privacy.UI
{
    public class PrivacyUIManager : IPrivacyUIManager
    {
        private const string TAG = "PRIVACY_UI_MANAGER";
        
        private Dictionary<string, object> _privacyTextDictionary;
        private PrivacyTexts _privacyTexts;
        private Dictionary<string, string> _constTexts;
        private IPrivacyCanvas _privacyCanvas;
        private VoodooSettings _voodooSettings;
        private PrivacyConsent _privacyConsent;
        private bool _isLearnMoreScreenInitialized;
        private bool _isParternScreenInitialized;
        private bool _isDeleteScreenInitialized;
        private bool _isAccessScreenInitialized;

        private PrivacyScreen.Parameters _privacyScreenParameters;
        
        public void Initialize(IPrivacyUIManagerParameters parameters)
        {
            _privacyCanvas = parameters.privacyCanvas;
            _voodooSettings = parameters.voodooSettings;
            var replaceTexts = new Dictionary<string, string> {
                { "@COLOR", "#" + ColorUtility.ToHtmlStringRGB(_voodooSettings.GdprPrimaryColor) },
                { "@GAME_NAME", Application.productName },
                { "@ANALYTICS_SDK_LIST", Environment.NewLine + string.Join(", ", PrivacyUtils.GetAnalyticsNames()) },
                { "@ADS_SDK_LIST", Environment.NewLine + string.Join(", ", PrivacyUtils.GetAdNetworkNames()) },
                { "@STUDIO_NAME", "Voodoo" },
                { "<br>", Environment.NewLine }
            };
            if (parameters.gdprTexts != null) {
                _privacyTexts = parameters.gdprTexts.FromJson<PrivacyTexts>(replaceTexts);
                if (_privacyTexts == null) {
                    VoodooLog.LogError(Module.PRIVACY, TAG, $"Couldn't parse string '{parameters.gdprTexts}' to JSON");
                    throw new Exception("Couldn't parse text to json.");
                }
            }
        }

        public async Task<PrivacyConsent> GetPrivacyConsent(Action startLimitedPlaytime, Action stopLimitedPlaytime, bool offerWallEnabled, bool openOfferWall)
        {
            _privacyConsent = null;
            _privacyScreenParameters = new PrivacyScreen.Parameters {
                mainColor = _voodooSettings.GdprPrimaryColor,
                title = _privacyTexts?.title,
                intro = _privacyTexts?.intro_thanks,
                outro = _privacyTexts?.outro_thanks,
                advertisingText = _privacyTexts?.consent_advertising,
                advertisingAction = OpenParternScreen,
                advertisingDefaultValue = false,
                advertisingActive =  true,
                analyticsText = _privacyTexts?.consent_analytics,
                analyticsAction = OpenParternScreen,
                analyticsDefaultValue = false,
                analyticsActive = true,
                ageText = _privacyTexts?.pgpd_sixteen,
                ageAction = OpenPolicyPage,
                ageDefaultValue = offerWallEnabled && openOfferWall,
                accept = _privacyTexts?.play,
                acceptCallback = (consent) => {
                    if (offerWallEnabled && _privacyTexts?.offer_wall != null && (!consent.adsConsent || !consent.analyticsConsent)) {
                        UnityAction startLimitedPlaytimeAction = null;
                        if (startLimitedPlaytime != null) {
                            startLimitedPlaytimeAction = () => {
                                startLimitedPlaytime.Invoke();
                                CloseOfferWallScreen();
                                UpdateConsent(consent);
                            };
                        }
                        UnityAction stopLimitedPlaytimeAction = null;
                        if (stopLimitedPlaytime != null) {
                            stopLimitedPlaytimeAction = () => {
                                stopLimitedPlaytime.Invoke();
                            };
                        }
                        _privacyCanvas.ClosePrivacyScreen();
                        OpenOfferWallScreen(startLimitedPlaytimeAction, stopLimitedPlaytimeAction);
                    } else {
                        stopLimitedPlaytime?.Invoke();
                        UpdateConsent(consent);
                    }
                },
                learnMore = _privacyTexts?.more,
                learnMoreAction = OpenLearnMoreScreen,
                automaticallyAccept = offerWallEnabled && openOfferWall,
                isSeparatorsActive = true
            };
            _privacyCanvas.OpenPrivacyScreen(_privacyScreenParameters);
            while (_privacyConsent == null)
            {
                await Task.Yield();
            }
            return _privacyConsent;
        }
        
        private void OpenPrivacyWithAllConsent()
        {
            _privacyScreenParameters.automaticallyAccept = false;
            _privacyScreenParameters.advertisingDefaultValue = true;
            _privacyScreenParameters.analyticsDefaultValue = true;
            _privacyScreenParameters.ageDefaultValue = true;
            _privacyCanvas.OpenPrivacyScreen(_privacyScreenParameters);
        }

        private void OpenLearnMoreScreen()
        {
            _privacyCanvas.ClosePrivacyScreen();
            if (!_isLearnMoreScreenInitialized) {
                var learnMoreScreenParameters = new LearnMoreScreen.Parameters {
                    mainColor = _voodooSettings.GdprPrimaryColor,
                    header = _privacyTexts?.title2,
                    intro = _privacyTexts?.intro2,
                    game = _privacyTexts?.game,
                    ads = _privacyTexts?.ads,
                    bug = _privacyTexts?.bug,
                    outro = _privacyTexts?.outro2,
                    backCallback = () => {
                        _privacyCanvas.CloseLearnMoreScreen();
                        _privacyCanvas.OpenPrivacyScreen();
                        PrivacyEventsManager.OnLearnMoreClick();
                    },
                    back = _privacyTexts?.back
                };
                _privacyCanvas.OpenLearnMoreScreen(learnMoreScreenParameters);
                _isLearnMoreScreenInitialized = true;
            } else {
                _privacyCanvas.OpenLearnMoreScreen();
            }
            PrivacyEventsManager.OnLearnMoreDisplay();
        }

        private void OpenParternScreen()
        {
            _privacyCanvas.ClosePrivacyScreen();
            if (!_isParternScreenInitialized) {
                var parternScreenParameters = new ParternsScreen.Parameters {
                    mainColor = _voodooSettings.GdprPrimaryColor,
                    header = _privacyTexts?.partners,
                    intro = _privacyTexts?.intro_partners,
                    advertising = _privacyTexts?.advertising,
                    advertisingUrls = PrivacyUtils.GetAdvertisingPrivacyPolicyUrls(),
                    analytics = _privacyTexts?.analytics,
                    analyticsUrls = PrivacyUtils.GetAnalyticsPrivacyPolicyUrls(),
                    backCallback = () => {
                        _privacyCanvas.CloseParternsScreen();
                        _privacyCanvas.OpenPrivacyScreen();
                    },
                    back = _privacyTexts?.back
                };
                _privacyCanvas.OpenPartnersScreen(parternScreenParameters);
                _isParternScreenInitialized = true;
            } else {
                _privacyCanvas.OpenPartnersScreen();
            }
        }

        private void OpenPolicyPage()
        {
            Application.OpenURL(_privacyTexts?.privacy_policy_url);
        }

        public void OpenSettingsScreen(bool adsConsent, bool analyticsConsent, bool idfaAuthorizationStatusDeniedOrRestricted, string idfa, 
                                       UnityAction<PrivacyUpdate> updatePrivacy, UnityAction<string> deleteData, 
                                       UnityAction<UserDetails> accessData, UnityAction tryOpenDebugger, 
                                       UnityAction closeSettingsScreen)
        {
            bool adTrackingEnabled = true;
            var settingsScreenParameters = new SettingsScreen.Parameters {
                mainColor = _voodooSettings.GdprPrimaryColor,
                title = _privacyTexts?.data_sharing,
                intro = (PlatformUtils.UNITY_IOS && idfaAuthorizationStatusDeniedOrRestricted ?
                        _privacyTexts?.data_sharing_subtitle + "\n\n" : "") + _privacyTexts?.intro3,
                advertisingValue = adsConsent,
                isAdvertisingLocked = !adTrackingEnabled,
                advertising = _privacyTexts?.advertising,
                advertisingUrls = PrivacyUtils.GetAdvertisingPrivacyPolicyUrls(),
                analyticsValue = analyticsConsent,
                isAnalyticsLocked = !adTrackingEnabled,
                analytics = _privacyTexts?.analytics,
                analyticsUrls = PrivacyUtils.GetAnalyticsPrivacyPolicyUrls(),
                isAccessDataActive = adTrackingEnabled,
                accessDataAction = () => {
                    OpenAccessDataScreen(idfa, accessData, tryOpenDebugger);
                    CloseSettingsScreen();
                },
                accessDataText = _privacyTexts?.access_data,
                deleteDataCallback = () => {
                    OpenDeleteScreen(idfa, deleteData);
                    CloseSettingsScreen();
                },
                isDeleteDataActive = adTrackingEnabled,
                deleteDataText = _privacyTexts?.delete_data,
                saveAction = updatePrivacy,
                saveText = _privacyTexts?.confirm,
                closeCallback = _privacyCanvas.CloseSettingsScreen,
                closeText = _privacyTexts?.close
            };
            _privacyCanvas.OpenSettingsScreen(settingsScreenParameters);
        }

        public void CloseSettingsScreen()
        {
            _privacyCanvas.CloseSettingsScreen();
        }
        
        private void OpenDeleteScreen(string idfa, UnityAction<string> deletePrivacy)
        {
            if (!_isDeleteScreenInitialized) {
                var deleteScreenParameters = new DeleteScreen.Parameters {
                    mainColor = _voodooSettings.GdprPrimaryColor,
                    title = _privacyTexts?.delete_data,
                    intro = _privacyTexts?.intro_delete,
                    emailTitle = _privacyTexts?.email,
                    idfaTitle = _privacyTexts?.idfa,
                    idfa = idfa,
                    deleteText = _privacyTexts?.delete_data,
                    deleteCallback = deletePrivacy,
                    closeText = _privacyTexts?.close,
                    closeCallback = () => {
                        _privacyCanvas.CloseDeleteScreen();
                        _privacyCanvas.OpenSettingsScreen();
                    }
                };
                _privacyCanvas.OpenDeleteScreen(deleteScreenParameters);
                _isDeleteScreenInitialized = true;
            } else {
                _privacyCanvas.OpenDeleteScreen();
            }
        }

        public void CloseDeleteScreen()
        {
            _privacyCanvas.CloseDeleteScreen();
        }

        private void OpenAccessDataScreen(string idfa, UnityAction<UserDetails> sendRequestAction, UnityAction tryOpenDebugger)
        {
            if (!_isAccessScreenInitialized) {
                var accessDataParameters = new AccessDataScreen.Parameters {
                    mainColor = _voodooSettings.GdprPrimaryColor,
                    title = _privacyTexts?.access_data,
                    intro = _privacyTexts?.intro4,
                    idfaTitle = _privacyTexts?.idfa,
                    idfa = idfa,
                    emailTitle = _privacyTexts?.email,
                    sendRequestAction = sendRequestAction,
                    sendRequestButtonText = _privacyTexts?.send,
                    closeAction = () => {
                        _privacyCanvas.CloseAccessDataScreen();
                        _privacyCanvas.OpenSettingsScreen();
                    },
                    closeButtonText = _privacyTexts?.close,
                    tryOpenDebugger = () => {
                        CloseAccessDataScreen();
                        tryOpenDebugger?.Invoke();
                    }
                };
                _privacyCanvas.OpenAccessDataScreen(accessDataParameters);
                _isAccessScreenInitialized = true;
            } else {
                _privacyCanvas.OpenAccessDataScreen();
            }
        }

        public void CloseAccessDataScreen()
        {
            _privacyCanvas.CloseAccessDataScreen();
        }

        private void OpenPopupScreen(string title, string message, string closeButtonText, UnityAction closeCallback)
        {
            var popupScreenParameters = new PopupScreen.Parameters {
                mainColor = _voodooSettings.GdprPrimaryColor,
                title = title,
                message = message,
                closeCallback = closeCallback,
                closeButtonText = closeButtonText
            };
            _privacyCanvas.OpenPopupScreen(popupScreenParameters);
        }
        
        public void ClosePopupScreen()
        {
            _privacyCanvas.ClosePopupScreen();
        }

        public void OpenPrivacyNotInitializedPopupScreen(UnityAction closeCallback, VoodooSettings voodooSettings, IPrivacyCanvas privacyCanvas)
        {
            _voodooSettings = voodooSettings;
            _privacyCanvas = privacyCanvas;
            // the following texts are hard coded on purpose because at this point the class is not yet initialized
            string title = "Error";
            string message = "Sorry, you must have an internet connection to access and change your privacy settings.";
            string closeButtonText = "Close";
            OpenPopupScreen(title, message, closeButtonText, closeCallback);
        }

        public void OpenUpdatePrivacySuccessPopupScreen(UnityAction closeCallback)
        {
            string title = _privacyTexts?.success_title;
            string message = "Your request has been registered, please restart the app to apply the changes.";
            string closeButtonText = _privacyTexts?.close;
            OpenPopupScreen(title, message, closeButtonText, closeCallback);
        }
        
        public void OpenUpdatePrivacyErrorPopupScreen(UnityAction closeCallback)
        {
            string title = _privacyTexts?.error_title;
            string message = _privacyTexts?.error;
            string closeButtonText = _privacyTexts?.close;
            OpenPopupScreen(title, message, closeButtonText, closeCallback);
        }

        public void OpenSuccessPopupScreen(UnityAction closeCallback)
        {
            string title = _privacyTexts?.success_title;
            string message = _privacyTexts?.success;
            string closeButtonText = _privacyTexts?.close;
            OpenPopupScreen(title, message, closeButtonText, closeCallback);
        }

        public void OpenErrorPopupScreen(string errorMessage, UnityAction closeCallback)
        {
            string title = _privacyTexts?.error_title;
            string message = errorMessage;
            string closeButtonText = _privacyTexts?.close;
            OpenPopupScreen(title, message, closeButtonText, closeCallback);
        }
        
        public void OpenConnexionErrorPopupScreen(UnityAction closeCallback)
        {
            string title = _privacyTexts?.error_title;
            string message = _privacyTexts?.error;
            string closeButtonText = _privacyTexts?.close;
            OpenPopupScreen(title, message, closeButtonText, closeCallback);
        }
        
        public void OpenEmailErrorPopupScreen(UnityAction closeCallback)
        {
            string title = _privacyTexts?.error_title;
            string message = "Invalid email address.";
            string closeButtonText = _privacyTexts?.close;
            OpenPopupScreen(title, message, closeButtonText, closeCallback);
        }

        private void OpenOfferWallScreen(UnityAction startLimitedPlaytime, UnityAction stopLimitedPlaytime)
        {
            UnityAction consentAction = () => {
                _privacyCanvas.CloseOfferWallScreen();
                OpenPrivacyWithAllConsent();
                PrivacyEventsManager.OnConsentOrPurchaseClick(PrivacyEventsManager.ConsentOrPurchasePopupButton.consent);
            };
            UnityAction purchaseAction = () => {
                Action onPurchaseComplete = () => {
                    VoodooSauce.EnablePremium();
                    _privacyConsent = new PrivacyConsent {
                        isSixteenOrOlder = true,
                        adsConsent = false,
                        analyticsConsent = false
                    };
                    stopLimitedPlaytime?.Invoke();
                    _privacyCanvas.CloseOfferWallScreen();
                    PrivacyEventsManager.OnPurchaseClick(PrivacyEventsManager.PurchasePopupButton.purchase);
                };
                Action onPurchaseFailure = () => {
                    PrivacyEventsManager.OnPurchaseClick(PrivacyEventsManager.PurchasePopupButton.consent);
                };
                VoodooSettings settings = VoodooSettings.Load();

                VoodooSauce.RegisterPurchaseDelegate(new NoAdsPurchaseDelegate(onPurchaseComplete, onPurchaseFailure));
                VoodooSauce.Purchase(settings.NoAdsBundleId);
                PrivacyEventsManager.OnConsentOrPurchaseClick(PrivacyEventsManager.ConsentOrPurchasePopupButton.purchase);
                PrivacyEventsManager.OnPurchaseDisplay();
            };
            UnityAction limitedPlaytimeAction = null;
            if (startLimitedPlaytime != null) {
                limitedPlaytimeAction = () => {
                    startLimitedPlaytime.Invoke();
                    PrivacyEventsManager.OnPurchaseClick(PrivacyEventsManager.PurchasePopupButton.limitedPlayTime);
                };
            }
            OpenOfferWallScreen(consentAction, purchaseAction, limitedPlaytimeAction);
        }

        private void OpenOfferWallScreen(UnityAction consentAction, UnityAction purchaseAction, UnityAction limitedPlayTimeAction)
        {
            PrivacyConfiguration config = VoodooSauce.GetItemOrDefault<PrivacyConfiguration>();
            var consentOrPurchaseScreenParameters = new OfferWallScreen.Parameters
            {
                title = _privacyTexts?.offer_wall?.title,
                body = _privacyTexts?.offer_wall?.body,
                consentAction = consentAction,
                consentText = _privacyTexts?.offer_wall?.consentButtonText,
                purchaseAction = purchaseAction,
                purchaseText = _privacyTexts?.offer_wall?.purchaseButtonText,
                limitedPlaytimeAction = limitedPlayTimeAction,
                limitedPlaytimeText = string.Format(_privacyTexts?.offer_wall?.limitedPlayTimeButtonText,
                    config.limitedPlaytimeInMinute.ToString(), config.limitedPlaytimeInMinute > 1 ? "s" : "")
            };
            _privacyCanvas.OpenOfferWallScreen(consentOrPurchaseScreenParameters);
            PrivacyEventsManager.OnConsentOrPurchaseDisplay();
        }

        public void CloseOfferWallScreen()
        {
            _privacyCanvas.CloseOfferWallScreen();
        }

        public void OpenLoadingScreen()
        {
            _privacyCanvas.OpenLoadingScreen();
        }

        public void CloseLoadingScreen()
        {
            _privacyCanvas.CloseLoadingScreen();
        }

        private void UpdateConsent(PrivacyConsent consent)
        {
            _privacyConsent = consent;
            _privacyCanvas.ClosePrivacyScreen();
        }
    }
}