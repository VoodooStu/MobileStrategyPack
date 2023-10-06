using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Sauce.Privacy.UI;

namespace Voodoo.Sauce.Privacy
{
    [Preserve]
    internal partial class PrivacyManager : PrivacyCore
    {
        private const string TAG = "PrivacyManager";

        private const int CONSENT_VALIDITY_PERIOD_IN_DAY = 182;

        private VoodooSettings _voodooSettings;
        private Action<GdprConsent> _onConsentGiven;
        private Action<GdprConsent> _onConsentUpdate;
        private Action<bool> _onPrivacyShown;
        private Action _onInternetAvailable;
        private Action _onGameStarted;
        private Action _onGameFinished;
        private Action _tryOpenDebugger;
        private Action _onDeleteDataRequest;
        private Func<bool> _hasGameStarted;
        private IPrivacyCanvas _privacyCanvas;
        private IRequest _request;
        private string _gdprTexts;
        private bool _isInitialized;
        private bool _showOfferWall;
        private PrivacyConfiguration _privacyConfiguration;

        private bool _alreadyAppliedGdpr;
        
        private int _startPlayDate;
        private int _limitedPlayTimeInSecond;

        private IPrivacyUIManager _uiManager;

        private IPrivacyUIManager UIManager => _uiManager ?? (_uiManager = new PrivacyUIManager());

        private NeedConsent _serverConsent;
        private NeedConsent.OfferWallStatus _offerWallStatus;
        private Action _onSettingsClosed;

        private IPrivacyAPI _privacyAPI;

        private IPrivacyAPI PrivacyAPI {
            get {
                if (_privacyAPI == null) {
                    _privacyAPI = new PrivacyAPI();
                    _privacyAPI.Initialize(_request);
                }

                return _privacyAPI;
            }
        }

        public override async Task Instantiate(VoodooSettings voodooSettings)
        {
            _voodooSettings = voodooSettings;
            await SynchronizeAdvertisingId();
        }

        public override void Initialize(PrivacyManagerParameters parameters)
        {
            _voodooSettings = parameters.voodooSettings;
            _onConsentGiven = parameters.onConsentGiven;
            _onConsentUpdate = parameters.onConsentUpdate;
            _onPrivacyShown = parameters.onPrivacyShown;
            parameters.onInternetAvailable += () => { _onInternetAvailable?.Invoke(); };
            parameters.onGameStarted += () => { _onGameStarted?.Invoke(); };
            parameters.onGameFinished += () => { _onGameFinished?.Invoke(); };
            _hasGameStarted = parameters.hasGameStarted;
            _tryOpenDebugger = parameters.tryOpenDebugger;
            _onDeleteDataRequest = parameters.onDeleteDataRequest;
            _privacyCanvas = parameters.canvas;
            _request = parameters.request;
            _privacyAPI = parameters.privacyAPI;
            _uiManager = parameters.privacyUIManager;
            _privacyConfiguration = parameters.privacyConfiguration;
            LoadCache();
            UIManager.Initialize(new IPrivacyUIManagerParameters {
                    privacyCanvas = _privacyCanvas,
                    voodooSettings = _voodooSettings,
                    gdprTexts = null
                });
            TrySynchronizeWithServer();
        }

        internal override void ShowPrivacyAuthorization()
        {
            UpdateConsent();
        }

        public override string GetAdvertisingId(bool forceZerosForLimitedAdTracking = true)
        {
            if (forceZerosForLimitedAdTracking && !_adTrackingEnabled)
                return LIMITED_AD_TRACKING_ID;
            return _idfa;
        }

#region PrivacySettings

        internal override void OpenPrivacySettings(Action onSettingsClosed = null)
        {
            _onSettingsClosed = onSettingsClosed;
            if (!_isInitialized)
                UIManager.OpenPrivacyNotInitializedPopupScreen(() => UIManager.ClosePopupScreen(),
                    _voodooSettings, _privacyCanvas);
            else
                UIManager.OpenSettingsScreen(_adsConsent, _analyticsConsent,
                    GetAuthorizationStatus() == IdfaAuthorizationStatus.Denied || GetAuthorizationStatus() == IdfaAuthorizationStatus.Restricted,
                    _idfa, UpdatePrivacyConsent, DeleteData, RequestAccessToData, _tryOpenDebugger.Invoke, ClosePrivacySettingsDialog);
        }

        private void ClosePrivacySettingsDialog()
        {
            UIManager.CloseSettingsScreen();
            _onSettingsClosed?.Invoke();
        }

        private async void UpdatePrivacyConsent(PrivacyUpdate privacyUpdate)
        {
            if (_isGdprApplicable && (_adsConsent != privacyUpdate.adsConsent || _analyticsConsent != privacyUpdate.analyticsConsent)) {
                UIManager.OpenLoadingScreen();
                if (await SendConsent(privacyUpdate.adsConsent, privacyUpdate.analyticsConsent)) {
                    UpdateCachedConsent(privacyUpdate);
                    UIManager.CloseLoadingScreen();
                    UIManager.OpenUpdatePrivacySuccessPopupScreen(() => {
                        UIManager.ClosePopupScreen();
                        UIManager.CloseSettingsScreen();
                        _onSettingsClosed?.Invoke();
                    });
                } else {
                    UIManager.CloseLoadingScreen();
                    UIManager.OpenUpdatePrivacyErrorPopupScreen(() => {
                        UIManager.ClosePopupScreen();
                        UIManager.CloseSettingsScreen();
                        _onSettingsClosed?.Invoke();
                    });
                }
            } else if (_adsConsent != privacyUpdate.adsConsent || _analyticsConsent != privacyUpdate.analyticsConsent) {
                UpdateCachedConsent(privacyUpdate);
                UIManager.OpenUpdatePrivacySuccessPopupScreen(() => {
                    UIManager.ClosePopupScreen();
                    UIManager.CloseSettingsScreen();
                    _onSettingsClosed?.Invoke();
                });
            } else {
                UIManager.CloseSettingsScreen();
                _onSettingsClosed?.Invoke();
            }
        }

        private void UpdateCachedConsent(PrivacyUpdate privacyUpdate)
        {
            _adsConsent = privacyUpdate.adsConsent;
            CacheAdsConsent(_adsConsent);
            _analyticsConsent = privacyUpdate.analyticsConsent;
            CacheAnalyticsConsent(_analyticsConsent);
            PlayerPrefs.Save();
            _gdprConsent = new GdprConsent
            {
                ExplicitConsentGivenForAds = privacyUpdate.adsConsent,
                ExplicitConsentGivenForAnalytics = privacyUpdate.analyticsConsent,
                IsEmbargoedCountry = _serverConsent.embargoed_country,
                IsGdprApplicable = _serverConsent.is_gdpr,
                IsAdsEnforcement = _serverConsent.ads_enforcement,
                IsAdjustEnforcement = _serverConsent.adjust_enforcement,
                IsVANEnforcement = _serverConsent.van_enforcement
            };
            
            _onConsentUpdate(_gdprConsent);
        }

        private async void DeleteData(string email)
        {
            if (!PrivacyUtils.IsValidEmail(email)) {
                UIManager.OpenEmailErrorPopupScreen(UIManager.ClosePopupScreen);
                return;
            }

            UIManager.OpenLoadingScreen();
            var parameters = new DeleteDataParameters {
                uuid = _idfa,
                appVersion = PrivacyUtils.GetAppVersion(),
                vsVersion = PrivacyUtils.GetVSVersion(),
                bundleId = Application.identifier,
                platform = PrivacyUtils.GetOsType(),
                locale = PrivacyUtils.GetLocale(),
                email = email,
                studioName = "Voodoo",
                gdprType = (int) PrivacyUtils.GetGDPRType(),
#if UNITY_IOS
                vendor_id = _vendorId,
#else
                vendor_id = null,
#endif
                voodoo_user_id = AnalyticsUserIdHelper.GetUserId()
            };
            PrivacyRequest privacyRequest = await PrivacyAPI.DeleteDataRequest(parameters);
            UIManager.CloseLoadingScreen();
            if (privacyRequest == null) {
                UIManager.OpenConnexionErrorPopupScreen(() => { UIManager.ClosePopupScreen(); });
            } else if (privacyRequest.success == "true") {
                CacheUserRequestedToBeForgotten(true);
                _onDeleteDataRequest?.Invoke();
                UIManager.OpenSuccessPopupScreen(() => {
                    UIManager.ClosePopupScreen();
                    UIManager.CloseDeleteScreen();
                    _onSettingsClosed?.Invoke();
                });
            } else {
                UIManager.OpenErrorPopupScreen(privacyRequest.errorMessage, () => { UIManager.ClosePopupScreen(); });
            }
        }

        private async void RequestAccessToData(UserDetails userDetails)
        {
            if (!PrivacyUtils.IsValidEmail(userDetails.email)) {
                UIManager.OpenEmailErrorPopupScreen(UIManager.ClosePopupScreen);
                return;
            }

            UIManager.OpenLoadingScreen();
            var parameters = new AccessDataParameters {
                uuid = _idfa,
                appVersion = PrivacyUtils.GetAppVersion(),
                vsVersion = PrivacyUtils.GetVSVersion(),
                bundleId = Application.identifier,
                platform = PrivacyUtils.GetOsType(),
                locale = PrivacyUtils.GetLocale(),
                email = userDetails.email,
                studioName = "Voodoo",
                gdprType = (int) PrivacyUtils.GetGDPRType(),
                vendor_id = _vendorId,
                voodoo_user_id = AnalyticsUserIdHelper.GetUserId()
            };
            PrivacyRequest privacyRequest = await PrivacyAPI.AccessDataRequest(parameters);
            UIManager.CloseLoadingScreen();
            if (privacyRequest == null) {
                UIManager.OpenConnexionErrorPopupScreen(() => { UIManager.ClosePopupScreen(); });
            } else if (privacyRequest.success == "true") {
                UIManager.OpenSuccessPopupScreen(() => {
                    UIManager.ClosePopupScreen();
                    UIManager.CloseAccessDataScreen();
                    _onSettingsClosed?.Invoke();
                });
            } else {
                UIManager.OpenErrorPopupScreen(privacyRequest.errorMessage, () => { UIManager.ClosePopupScreen(); });
            }
        }

#endregion

        private async Task SynchronizeAdvertisingId()
        {
#if UNITY_EDITOR
            _vendorId = "";
#elif UNITY_IOS
            _vendorId = UnityEngine.iOS.Device.vendorIdentifier;
            _idfaAuthorizationStatus = NativeWrapper.GetAuthorizationStatus();
#elif UNITY_ANDROID 
            _vendorId = await VendorIdHelperAndroid.RequestVendorId();
#endif
            AdvertisingStatus advertisingStatus = await IdfaHelper.RequestAdvertisingId(_voodooSettings, this);
            _idfa = advertisingStatus.Idfa;
            _adTrackingEnabled = advertisingStatus.AdTrackingEnabled;
#if !UNITY_IOS
            _idfaAuthorizationStatus = HasLimitAdTrackingEnabled() ? IdfaAuthorizationStatus.Denied : IdfaAuthorizationStatus.Authorized;
#endif
        }

#if UNITY_IOS
        private async Task DisplayATTPopup()
        {
            PrivacyEventsManager.OnAppleIdfaAuthorizationDisplay();
            _idfaAuthorizationStatus = await NativeWrapper.RequestAuthorization();
            PrivacyEventsManager.ApplePopupButton applePopupButton = _idfaAuthorizationStatus == IdfaAuthorizationStatus.Authorized
                ? PrivacyEventsManager.ApplePopupButton.allow : PrivacyEventsManager.ApplePopupButton.deny;
            PrivacyEventsManager.OnAppleIdfaAuthorizationClick(applePopupButton);
            AdvertisingStatus advertisingStatus = await IdfaHelper.RequestAdvertisingId(_voodooSettings, this);
            _idfa = advertisingStatus.Idfa;
            _adTrackingEnabled = advertisingStatus.AdTrackingEnabled;
        }
#endif


        private void TrySynchronizeWithServer()
        {
            if (IsInternetAvailable()) {
                SynchronizeWithServer();
            } else {
                OnSynchronizeWithServerEnd(false);
                _onInternetAvailable += SynchronizeWithServer;
            }
        }

        private async void SynchronizeWithServer()
        {
            _onInternetAvailable -= SynchronizeWithServer;
            _serverConsent = await RequestConsent();
            if (_serverConsent != null) {
                _isGdprApplicable = _serverConsent.is_gdpr;
                _isCcpaApplicable = _serverConsent.is_ccpa;
                _offerWallStatus = _serverConsent.OfferWall;
                UIManager.Initialize(new IPrivacyUIManagerParameters {
                    privacyCanvas = _privacyCanvas,
                    voodooSettings = _voodooSettings,
                    gdprTexts = _serverConsent.texts
                });
                if (IsCachePresent()) {
                    if (NeedUpdateLocalConsent()) {
                        TryUpdateConsent();
                    } else if (_isGdprApplicable && _offerWallStatus != NeedConsent.OfferWallStatus.Disabled) {
                        OfferWallFlow();
                    } else {
                        OnSynchronizeWithServerEnd(true);
                        if (NeedSendConsent()) TrySendConsent();
                    }
                } else {
                    if (_isGdprApplicable) {
                        TryUpdateConsent();
                    } else {
                        CacheUserRequestedToBeForgotten(false);
                        CacheSixteenOrOlder(true);
                        _adsConsent = true;
                        CacheAdsConsent(_adsConsent);
                        _analyticsConsent = true;
                        CacheAnalyticsConsent(_analyticsConsent);
                        OnSynchronizeWithServerEnd(true);
                    }
                }
            } else {
                OnSynchronizeWithServerEnd(false);
                if (!IsInternetAvailable()) {
                    _onInternetAvailable += SynchronizeWithServer;
                }
            }
        }


        private async void OnSynchronizeWithServerEnd(bool isSuccess)
        {
#if UNITY_IOS
            if (!PlatformUtils.UNITY_EDITOR && _idfaAuthorizationStatus == IdfaAuthorizationStatus.NotDetermined) {
                await DisplayATTPopup();
            }
#endif
            _isInitialized = isSuccess;
            _gdprConsent = new GdprConsent
            {
                ExplicitConsentGivenForAds = _adsConsent,
                ExplicitConsentGivenForAnalytics = _analyticsConsent,
                IsEmbargoedCountry = isSuccess ? _serverConsent.embargoed_country : false,
                IsGdprApplicable = isSuccess ? _serverConsent.is_gdpr : _alreadyAppliedGdpr,
                IsAdsEnforcement = isSuccess ? _serverConsent.ads_enforcement : false, 
                IsAdjustEnforcement = isSuccess ? _serverConsent.adjust_enforcement : false,
                IsVANEnforcement = isSuccess ? _serverConsent.van_enforcement : false
            };
            
            _onConsentGiven(_gdprConsent);
        }

        private void TryUpdateConsent()
        {
            if (!_hasGameStarted.Invoke()) {
                UpdateConsent();
            } else {
                _onGameFinished += UpdateConsent;
            }
        }

        private async void UpdateConsent()
        {
            PrivacyConsent privacyConsent = await AskConsent();
#if UNITY_IOS
            if (!PlatformUtils.UNITY_EDITOR && _idfaAuthorizationStatus == IdfaAuthorizationStatus.NotDetermined) {
                await DisplayATTPopup();
            }
#endif
            _isInitialized = true;
            CacheUserRequestedToBeForgotten(false);
            CacheConsent(privacyConsent);
            if (_serverConsent.is_gdpr) {
                _alreadyAppliedGdpr = _serverConsent.is_gdpr;
                CacheNeedSendConsent(_serverConsent.is_gdpr);
                CachePrivacyVersion(_serverConsent.privacy_version);
                CachePrivacyList();
                CacheConsentDate();
                CacheAdTrackingEnabled(_adTrackingEnabled);
                TrySendConsent();
            }
            _onGameFinished -= UpdateConsent;
            
            _gdprConsent = new GdprConsent
            {
                ExplicitConsentGivenForAds = privacyConsent.adsConsent,
                ExplicitConsentGivenForAnalytics = privacyConsent.analyticsConsent,
                IsEmbargoedCountry = _serverConsent.embargoed_country,
                IsGdprApplicable = _serverConsent.is_gdpr,
                IsAdsEnforcement = _serverConsent.ads_enforcement, 
                IsAdjustEnforcement = _serverConsent.adjust_enforcement,
                IsVANEnforcement = _serverConsent.van_enforcement
            };
            
            _onConsentGiven(_gdprConsent);
        }

        private void TrySendConsent()
        {
            if (IsInternetAvailable()) {
                SendConsent();
            } else {
                _onInternetAvailable += SendConsent;
            }
        }

        private async Task<PrivacyConsent> AskConsent()
        {
            PrivacyEventsManager.OnGDPRDisplay();
            _onPrivacyShown(true);
            Action startLimitedPlaytime = null;
            if (_offerWallStatus == NeedConsent.OfferWallStatus.LimitedPlayTime && (_privacyConfiguration.limitedPlaytimeInMinute > 0) &&
                (GetLastPlaytimeInSecond() < 60 * _privacyConfiguration.limitedPlaytimeInMinute)) {
                startLimitedPlaytime = StartLimitedPlaytime;
            }
            Action stopLimitedPlaytime = StopLimitedPlaytime;
            bool offerWallEnabled = _offerWallStatus != NeedConsent.OfferWallStatus.Disabled;
            bool openOfferWall = _showOfferWall;
            PrivacyConsent consent = await UIManager.GetPrivacyConsent(startLimitedPlaytime, stopLimitedPlaytime, offerWallEnabled, openOfferWall);
            _onPrivacyShown(false);
            PrivacyEventsManager.OnGDPRProceedClick(consent.adsConsent,consent.analyticsConsent);
            return consent;
        }

        private void OfferWallFlow()
        {
            if (VoodooSauce.IsPremium() || (_adsConsent && _analyticsConsent)) {
                OnSynchronizeWithServerEnd(true);
                if (NeedSendConsent()) TrySendConsent();
            } else if (_offerWallStatus == NeedConsent.OfferWallStatus.LimitedPlayTime && _privacyConfiguration.limitedPlaytimeInMinute > 0) {
                if (TimeUtils.GetDaysFromSeconds(TimeUtils.NowAsTimeStamp()) != TimeUtils.GetDaysFromSeconds(GetLastPlayDate())) {
                    _showOfferWall = true;
                    TryUpdateConsent();
                } else {
                    int limitedPlayTimeInSecond = 60 * _privacyConfiguration.limitedPlaytimeInMinute;
                    if (GetLastPlaytimeInSecond() >= limitedPlayTimeInSecond) {
                        _showOfferWall = true;
                        TryUpdateConsent();
                    } else {
                        StartLimitedPlaytime();
                        OnSynchronizeWithServerEnd(true);
                        if (NeedSendConsent()) TrySendConsent();
                    }
                }
            } else {
                _showOfferWall = true;
                TryUpdateConsent();
            }
        }

        private void StartLimitedPlaytime()
        {
            _limitedPlayTimeInSecond = 60 * _privacyConfiguration.limitedPlaytimeInMinute;
            if (TimeUtils.GetDaysFromSeconds(TimeUtils.NowAsTimeStamp()) != TimeUtils.GetDaysFromSeconds(GetLastPlayDate())) {
                CacheCurrentPlayDate();
                CacheCurrentPlaytime(0);
            }
            if (_onGameStarted == null) {
                _onGameStarted += OnGameStartedLimitedPlaytime;
                _onGameFinished += OnGameFinishedLimitedPlaytime;
            }
        }

        private void StopLimitedPlaytime()
        {
            _onGameStarted -= OnGameStartedLimitedPlaytime;
            _onGameFinished -= OnGameFinishedLimitedPlaytime;
        }

        private void OnGameStartedLimitedPlaytime()
        {
            _startPlayDate = TimeUtils.NowAsTimeStamp();
        }

        private void OnGameFinishedLimitedPlaytime()
        {
            if (TimeUtils.GetDaysFromSeconds(TimeUtils.NowAsTimeStamp()) != TimeUtils.GetDaysFromSeconds(GetLastPlayDate())) {
                _showOfferWall = true;
                TryUpdateConsent();
            } else {
                int stopPlayDate = TimeUtils.NowAsTimeStamp();
                if (_startPlayDate <= 0 || stopPlayDate - _startPlayDate < 0) _startPlayDate = TimeUtils.NowAsTimeStamp();
                int playtime = stopPlayDate - _startPlayDate + GetLastPlaytimeInSecond();
                CacheCurrentPlaytime(playtime);
                if (!VoodooSauce.IsPremium() && (!_adsConsent || !_analyticsConsent) && playtime >= _limitedPlayTimeInSecond) {
                    _showOfferWall = true;
                    UpdateConsent();
                }
            }
        }

        private Task<NeedConsent> RequestConsent()
        {
            var needConsentParams = new NeedConsentParams {
                bundle_id = Application.identifier,
                popup_version = PrivacyUtils.GetPopupVersion(),
                user = PrivacyUtils.GetUserStatus(),
                os_type = PrivacyUtils.GetOsType(),
                app_version = PrivacyUtils.GetAppVersion(),
                vs_version = PrivacyUtils.GetVSVersion(),
                locale = PrivacyUtils.GetLocale(),
                uuid = _idfa,
                studio_name = "Voodoo",
                gdpr_type = (int) PrivacyUtils.GetGDPRType(),
#if UNITY_IOS
                vendor_id = _vendorId,
#else
                vendor_id = null,
#endif
                voodoo_user_id = AnalyticsUserIdHelper.GetUserId(),
                idfa_authorization_status = _idfaAuthorizationStatus.ToString()
            };
            return PrivacyAPI.NeedConsent(needConsentParams);
        }

        private async void SendConsent()
        {
            _onInternetAvailable -= SendConsent;
            var consentInsightsParams = new ConsentInsightsParams {
                bundle_id = Application.identifier,
                popup_version = PrivacyUtils.GetPopupVersion(),
                user = PrivacyUtils.GetUserStatus(),
                ads_consent = _adsConsent,
                analytics_consent = _analyticsConsent,
                os_type = PrivacyUtils.GetOsType(),
                app_version = PrivacyUtils.GetAppVersion(),
                vs_version = PrivacyUtils.GetVSVersion(),
                locale = PrivacyUtils.GetLocale(),
                uuid = _idfa,
                studio_name = "Voodoo",
                gdpr_type = (int) PrivacyUtils.GetGDPRType(),
                vendor_id = _vendorId,
                voodoo_user_id = AnalyticsUserIdHelper.GetUserId(),
                idfa_authorization_status = _idfaAuthorizationStatus.ToString()
            };
            bool success = await PrivacyAPI.ConsentInsights(consentInsightsParams);
            if (success) CacheNeedSendConsent(false);
            else _onInternetAvailable += SendConsent;
        }

        private async Task<bool> SendConsent(bool adsConsent, bool analyticsConsent)
        {
            _onInternetAvailable -= SendConsent;
            var consentInsightsParams = new ConsentInsightsParams {
                bundle_id = Application.identifier,
                popup_version = PrivacyUtils.GetPopupVersion(),
                user = PrivacyUtils.GetUserStatus(),
                ads_consent = adsConsent,
                analytics_consent = analyticsConsent,
                os_type = PrivacyUtils.GetOsType(),
                app_version = PrivacyUtils.GetAppVersion(),
                vs_version = PrivacyUtils.GetVSVersion(),
                locale = PrivacyUtils.GetLocale(),
                uuid = _idfa,
                studio_name = "Voodoo",
                gdpr_type = (int) PrivacyUtils.GetGDPRType(),
                vendor_id = _vendorId,
                voodoo_user_id = AnalyticsUserIdHelper.GetUserId(),
                idfa_authorization_status = _idfaAuthorizationStatus.ToString()
            };
            return await PrivacyAPI.ConsentInsights(consentInsightsParams);
        }

        private void CacheConsent(PrivacyConsent privacyConsent)
        {
            CacheSixteenOrOlder(privacyConsent.isSixteenOrOlder);
            _adsConsent = privacyConsent.adsConsent;
            CacheAdsConsent(_adsConsent);
            _analyticsConsent = privacyConsent.analyticsConsent;
            CacheAnalyticsConsent(_analyticsConsent);
            PlayerPrefs.Save();
        }

        private void LoadCache()
        {
            if (IsCachePresent()) {
                _adsConsent = GetAdsConsent();
                _analyticsConsent = GetAnalyticsConsent();
                _alreadyAppliedGdpr = GetAlreadyAppliedGdpr();
            }
        }

        private bool NeedUpdateLocalConsent() => _serverConsent.is_gdpr &&
            (NeedConsentToNewVoodooPolicy() ||
                NeedConsentToNew3rdPartyPolicy() ||
                ConsentNotUpToDate() ||
                AdTrackingIsReEnabled());

        //if the privacy version server side is higher than the privacy version the user consented
        private bool NeedConsentToNewVoodooPolicy() => _serverConsent.privacy_version != GetPrivacyVersion();

        //compare the privacy policy list user consented and the actual privacy policy list
        //if there is a new privacy policy we have to ask again the user consent
        private bool NeedConsentToNew3rdPartyPolicy() => PrivacyUtils.GetPrivacyPolicyUrls().ToArray().Except(GetPrivacyList()).Any();

        //Consent must be ask again every 6months
        private static bool ConsentNotUpToDate()
        {
            int lastConsentDateTimestamp = GetConsentDate();
            int nowTimestamp = TimeUtils.NowAsTimeStamp();
            return lastConsentDateTimestamp > 0 && 
                   TimeUtils.GetDaysFromSeconds(nowTimestamp - lastConsentDateTimestamp) >= CONSENT_VALIDITY_PERIOD_IN_DAY;
        }

        //check if the app is moved from LAT to no LAT
        private bool AdTrackingIsReEnabled() => !GetCachedAdTrackingEnabled() && _adTrackingEnabled;

        private bool IsInternetAvailable() => Application.internetReachability != NetworkReachability.NotReachable;
    }
}
