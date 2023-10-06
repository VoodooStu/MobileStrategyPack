using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public interface IAudioAdsAnalyticsManager
    {
        public event Action<AudioAdTriggerAnalyticsInfo> OnAudioAdTriggerEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdShownEvent;
        public event Action<AudioAdImpressionAnalyticsInfo> OnAudioAdImpressionEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdClickEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdWatchedEvent;
        public event Action<AudioAdAnalyticsInfo> OnAudioAdCloseEvent;
        public event Action<AudioAdFailedAnalyticsInfo> OnAudioAdFailedEvent;

        public void TrackAudioAdTrigger(AudioAdTriggerAnalyticsInfo info);
        public void TrackAudioAdShown(AudioAdAnalyticsInfo info);
        public void TrackAudioAdImpression(AudioAdImpressionAnalyticsInfo info);
        public void TrackAudioAdClicked(AudioAdAnalyticsInfo info);
        public void TrackAudioAdWatched(AudioAdAnalyticsInfo info);
        public void TrackAudioAdClosed(AudioAdAnalyticsInfo info);
        public void TrackAudioAdFailed(AudioAdFailedAnalyticsInfo info);
    }
}