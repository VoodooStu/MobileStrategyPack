using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public interface IAudioAdsNetwork
    {
        string Name { get; }
        void Initialize(bool consent, bool isCcpaApplicable);
        void OnApplicationPause(bool pauseStatus);
        bool IsAudioAdAvailable();
        void ShowAudioAd(AudioAdPosition position, Vector2Int offset);
        void ShowAudioAd(IAudioAdPositionBehaviour position);
        void CloseAudioAd();
        bool IsShowingAd();
        void SetOnAudioAdClick(Action callback);
        void SetOnAudioAdShow(Action callback);
        void OnFullscreenAdShow();

        /// <param name="callback">This action is called when the revenue event is received. The double parameter corresponds to the revenue of the ad.</param>
        void SetOnAudioAdImpression(Action<double> callback);


        /// <param name="callback">This action is called when there are an error in loading or showing audio ads. The string parameter is the corresponding error code</param>
        void SetOnAudioAdFailed(Action<string> callback);
        /// SetOnAudioAdWatched is for when the audio ads are closed once it's finished
        void SetOnAudioAdWatched(Action callback);
        /// SetOnAudioAdUserClose is for when the audio ads are closed by the user by clicking the close button
        void SetOnAudioAdUserClose(Action callback);
    }
}