using System;
using System.Text;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.LoadingTime
{
    internal class LoadingTimerManager
    {
        
#region Common

        internal long GetSessionId() => GetGlobalLoadingTimeStartTimestamp();

        internal void Pause() {
            _globalTimer?.Pause();
            _voodooSauceSDKTimer?.Pause();
            _unityTimer?.Pause();
            _privacyTimer?.Pause();
            _attTimer?.Pause();
        }
        
        internal void Unpause() {
            _globalTimer?.Unpause();
            _voodooSauceSDKTimer?.Unpause();
            _unityTimer?.Unpause();
            _privacyTimer?.Unpause();
            _attTimer?.Unpause();
        }

#endregion
        
#region Global Loading Time

        // This is the global loading timer of the game,
        // i.e. the time from the application launch to the first interactable action for the user.
        private readonly LoadingTimer _globalTimer = new LoadingTimer(DateTimeOffset.FromUnixTimeMilliseconds(ElapsedTime.GetStartTimestamp()));

        internal long GetGlobalLoadingTimeStartTimestamp() => _globalTimer.GetStartTimestamp();

        internal long GetGlobalLoadingTimeEndTimestamp() => _globalTimer.GetEndTimestamp();

        internal long GetGlobalLoadingTimeDuration() => _globalTimer.GetDuration();

        internal long GetRealGlobalLoadingTimeDuration()
        {
            long duration = GetGlobalLoadingTimeDuration();
            //On going timer wont be considered
            if (duration < 0) {
                return -1;
            }
            
            // During the Global loading time the privacy popup can be shown (at the first launch of the game)
            // so this elapsed time must be subtracted because it isn't effective technical loading time.
            // See the timeline:
            // [APP STARTS].....[Global INIT]+++++[PRIVACY POPUP SHOWN]-----[PRIVACY POPUP CLOSED][ATT POPUP SHOWN]-----[ATT POPUP CLOSED]+++++[Global DONE].....
            // Only consider the duration if the privacy dialog is shown when we are measuring the Global duration
            long privacyPopupDuration = GetPrivacyDisplayingTimeDuration();
            if (privacyPopupDuration > 0 && 
                GetPrivacyDisplayingTimeStartTimestamp() >= GetGlobalLoadingTimeStartTimestamp() && 
                GetPrivacyDisplayingTimeEndTimestamp() <= GetGlobalLoadingTimeEndTimestamp()) {
                duration -= privacyPopupDuration;
            }

            // During the Global loading time the ATT popup can be shown (at the first launch of the game)
            // so this elapsed time must be subtracted because it isn't effective technical loading time.
            // See the timeline:
            // [APP STARTS].....[Global INIT]+++++[ATT POPUP SHOWN]-----[ATT POPUP CLOSED]+++++[Global DONE].....
            // Only consider the duration if the ATT dialog is shown when we are measuring the Global duration
            long attPopUpDuration = GetAttDisplayingTimeDuration();
            if (attPopUpDuration > 0 &&
                GetAttDisplayingTimeStartTimestamp() >= GetGlobalLoadingTimeStartTimestamp() &&
                GetAttDisplayingTimeEndTimestamp() <= GetGlobalLoadingTimeEndTimestamp()) {
                duration -= attPopUpDuration;
            }

            return duration;
        }
        
        internal void StopGlobalLoadingTimer() => _globalTimer.Stop();

        internal bool IsGlobalLoadingTimerStopped() => _globalTimer.IsStopped();

#endregion
        
#region VoodooSauce SDK Loading Time

        // This is the loading timer of the VoodooSauce SDK.
        private LoadingTimer _voodooSauceSDKTimer;

        internal void StartVoodooSauceSDKLoadingTimer() {
            if (_voodooSauceSDKTimer != null) {
                return;
            }

            _voodooSauceSDKTimer = new LoadingTimer();
        }
        
        internal  long GetVoodooSauceSDKLoadingTimeStartTimestamp() => _voodooSauceSDKTimer?.GetStartTimestamp() ?? 0;

        internal long GetVoodooSauceSDKLoadingTimeEndTimestamp() => _voodooSauceSDKTimer?.GetEndTimestamp() ?? 0;

        internal void StopVoodooSauceSDKLoadingTimer() => _voodooSauceSDKTimer?.Stop();

        internal long GetVoodooSauceSDKLoadingTimeDuration() => _voodooSauceSDKTimer?.GetDuration() ?? -1;

        internal long GetVoodooSauceSDKRealLoadingTimeDuration()
        {
            // The timer must be stopped first.
            long duration = GetVoodooSauceSDKLoadingTimeDuration();
            if (duration < 0) {
                return -1;
            }

            // During the VS SDK loading time the privacy popup can be shown (at the first launch of the game)
            // so this elapsed time must be subtracted because it isn't effective technical loading time.
            // See the timeline:
            // [APP STARTS].....[VS INIT]+++++[PRIVACY POPUP SHOWN]-----[PRIVACY POPUP CLOSED][ATT POPUP SHOWN]-----[ATT POPUP CLOSED]+++++[VS DONE].....
            // Only consider the duration if the privacy dialog is shown when we are measuring the vs init duration
            long privacyPopupDuration = GetPrivacyDisplayingTimeDuration();
            if (privacyPopupDuration > 0 && 
                GetPrivacyDisplayingTimeStartTimestamp() >= GetVoodooSauceSDKLoadingTimeStartTimestamp() && 
                GetPrivacyDisplayingTimeEndTimestamp() <= GetVoodooSauceSDKLoadingTimeEndTimestamp()) {
                duration -= privacyPopupDuration;
            }

            // During the VS SDK loading time the ATT popup can be shown (at the first launch of the game)
            // so this elapsed time must be subtracted because it isn't effective technical loading time.
            // See the timeline:
            // [APP STARTS].....[VS INIT]+++++[ATT POPUP SHOWN]-----[ATT POPUP CLOSED]+++++[VS DONE].....
            // Only consider the duration if the ATT dialog is shown when we are measuring the vs init duration
            long attPopUpDuration = GetAttDisplayingTimeDuration();
            if (attPopUpDuration > 0 &&
                GetAttDisplayingTimeStartTimestamp() >= GetVoodooSauceSDKLoadingTimeStartTimestamp() &&
                GetAttDisplayingTimeEndTimestamp() <= GetVoodooSauceSDKLoadingTimeEndTimestamp()) {
                duration -= attPopUpDuration;
            }

            // When I tested, when Game started first without GDPR api call return (user without internet) the vs_init
            // steps is already completed, hence in new GDPR flow, game never started without completing vs_init steps
            // But since I only tested in VS test app not in majority of games, I only comment out the code for now
            //
            // // In the case that the privacy popup is displayed after the game is loaded, i.e. there is no
            // // internet connection at the beginning, the play time is subtracted from the VS SDK loading time.
            // // The time span between the game became interactable and the privacy popup is shown must be subtracted. 
            // // See the timeline:
            // // [APP STARTS].....[VS INIT]+++++[GAME STARTS]-----[GAME ENDS]---[PRIVACY POPUP SHOWN]-----
            // // -----[PRIVACY POPUP CLOSED]+++++[VS DONE].....
            // long appStartTimestamp = GetGlobalLoadingTimeEndTimestamp();
            // long privacyPopupStartTimestamp = GetPrivacyDisplayingTimeStartTimestamp();
            // if (appStartTimestamp > 0 && privacyPopupStartTimestamp > 0 &&
            //     privacyPopupStartTimestamp >= appStartTimestamp) {
            //     duration -= (privacyPopupStartTimestamp - appStartTimestamp);
            // }
            
            return duration;
        }
    
#endregion

#region Unity Loading Time

        // This is the Unity loading timer,
        // i.e. the time from the application launch to the Unity engine is started.
        private readonly LoadingTimer _unityTimer = new LoadingTimer(DateTimeOffset.FromUnixTimeMilliseconds(ElapsedTime.GetStartTimestamp()));

        internal long GetUnityLoadingTimeStartTimestamp() => _unityTimer.GetStartTimestamp();

        internal long GetUnityLoadingTimeEndTimestamp() => _unityTimer.GetEndTimestamp();

        internal long GetUnityLoadingTimeDuration() => _unityTimer.GetDuration();

        internal void StopUnityLoadingTimer() {
            float elapsedTimeSinceStartup = Time.realtimeSinceStartup;
            if (elapsedTimeSinceStartup > 0.0) {
                _unityTimer.SetEnd(DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(elapsedTimeSinceStartup)));   
            }
        }

#endregion

#region Privacy Displaying Time

        private LoadingTimer _privacyTimer;

        internal void StartPrivacyDisplayingTimer() {
            if (_privacyTimer != null) {
                return;
            }

            _privacyTimer = new LoadingTimer();
        }

        internal void StopPrivacyDisplayingTimer() {
            _privacyTimer?.Stop();
        }

        internal long GetPrivacyDisplayingTimeStartTimestamp() => _privacyTimer?.GetStartTimestamp() ?? 0;

        internal long GetPrivacyDisplayingTimeEndTimestamp() => _privacyTimer?.GetEndTimestamp() ?? 0;

        internal long GetPrivacyDisplayingTimeDuration() => _privacyTimer?.GetDuration() ?? -1;

        #endregion
        
#region ATT Displaying Time

        private LoadingTimer _attTimer;

        internal void StartAttDisplayingTimer() {
            if (_attTimer != null) {
                return;
            }

            _attTimer = new LoadingTimer();
        }

        internal void StopAttDisplayingTimer() {
            _attTimer?.Stop();
        }

        internal long GetAttDisplayingTimeStartTimestamp() => _attTimer?.GetStartTimestamp() ?? 0;

        internal long GetAttDisplayingTimeEndTimestamp() => _attTimer?.GetEndTimestamp() ?? 0;

        internal long GetAttDisplayingTimeDuration() => _attTimer?.GetDuration() ?? -1;

#endregion
    }
}