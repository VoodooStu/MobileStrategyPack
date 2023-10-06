using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class AudioAdsAnalyticsManager: IAudioAdsAnalyticsManager
    {
        public event Action<AudioAdTriggerAnalyticsInfo> OnAudioAdTriggerEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdShownEvent;
        public event Action<AudioAdImpressionAnalyticsInfo> OnAudioAdImpressionEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdClickEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdWatchedEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdCloseEvent;
        public event Action<AudioAdFailedAnalyticsInfo> OnAudioAdFailedEvent;
        
        private void SetAudioAdInfoCommonParameters(AudioAdAnalyticsInfo info)
        {
            info.gameCount = AnalyticsStorageHelper.Instance.GetGameCount();
            info.audioAdType = "game_start";
        }
        
        public void TrackAudioAdTrigger(AudioAdTriggerAnalyticsInfo info)
        {
            SetAudioAdInfoCommonParameters(info);
            OnAudioAdTriggerEvent?.Invoke(info);
        }

        public void TrackAudioAdShown(AudioAdAnalyticsInfo info)
        {
            SetAudioAdInfoCommonParameters(info);
            OnAudioAdShownEvent?.Invoke(info);
        }

        public void TrackAudioAdImpression(AudioAdImpressionAnalyticsInfo info)
        {
            SetAudioAdInfoCommonParameters(info);
            info.adUnitFormat = "Logo";
            OnAudioAdImpressionEvent?.Invoke(info);
        }

        public void TrackAudioAdClicked(AudioAdAnalyticsInfo info)
        {
            SetAudioAdInfoCommonParameters(info);
            OnAudioAdClickEvent?.Invoke(info);
        }
        
        public void TrackAudioAdWatched(AudioAdAnalyticsInfo info)
        {
            SetAudioAdInfoCommonParameters(info);
            OnAudioAdWatchedEvent?.Invoke(info);
        }

        public void TrackAudioAdClosed(AudioAdAnalyticsInfo info)
        {
            SetAudioAdInfoCommonParameters(info);
            OnAudioAdCloseEvent?.Invoke(info);
        }

        public void TrackAudioAdFailed(AudioAdFailedAnalyticsInfo info)
        {
            SetAudioAdInfoCommonParameters(info);
            OnAudioAdFailedEvent?.Invoke(info);
        }
    }
}