using System;
using System.Timers;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    internal class AnalyticsSessionManager
    {
        private const string TAG = "Analytics - SessionHelper";

        private const string PlayerPrefSessionCountKey = "VOODOO_ANALYTICS_SESSION_COUNT";

        private DateTime _lastEventCreationDate = DateTime.Now;
        private Timer _sessionLengthTimer;
        private int _sessionIdRenewalIntervalInSeconds;

        internal SessionInfo SessionInfo { get; } = new SessionInfo();
        internal event Action<SessionInfo> OnNewSession;

        private static AnalyticsSessionManager _instance;

        internal static AnalyticsSessionManager Instance()
        {
            if (_instance == null) {
                _instance = new AnalyticsSessionManager();
            }

            return _instance;
        }

        internal void Init()
        {
            _sessionIdRenewalIntervalInSeconds = VoodooSauce.GetItemOrDefault<AnalyticsConfig>().GetSessionIdRenewalIntervalInSeconds();
            if (_sessionLengthTimer == null) {
                _sessionLengthTimer = new Timer(1000) {Enabled = true, AutoReset = true};
                _sessionLengthTimer.Elapsed += (sender, args) => { SessionInfo.length++; };
                _sessionLengthTimer.Start();
            }

            // no session count value yet => take the injected value
            if (!PlayerPrefs.HasKey(PlayerPrefSessionCountKey)) {
                PlayerPrefs.SetInt(PlayerPrefSessionCountKey, AnalyticsStorageHelper.Instance.GetAppLaunchCount());
            } else {
                IncrementSessionCount();
            }

            SessionInfo.count = PlayerPrefs.GetInt(PlayerPrefSessionCountKey, 1);

            ResetSession();
            RefreshCreationDate();
        }

        internal void OnNewEvent()
        {
            if (_lastEventCreationDate.AddSeconds(_sessionIdRenewalIntervalInSeconds) < DateTime.Now) {
                Update();
            }

            RefreshCreationDate();
        }

        private void Update()
        {
            ResetSession();
            IncrementSessionCount();
            RefreshCreationDate();
        }

        private void ResetSession()
        {
            // renew session id
            SessionInfo.id = Guid.NewGuid().ToString();

            // reset session length counter
            SessionInfo.length = 0;
            
            // trigger new session event
            OnNewSession?.Invoke(SessionInfo);

            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "New session id: " + SessionInfo.id);
        }

        private void IncrementSessionCount()
        {
            SessionInfo.count = PlayerPrefs.GetInt(PlayerPrefSessionCountKey, 1) + 1;
            PlayerPrefs.SetInt(PlayerPrefSessionCountKey, SessionInfo.count);
            VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Session count incremented to: " + SessionInfo.count);
        }

        private void RefreshCreationDate() => _lastEventCreationDate = DateTime.Now;

        internal void StopTimer()
        {
            _sessionLengthTimer?.Stop();
        }
        
    }
}