using System;
using AmazonAds;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Utils;

#pragma warning disable 618

// ReSharper disable once CheckNamespace
// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable ConvertToLocalFunction
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    [Preserve]
    public class MaxMediationAdapter : IMediationAdapter
    {
        private const string TAG = "MaxMediationAdapter";
        private static readonly Vector2 MrecAdSizeDp = new Vector2(300, 250);

        private AdUnits _adUnits;
        private bool _appOpenEnabled;
        private bool _hasPaidToHideAds;

        private bool _rewardedReceived;
        private MaxAdInfo _bannerAdInfo;
        private MaxAdInfo _rewardedAdInfo;
        private MaxAdInfo _interstitialAdInfo;
        private MaxAdInfo _appOpenAdInfo;
        private MaxAdInfo _mrecAdInfo;
        private MaxAdInfo _nativeAdInfo;
        private ExponentialBackoff.ExpBackoff _interstitialLoadingBackoff;
        private ExponentialBackoff.ExpBackoff _rewardedLoadingBackoff;
        private ExponentialBackoff.ExpBackoff _appOpenAdLoadingBackoff;
        private ExponentialBackoff.ExpBackoff _mrecLoadingBackoff;
        private ExponentialBackoff.ExpBackoff _nativeAdsLoadingBackoff;

        private bool _bannerInitialized;
        private bool _bannerCreated;
        private bool _interstitialInitialized;
        private bool _rewardedVideoInitialized;
        private bool _appOpenAdInitialized;
        private bool _mrecInitialized;
        private bool _mrecCreated;
        private bool _nativeAdInitialized;
        private float _screenDensity;
        private static bool _isMaxNativeSdkInitialized;

        private bool _isInterstitialShownInsteadOfRewarded;
        
#region adapter interface

        public void Initialize(AdsKeys keys, bool hasPaidToHideAds, bool consent, bool isCcpaApplicable, Action sdkInitialized)
        {
            _adUnits = keys.AdUnits;
            _hasPaidToHideAds = hasPaidToHideAds;
            _appOpenEnabled = keys.enableAppOpenAdSoftLaunch;
            
            _interstitialLoadingBackoff = ExponentialBackoff.CreateExpBackoff(LoadInterstitial, TAG + ": LoadInterstitial");
            _rewardedLoadingBackoff = ExponentialBackoff.CreateExpBackoff(LoadRewardedAd, TAG + ": LoadRewardedAd");
            _appOpenAdLoadingBackoff = ExponentialBackoff.CreateExpBackoff(LoadAppOpenAd, TAG + ": LoadAppOpenAd");
            _nativeAdsLoadingBackoff = ExponentialBackoff.CreateExpBackoff(LoadNativeAd, TAG + ": LoadNativeAds");
            _mrecLoadingBackoff = ExponentialBackoff.CreateExpBackoff(LoadMrec, TAG + ": LoadMrec");
            
            if (_adUnits?.IsEmpty() == true) {
                VoodooLog.LogError(Module.ADS, TAG, "No AdUnits found for this platform.");
                OnSdkInitialized();
                return;
            }

            MaxSdk.SetVerboseLogging(VoodooLog.IsDebugLogsEnabled);

            // Set Max UserId
            MaxSdk.SetUserId(AnalyticsUserIdHelper.GetUserId());
            
            // Set Max to lock current orientation to prevent intermittent crash
            MaxSdk.SetExtraParameter("lock_current_orientation", "true");

            // I have created a variable for storing the callback to get a reference and unsubscribe it on itself.
            Action<MaxSdkBase.SdkConfiguration> maxAdsInitCallback = null;
            maxAdsInitCallback = config => {
                // MaxSdk can fail its initialization in some sporadic cases.
                // If that happens, MaxAds will trigger the callback again after its successful initialization.
                if (MaxSdk.IsInitialized()) {
                    _isMaxNativeSdkInitialized = true;
                    OnSdkInitialized();
                    sdkInitialized?.Invoke();
                    MaxSdkCallbacks.OnSdkInitializedEvent -= maxAdsInitCallback;
                }
            };
            MaxSdk.SetSdkKey(keys.maxSdkKey);
            MaxSdkCallbacks.OnSdkInitializedEvent += maxAdsInitCallback;

            AmazonWrapper.Initialize(VoodooSettings.Load().AmazonKey);
            SetConsent(consent, isCcpaApplicable);
            MaxSdk.InitializeSdk(_adUnits?.ExportToStringList(_hasPaidToHideAds));
        }

        private static void OnSdkInitialized()
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "Max Mediation is initialized");
        }

        public MediationType GetMediationType() => MediationType.MaxAds;
        

        public bool IsSdkInitialized() => _isMaxNativeSdkInitialized;

        // The consent flags must be initialized BEFORE the SDK initialization. So ensure that 'SetConsent' is called before initializing the Max SDK.
        // 1/ see: https://developers.facebook.com/docs/audience-network/optimization/best-practices/ccpa
        // 2/ see: https://dash.applovin.com/documentation/mediation/unity/getting-started/privacy
        // "The AppLovin SDK writes your app’s privacy states (“Age Restricted User”, “Has User Consent”, and “Do Not Sell”) to the log.
        // It does this when you initialize the SDK, so it is a good practice to set those privacy states before you do this initialization.
        // This helps to ensure that the privacy states are set correctly."
        public void SetConsent(bool consent, bool isCcpaApplicable)
        {
            MaxSdk.SetHasUserConsent(consent);
            MaxSdk.SetIsAgeRestrictedUser(false);

            FacebookMaxAdNetwork.SetConsent(consent);

            VoodooLog.LogDebug(Module.ADS, TAG, isCcpaApplicable ? "Apply CCPA" : "CCPA not applicable");

            // For the users under the CCPA law the data must not be sold.
            // See https://dash.applovin.com/documentation/mediation/unity/getting-started/privacy
            bool doNotSellData = isCcpaApplicable && !consent;
            MaxSdk.SetDoNotSell(doNotSellData);
            FacebookMaxAdNetwork.SetDoNotSell(doNotSellData);
        }


        private bool IsBannerEnabled() => !string.IsNullOrEmpty(_adUnits?.bannerAdUnit);
        public void ShowBanner()
        {
            if (!IsBannerEnabled()) return;
            MaxSdk.ShowBanner(_adUnits.bannerAdUnit);
        }

        public void HideBanner()
        {
            if (!IsBannerEnabled()) return;
            MaxSdk.HideBanner(_adUnits.bannerAdUnit);
        }

        public void DestroyBanner()
        {
            if (!IsBannerEnabled() || !_bannerCreated) return;
            MaxSdk.DestroyBanner(_adUnits.bannerAdUnit);
            _bannerCreated = false;
            _bannerAdInfo = null;
        }

        public void SetBannerBackgroundVisibility(bool visibility, Color color)
        {
            if (!IsBannerEnabled()) return;
            if (visibility) {
                MaxSdk.SetBannerBackgroundColor(_adUnits.bannerAdUnit, color);
                BannerBackground.Show(color);
            } else {
                BannerBackground.Hide();
            }
        }

        private bool IsInterstitialEnabled() => !string.IsNullOrEmpty(_adUnits?.interstitialAdUnit);
        public void ShowInterstitial(bool isInterstitialShownInsteadOfRewarded = false)
        {
            if (!IsInterstitialEnabled()) return;
            _isInterstitialShownInsteadOfRewarded = isInterstitialShownInsteadOfRewarded;
            MaxSdk.ShowInterstitial(_adUnits?.interstitialAdUnit, null, AnalyticsUserIdHelper.GetUserId());
        }

        private bool IsRewardedVideoEnabled() => !string.IsNullOrEmpty(_adUnits?.rewardedVideoAdUnit);
        public void ShowRewardedVideo()
        {
            if (!IsRewardedVideoEnabled()) return;
            MaxSdk.ShowRewardedAd(_adUnits.rewardedVideoAdUnit, null, AnalyticsUserIdHelper.GetUserId());
        }

        private bool IsAppOpenEnabled() => !_hasPaidToHideAds && _appOpenEnabled && !string.IsNullOrEmpty(_adUnits?.appOpenAdUnit);
        public void ShowAppOpen()
        {
            if (!IsAppOpenEnabled()) return;
            MaxSdk.ShowAppOpenAd(_adUnits.appOpenAdUnit, null, AnalyticsUserIdHelper.GetUserId());
        }

        private bool IsNativeAdEnabled() => !string.IsNullOrEmpty(_adUnits?.nativeAdsAdUnit);
        public void ShowNativeAd(string layoutName, float x, float y, float width, float height)
        {
            if (!IsNativeAdEnabled()) return;
            MaxNativeAdsSdk.ShowAd(layoutName, x, y, width, height);
        }

        public void HideNativeAd()
        {
            if (!IsNativeAdEnabled()) return;
            MaxNativeAdsSdk.HideAd();
        }

        public void SetAdUnit(AdUnitType type, string adUnit) { }

        public bool HasDebugger() => true;

        public void ShowDebugger()
        {
            MaxSdk.ShowMediationDebugger();
        }

        public void OnApplicationPause(bool pauseStatus) { }

        public VoodooAdInfo GetBannerInfo() => BuildAdInfo(_bannerAdInfo);
        public VoodooAdInfo GetInterstitialInfo() => BuildAdInfo(_interstitialAdInfo);
        public VoodooAdInfo GetRewardedVideoInfo() => BuildAdInfo(_rewardedAdInfo);
        public VoodooAdInfo GetAppOpenInfo() => BuildAdInfo(_appOpenAdInfo);
        public VoodooAdInfo GetMrecInfo() => BuildAdInfo(_mrecAdInfo);
        public VoodooAdInfo GetNativeAdsInfo() => BuildAdInfo(_nativeAdInfo);

        private static VoodooAdInfo BuildAdInfo(MaxAdInfo adInfo) => new VoodooAdInfo {
            Id = adInfo?.MaxNativeAdInfo.CreativeIdentifier,
            AdUnit = adInfo?.MaxNativeAdInfo.AdUnitIdentifier,
            AdNetworkName = adInfo?.MaxNativeAdInfo.NetworkName,
            AdNetworkPlacementId = adInfo?.MaxNativeAdInfo.Placement,
            Revenue = adInfo?.MaxNativeAdInfo.Revenue,
            WaterfallTestName = adInfo?.WaterfallTestName,
            WaterfallName = adInfo?.WaterfallName,
            Creative = adInfo?.MaxNativeAdInfo.CreativeIdentifier,
            Country = GetCountryCode()
        };

        private static string _countryCode;

        private static string GetCountryCode()
        {
            if (!_isMaxNativeSdkInitialized)
                return "";
            
            if (string.IsNullOrEmpty(_countryCode)) {
                _countryCode = MaxSdk.GetSdkConfiguration().CountryCode;
            }
            return _countryCode;
        }
        
#endregion

#region banner methods

        public void InitializeBanner()
        {
            if (!IsBannerEnabled()) {
                VoodooLog.LogError(Module.ADS, TAG, "No Banner AdUnit found for this platform.");
                AdsManager.Banner.OnInitialized(false);
                return;
            }

            if (!_bannerInitialized) {
                _bannerInitialized = true; 
                // Attach callback
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoaded;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailed;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClicked;
            }
            AdsManager.Banner.OnInitialized(true);
            // Load Banner, must not be called twice.
            if (!_bannerCreated) {
                _bannerCreated = true;
                AmazonWrapper.InitializeBanner(_adUnits, () =>
                {
                    MaxSdk.SetBannerCustomData(_adUnits.bannerAdUnit, AnalyticsUserIdHelper.GetUserId());
                    MaxSdk.CreateBanner(_adUnits.bannerAdUnit, MaxSdkBase.BannerPosition.BottomCenter);
                });
            }
        }

        public float GetNativeBannerHeight() => MaxSdkUtils.GetAdaptiveBannerHeight();
        public float GetScreenDensity() => MaxSdkUtils.GetScreenDensity();

        private void OnBannerAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _bannerAdInfo = new MaxAdInfo(adInfo);
            AdsManager.Banner.OnLoadSuccess();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnBannerAdLoadedEvent - {adUnitId} - {adInfo}");
        }

        private void OnBannerAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _bannerAdInfo = null;
            AdsManager.Banner.OnLoadFailed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnBannerAdLoadFailedEvent - {adUnitId} - {errorInfo}");
        }

        private void OnBannerAdClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _bannerAdInfo?.Update(adInfo);
            AdsManager.Banner.OnClicked();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnBannerAdClicked - {adUnitId} - {adInfo}");
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _bannerAdInfo?.Update(adInfo);
            AdsManager.Banner.OnImpression(ImpressionInfoType.MaxAds);
        }
#endregion

#region interstitial methods

        public void InitializeInterstitial()
        {
            if (!IsInterstitialEnabled()) {
                VoodooLog.LogError(Module.ADS, TAG, "No Interstitial AdUnit found for this platform.");
                AdsManager.Interstitial.OnInitialized(false);
                return;
            }
            if (!_interstitialInitialized) {
                _interstitialInitialized = true;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailed;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToDisplay;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissed;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClicked;
            }
            AdsManager.Interstitial.OnInitialized(true);
            if(AdsManager.EnableReplaceRewardedOnCpm || AdsManager.EnableReplaceRewardedIfNotLoaded)
                AdsManager.RewardedInterstitialVideo.OnInitialized(true);
            // Load the first interstitial
            AmazonWrapper.InitializeInterstitial(_adUnits, _interstitialLoadingBackoff.Start);
        }

        private void LoadInterstitial()
        {
            if (!IsInterstitialEnabled()) return;

            _interstitialAdInfo = null;
            AdsManager.Interstitial.OnLoading();
            if(AdsManager.EnableReplaceRewardedOnCpm || AdsManager.EnableReplaceRewardedIfNotLoaded)
                AdsManager.RewardedInterstitialVideo.OnLoading();
            MaxSdk.LoadInterstitial(_adUnits.interstitialAdUnit);
        }
        
        private void OnInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interstitialAdInfo = new MaxAdInfo(adInfo);
            AdsManager.Interstitial.OnLoadSuccess();
            if(AdsManager.EnableReplaceRewardedOnCpm || AdsManager.EnableReplaceRewardedIfNotLoaded)
                AdsManager.RewardedInterstitialVideo.OnLoadSuccess();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnInterstitialLoadedEvent - {adUnitId} - {adInfo}");
        }

        private  void OnInterstitialClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interstitialAdInfo?.Update(adInfo);
            if(_isInterstitialShownInsteadOfRewarded)
                AdsManager.RewardedInterstitialVideo.OnClicked();
            else
                AdsManager.Interstitial.OnClicked();

            VoodooLog.LogDebug(Module.ADS, TAG, $"OnInterstitialClicked - {adUnitId} - {adInfo}");
        }

        private void OnInterstitialFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _interstitialAdInfo = null;
            AdsManager.Interstitial.OnLoadFailed();
            if(AdsManager.EnableReplaceRewardedOnCpm || AdsManager.EnableReplaceRewardedIfNotLoaded)
                AdsManager.RewardedInterstitialVideo.OnLoadFailed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnInterstitialFailedEvent - {adUnitId} - {errorInfo}");
            _interstitialLoadingBackoff.Start();
        }

        private void OnInterstitialFailedToDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            _interstitialAdInfo?.Update(adInfo);
            if (_isInterstitialShownInsteadOfRewarded) {
                AdsManager.RewardedInterstitialVideo.OnFailedToShow(MapToVoodooErrorAdInfo(errorInfo));
            } else {
                AdsManager.Interstitial.OnFailedToShow(MapToVoodooErrorAdInfo(errorInfo));
            }
            
            VoodooLog.LogDebug(Module.ADS, TAG, $"InterstitialFailedToDisplayEvent - {adUnitId} - {errorInfo} - {adInfo}");
            _interstitialLoadingBackoff.Restart();
        }

        private void OnInterstitialDismissed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interstitialAdInfo?.Update(adInfo);
            if(_isInterstitialShownInsteadOfRewarded)
                AdsManager.RewardedInterstitialVideo.OnDismissed();
            else
                AdsManager.Interstitial.OnDismissed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnInterstitialDismissedPrivateEvent - {adUnitId} - {adInfo}");
            _interstitialLoadingBackoff.Restart();
        }

        private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interstitialAdInfo?.Update(adInfo);
            if(_isInterstitialShownInsteadOfRewarded)
                AdsManager.RewardedInterstitialVideo.OnImpression(ImpressionInfoType.MaxAds);
            else 
                AdsManager.Interstitial.OnImpression(ImpressionInfoType.MaxAds);
        }

#endregion

#region rewarded methods

        public void InitializeRewardedVideo()
        {
            if (!IsRewardedVideoEnabled()) {
                VoodooLog.LogError(Module.ADS, TAG, "No RewardedVideo AdUnit found for this platform.");
                AdsManager.RewardedVideo.OnInitialized(false);
                return;
            }

            if (!_rewardedVideoInitialized) {
                _rewardedVideoInitialized = true;
                // Attach callback
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHidden;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailed;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplay;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedReward;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClicked;
            }
            AdsManager.RewardedVideo.OnInitialized(true);
            // Load the first RewardedAd
            AmazonWrapper.InitializeRewardedVideo(_adUnits, _rewardedLoadingBackoff.Start);
        }

        private void LoadRewardedAd()
        {
            _rewardedAdInfo = null;
            _rewardedReceived = false;
            AdsManager.RewardedVideo.OnLoading();
            MaxSdk.LoadRewardedAd(_adUnits.rewardedVideoAdUnit);
        }

        private void OnRewardedAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedAdInfo = new MaxAdInfo(adInfo);
            AdsManager.RewardedVideo.OnLoadSuccess();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnRewardedAdLoadedEvent - {adUnitId} - {adInfo}");
        }
        private void OnRewardedAdFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _rewardedAdInfo = null;
            AdsManager.RewardedVideo.OnLoadFailed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnRewardedAdFailedEvent - {adUnitId} - {errorInfo}");
            _rewardedLoadingBackoff.Start();
        }

        private void OnRewardedAdReceivedReward(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedReceived = true;
            _rewardedAdInfo?.Update(adInfo);
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnRewardedAdReceivedRewardEvent - {adUnitId} - {reward} - {adInfo}");
        }

        private  void OnRewardedAdClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedAdInfo?.Update(adInfo);
            AdsManager.RewardedVideo.OnClicked();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnRewardedAdClickedEvent - {adUnitId} - {adInfo}");
        
        }

        private void OnRewardedAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedAdInfo?.Update(adInfo);
            AdsManager.RewardedVideo.OnDismissed(_rewardedReceived);
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnRewardedAdDismissedEvent - {adUnitId} - {_rewardedReceived}  - {adInfo}");
            _rewardedLoadingBackoff.Restart();
        }
        
        private void OnRewardedAdFailedToDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedAdInfo?.Update(adInfo);
            AdsManager.RewardedVideo.OnFailedToShow(MapToVoodooErrorAdInfo(errorInfo));
            VoodooLog.LogError(Module.ADS, TAG, $"RewardedAdFailedToDisplayEvent - {adUnitId} - {errorInfo} - {adInfo}");
            _rewardedLoadingBackoff.Restart();
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedAdInfo?.Update(adInfo);
            AdsManager.RewardedVideo.OnImpression(ImpressionInfoType.MaxAds);
        }

#endregion
        
#region appOpen methods

        public void InitializeAppOpen()
        {
            if (_hasPaidToHideAds || !_appOpenEnabled) {
                VoodooLog.LogDebug(Module.ADS, TAG, 
                    "The AppOpen feature is disabled");
                AdsManager.AppOpen.OnInitialized(false);
                return;
            }

            if (string.IsNullOrEmpty(_adUnits?.appOpenAdUnit)) {
                VoodooLog.LogWarning(Module.ADS, TAG, 
                    "Can not initialize AppOpen because no AppOpen AdUnit found for this platform");
                AdsManager.AppOpen.OnInitialized(false);
                return;
            }
            
            if (!_appOpenAdInitialized) {
                _appOpenAdInitialized = true;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAppOpenAdRevenuePaidEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoaded;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenFailed;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenFailedToDisplay;
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissed;
                MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAppOpenClicked;
            }
            AdsManager.AppOpen.OnInitialized(true);
            
            // Load the first appOpen ad
            _appOpenAdLoadingBackoff.Start();
        }

        private void LoadAppOpenAd()
        {
            if (!IsAppOpenEnabled()) return;

            _appOpenAdInfo = null;
            AdsManager.AppOpen.OnLoading();
            MaxSdk.LoadAppOpenAd(_adUnits.appOpenAdUnit);
        }
        
        private void OnAppOpenLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _appOpenAdInfo = new MaxAdInfo(adInfo);
            AdsManager.AppOpen.OnLoadSuccess();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnAppOpenLoaded - {adUnitId} - {adInfo}");
        }

        private  void OnAppOpenClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _appOpenAdInfo?.Update(adInfo);
            AdsManager.AppOpen.OnClicked();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnAppOpenClicked - {adUnitId} - {adInfo}");
        }

        private void OnAppOpenFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _appOpenAdInfo = null;
            AdsManager.AppOpen.OnLoadFailed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnAppOpenFailed - {adUnitId} - {errorInfo}");
            _appOpenAdLoadingBackoff.Start();
        }

        private void OnAppOpenFailedToDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            _appOpenAdInfo?.Update(adInfo);
            AdsManager.AppOpen.OnFailedToShow(MapToVoodooErrorAdInfo(errorInfo));
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnAppOpenFailedToDisplay - {adUnitId} - {errorInfo} - {adInfo}");
            _appOpenAdLoadingBackoff.Restart();
        }

        private void OnAppOpenDismissed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _appOpenAdInfo?.Update(adInfo);
            AdsManager.AppOpen.OnDismissed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnAppOpenDismissed - {adUnitId} - {adInfo}");
            _appOpenAdLoadingBackoff.Restart();
        }

        private void OnAppOpenAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _appOpenAdInfo?.Update(adInfo);
            AdsManager.AppOpen.OnImpression(ImpressionInfoType.MaxAds);
        }

#endregion

#region Mrec methods

        public void InitializeMrec()
        {
            if (!IsMrecEnabled()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "No Mrec AdUnit found for this platform.");
                AdsManager.Mrec.OnInitialized(false);  
                return;
            }

            if (!_mrecInitialized) {
                _mrecInitialized = true;
                _screenDensity = MaxSdkUtils.GetScreenDensity();
                
                MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoaded;
                MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClicked;
                MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
                MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailed;
            }
            AdsManager.Mrec.OnInitialized(true); 
            // Load Mrec, must not be called twice.
            if (!_mrecCreated) {
                _mrecCreated = true;
                MaxSdk.SetMRecCustomData(_adUnits.mrecAdUnit, AnalyticsUserIdHelper.GetUserId());
                MaxSdk.CreateMRec(_adUnits.mrecAdUnit, MaxSdkBase.AdViewPosition.Centered);
                MaxSdk.StopMRecAutoRefresh(_adUnits.mrecAdUnit);
            }
            // Load the first Mrec
            _mrecLoadingBackoff.Start();
        }
        
        private bool IsMrecEnabled() => !string.IsNullOrEmpty(_adUnits?.mrecAdUnit);
        public void ShowMrec(float x, float y)
        {
            if (!IsMrecEnabled()) return;
            
            //Calculate the coordinate of the MREC relative to the top left corner of the screen
            float density = _screenDensity;
            float xDp = x / density - MrecAdSizeDp.x * .5f;
            float yDp = y / density - MrecAdSizeDp.y * .5f;
            VoodooLog.LogDebug(Module.ADS, TAG, $"Update MRec position from {x}/{y} to  {xDp}/{xDp} ");
            MaxSdk.UpdateMRecPosition(_adUnits.mrecAdUnit, xDp, yDp);
            MaxSdk.ShowMRec(_adUnits.mrecAdUnit);
        }

        public void HideMrec()
        {
            if (!IsMrecEnabled()) return;
            MaxSdk.HideMRec(_adUnits.mrecAdUnit);
            VoodooLog.LogDebug(Module.ADS, TAG, "HideMrec ...");
            _mrecAdInfo = null;
            _mrecLoadingBackoff.Restart();
        }
        
        public void DestroyMrec()
        {
            if (!IsMrecEnabled() || !_mrecCreated) return;
            MaxSdk.DestroyMRec(_adUnits.mrecAdUnit);
            _mrecCreated = false;
            _mrecAdInfo = null;
        }
        
        private void LoadMrec()
        {
            _mrecAdInfo = null;
            AdsManager.Mrec.OnLoading();
            MaxSdk.SetMRecCustomData(_adUnits.mrecAdUnit, AnalyticsUserIdHelper.GetUserId());
            MaxSdk.LoadMRec(_adUnits.mrecAdUnit);
        }

        private void OnMRecAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _mrecAdInfo = new MaxAdInfo(adInfo);
            AdsManager.Mrec.OnLoadSuccess();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnMrecAdLoaded - {adUnitId} - {adInfo}");
        }

        private void OnMRecAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _mrecAdInfo = null;
            AdsManager.Mrec.OnLoadFailed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnMrecAdLoadFailed - {adUnitId} - {errorInfo}");
            _mrecLoadingBackoff.Start();
        }

        private void OnMRecAdClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _mrecAdInfo?.Update(adInfo);
            AdsManager.Mrec.OnClicked();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnMrecAdClicked - {adUnitId} - {adInfo}");
        }

        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _mrecAdInfo?.Update(adInfo);
            AdsManager.Mrec.OnImpression(ImpressionInfoType.MaxAds);
        }

#endregion

#region NativeAds methods

        public void InitializeNativeAds()
        {
            if (!IsNativeAdEnabled()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "No NativeAds AdUnit found for this platform.");
                AdsManager.NativeAds.OnInitialized(false);
                return;
            }

            if (!_nativeAdInitialized) {
                _nativeAdInitialized = true;
                VoodooLog.LogDebug(Module.ADS, TAG, $"Initialize NativeAds : {_adUnits.nativeAdsAdUnit}");
                MaxNativeAdsSdk.Initialize(_adUnits.nativeAdsAdUnit);
                MaxNativeAdsSdkCallbacks.OnAdRevenuePaidEvent += OnNativeAdRevenuePaidEvent;
                MaxNativeAdsSdkCallbacks.OnAdLoadedEvent += OnNativeAdLoaded;
                MaxNativeAdsSdkCallbacks.OnAdDisplayedEvent += OnNativeAdDisplayed;
                MaxNativeAdsSdkCallbacks.OnAdFailedToDisplayEvent += OnNativeAdFailedToDisplay;
                MaxNativeAdsSdkCallbacks.OnAdLoadFailedEvent += OnNativeAdFailed;
                MaxNativeAdsSdkCallbacks.OnAdHiddenEvent += OnNativeAdDismissed;
                MaxNativeAdsSdkCallbacks.OnAdClickedEvent += OnNativeAdClicked;
            }
            AdsManager.NativeAds.OnInitialized(true);
            // Load the first native ads
            _nativeAdsLoadingBackoff.Start();
        }

        private void LoadNativeAd()
        {
            _nativeAdInfo = null;
            AdsManager.NativeAds.OnLoading();
            MaxNativeAdsSdk.SetCustomData(AnalyticsUserIdHelper.GetUserId());
            MaxNativeAdsSdk.LoadAd();
        }

        private void OnNativeAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _nativeAdInfo = new MaxAdInfo(adInfo);
            AdsManager.NativeAds.OnLoadSuccess();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnNativeAdsLoaded - {adUnitId} - {adInfo}");
        }

        private  void OnNativeAdClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _nativeAdInfo?.Update(adInfo);
            AdsManager.NativeAds.OnClicked();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnnNativeAdsClicked - {adUnitId} - {adInfo}");
        }

        private void OnNativeAdFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _nativeAdInfo = null;
            AdsManager.NativeAds.OnLoadFailed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnNativeAdsFailed - {adUnitId} - {errorInfo}");
            _nativeAdsLoadingBackoff.Start();
        }

        private void OnNativeAdDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _nativeAdInfo?.Update(adInfo);
            AdsManager.NativeAds.OnShown();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnNativeAdDisplayed - {adUnitId} - {adInfo}");
        }

        private void OnNativeAdFailedToDisplay(string adUnit, MaxSdkBase.ErrorInfo errorInfo)
        {
            AdsManager.NativeAds.OnFailedToShow(MapToVoodooErrorAdInfo(errorInfo));
            VoodooLog.LogError(Module.ADS, TAG, $"OnNativeAdFailedToDisplay - {errorInfo}");
        }

        private void OnNativeAdDismissed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _nativeAdInfo?.Update(adInfo);
            AdsManager.NativeAds.OnDismissed();
            VoodooLog.LogDebug(Module.ADS, TAG, $"OnNativeAdsDismissed - {adUnitId} - {adInfo}");
            _nativeAdInfo = null;
            _nativeAdsLoadingBackoff.Restart();
        }

        private void OnNativeAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _nativeAdInfo?.Update(adInfo);
            AdsManager.NativeAds.OnImpression(ImpressionInfoType.MaxAds);
        }

#endregion

#region utility

        private VoodooErrorAdInfo MapToVoodooErrorAdInfo(MaxSdk.ErrorInfo errorInfo)
        {
            var errorAdInfo = new VoodooErrorAdInfo {
                ErrorCode = (int)errorInfo.Code,
                ErrorMessage = errorInfo.Message,
                AdNetworkErrorCode = errorInfo.MediatedNetworkErrorCode,
                AdNetworkErrorMessage = errorInfo.MediatedNetworkErrorMessage
            };
            return errorAdInfo;
        }

#endregion

#region Amazon Ads

        public bool EnableAmazonTesting()
        {
            Amazon.EnableTesting(true);
            Amazon.EnableLogging(true);
            return true;
        }

#endregion
    }
}