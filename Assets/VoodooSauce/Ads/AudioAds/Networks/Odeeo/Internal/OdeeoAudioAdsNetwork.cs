using System;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class OdeeoAudioAdsNetwork: IAudioAdsNetwork
    {
        private const string TAG = "OdeeoAudioAdsNetwork";
        private const int LOGO_SIZE = 70;
        private const string AD_BLOCKED = "AdBlocked";
        public string Name => "odeeo";

        private AdUnit _logoAdUnit;
        private event Action _onAudioAdShow;
        private event Action _onAudioAdClick;
        private event Action<double> _onAudioAdRevenue;
        private event Action<string> _onAudioAdFailed;
        private event Action _onAudioAdWatched;
        private event Action _onAudioAdUserClose;
        private bool _showing;
        private OdeeoConfig _odeeoConfig;
        
        public void Initialize(bool consent, bool isCcpaApplicable)
        {
            PlayOnSDK.SetLogLevel(Debug.isDebugBuild ? PlayOnSDK.LogLevel.Debug: PlayOnSDK.LogLevel.None);
            _odeeoConfig = VoodooSauce.GetItem<OdeeoConfig>();
            var voodooSettings = VoodooSettings.Load();
            if (string.IsNullOrEmpty(_odeeoConfig?.appKey))
            {
                _odeeoConfig = PlatformUtils.UNITY_IOS ? voodooSettings.iOSOdeeoConfig: voodooSettings.AndroidOdeeoConfig;
            }
            
            if (_odeeoConfig == null)
            {
                VoodooLog.LogError(Module.ADS, TAG, "No app key found for Odeeo audio ads network. Then, it's not initialized.");
                return;
            }

            if (string.IsNullOrEmpty(_odeeoConfig.appKey))
            {
                VoodooLog.LogError(Module.ADS, TAG, "No config found for Odeeo audio ads network (Kitchen or VoodooTune). Then, it's not initialized.");
                return;
            }
            
            PlayOnSDK.SetGdprConsent(consent);
            PlayOnSDK.SetDoNotSell(isCcpaApplicable && !consent);
            PlayOnSDK.OnInitializationFinished += OnInitialized;
            PlayOnSDK.Initialize(_odeeoConfig.appKey, GetRelevantStoreId(voodooSettings));
        }

        private static string GetRelevantStoreId(VoodooSettings settings)
        {
            if (PlatformUtils.UNITY_ANDROID)
            {
                return string.Empty;
            }

            if (Application.identifier == VoodooConstants.TEST_APP_BUNDLE)
            {
                // For debug purpose (The VS test app doesn't have a store ID).
                return "11111111";
            }

            return settings.AppleStoreId;
        }

        private void OnInitialized()
        {
            _logoAdUnit = new AdUnit(PlayOnSDK.AdUnitType.AudioLogoAd);
            _logoAdUnit.AdCallbacks.OnClose += () => {
                _showing = false; 
                _onAudioAdWatched?.Invoke();
            };
            _logoAdUnit.AdCallbacks.OnUserClose += () => _onAudioAdUserClose?.Invoke();
            _logoAdUnit.AdCallbacks.OnClick += () => _onAudioAdClick?.Invoke();
            _logoAdUnit.AdCallbacks.OnAdBlocked += () => _onAudioAdFailed?.Invoke(AD_BLOCKED);
            // the OnImpression callback is more reliable and should be used for both the impression and the ILRD events
            _logoAdUnit.AdCallbacks.OnImpression += data => {
                _showing = true;
                _onAudioAdShow?.Invoke();
                _onAudioAdRevenue?.Invoke(data.GetRevenue() / 1000f);
            };
            _logoAdUnit.SetActionButton(
                _odeeoConfig.buttonType?.Trim().ToLower() == "mute"
                    ? PlayOnSDK.AdUnitActionButtonType.Mute
                    : PlayOnSDK.AdUnitActionButtonType.Close, 7);
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            PlayOnSDK.onApplicationPause(pauseStatus);
        }

        public bool IsAudioAdAvailable()
        {
            return _logoAdUnit != null && _logoAdUnit.IsAdAvailable();
        }

        public void ShowAudioAd(IAudioAdPositionBehaviour prefab)
        {
            ShowAudioAd(() =>
            {
                _logoAdUnit.LinkLogoToRectTransform(PlayOnSDK.Position.Centered, prefab.RectTransform, prefab.Canvas);
            });
        }

        public void ShowAudioAd(AudioAdPosition position, Vector2Int offset)
        {
            ShowAudioAd(() =>
            {
                // Converting AudioAdPosition to PlayOnSDK.Position
                PlayOnSDK.Position playOnSdkPosition;
                switch (position)
                {
                    case AudioAdPosition.Centered:
                        playOnSdkPosition = PlayOnSDK.Position.Centered;
                        break;
                    case AudioAdPosition.BottomCenter:
                        playOnSdkPosition = PlayOnSDK.Position.BottomCenter;
                        break;
                    case AudioAdPosition.BottomLeft:
                        playOnSdkPosition = PlayOnSDK.Position.BottomLeft;
                        break;
                    case AudioAdPosition.BottomRight:
                        playOnSdkPosition = PlayOnSDK.Position.BottomRight;
                        break;
                    case AudioAdPosition.CenterLeft:
                        playOnSdkPosition = PlayOnSDK.Position.CenterLeft;
                        break;
                    case AudioAdPosition.CenterRight:
                        playOnSdkPosition = PlayOnSDK.Position.CenterRight;
                        break;
                    case AudioAdPosition.TopCenter:
                        playOnSdkPosition = PlayOnSDK.Position.TopCenter;
                        break;
                    case AudioAdPosition.TopLeft:
                        playOnSdkPosition = PlayOnSDK.Position.TopLeft;
                        break;
                    default:
                        playOnSdkPosition = PlayOnSDK.Position.TopRight;
                        break;
                }

                _logoAdUnit.SetLogo(playOnSdkPosition, offset.x, offset.y, LOGO_SIZE);
            });
        }

        private void ShowAudioAd(Action actionToCallForPositioning)
        {
            
            if (_logoAdUnit == null)
            {
                VoodooLog.LogWarning(Module.ADS, TAG, "Can't display an audio ad because Odeeo isn't initialized.");
                return;
            }

            if (!_logoAdUnit.IsAdAvailable())
            {
                VoodooLog.LogWarning(Module.ADS, TAG, "Can't display an audio ad because it's not loaded yet.");
                return;
            }
            
            actionToCallForPositioning.Invoke();
            
            _logoAdUnit.ShowAd();
        }

        public void CloseAudioAd()
        {
            _logoAdUnit?.CloseAd();
        }

        public bool IsShowingAd() => _showing;

        public void SetOnAudioAdClick(Action callback)
        {
            _onAudioAdClick += callback;
        }

        public void SetOnAudioAdShow(Action callback)
        {
            _onAudioAdShow += callback;
        }
        
        public void OnFullscreenAdShow() 
        {
            // Odeeo SDK needs to call this method when a fullscreen ads will be show in order to avoid overlapping between FS/RV and audio ads.
            PlayOnSDK.onApplicationPause(true);
        }
        
        public void SetOnAudioAdImpression(Action<double> callback)
        {
            _onAudioAdRevenue += callback;
        }

        public void SetOnAudioAdFailed(Action<string> callback)
        {
            _onAudioAdFailed += callback;
        }

        public void SetOnAudioAdWatched(Action callback)
        {
            _onAudioAdWatched += callback;
        }

        public void SetOnAudioAdUserClose(Action callback)
        {
            _onAudioAdUserClose += callback;
        }
    }
}