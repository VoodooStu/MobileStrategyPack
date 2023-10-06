using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Constants = Voodoo.Sauce.Internal.Analytics.AdnAnalyticsConstants;

namespace Voodoo.Sauce.Internal.Analytics
{
    /// This class is a utility class that stores the latest (Constants.AD_SESSION_CAPACITY) ad session 
    internal class AdSessionQueue
    {
        private const string FIRST_AD_SESSIONS_KEY = "first_ad_sessions_key";
        private readonly List<AdSession> _firstSessionsList;
        private readonly Queue<AdSession> _latestSessionsQueue;
        private readonly object _syncRoot = new object();

        public AdSessionQueue()
        {
            try {
                _latestSessionsQueue = new Queue<AdSession>();
                string json = PlayerPrefs.GetString(FIRST_AD_SESSIONS_KEY);
                _firstSessionsList = string.IsNullOrEmpty(json) ? new List<AdSession>() : JsonConvert.DeserializeObject<List<AdSession>>(json);
            } catch (Exception e) {
                _firstSessionsList = new List<AdSession>();
            }
        }

        internal List<Dictionary<string, object>> FirstSessionsDictionaryList() => ToDictionaryList(_firstSessionsList);
        internal List<Dictionary<string, object>> LatestSessionsDictionaryList() => ToDictionaryList(_latestSessionsQueue);

        private List<Dictionary<string, object>> ToDictionaryList(IEnumerable<AdSession> sessions)
        {
            var list = new List<Dictionary<string, object>>();
            lock (_syncRoot) {
                foreach (AdSession adSession in sessions) {
                    list.Add(adSession.ToDictionary());
                }
            }

            return list;
        }

        internal void UpdateAdSessions(string Placement,
                                       AdClosedEventAnalyticsInfo adAnalyticsInfo,
                                       double? revenue,
                                       bool isClicked,
                                       bool isSkipped)
        {
            var adSession = new AdSession {
                Placement = Placement,
                AdNetwork = adAnalyticsInfo.AdNetworkName,
                Cpm = revenue,
                CreativeId = adAnalyticsInfo.Creative,
                Duration = adAnalyticsInfo.AdDuration / Constants.MILLIS_PER_SECOND,
                IsClicked = isClicked,
                IsSkipped = isSkipped
            };
            lock (_syncRoot) {
                UpdateLatestAdSessions(adSession);
                UpdateFirstAdSessions(adSession);
            }
        }

        private void UpdateLatestAdSessions(AdSession adSession)
        {
            try {
                if (_latestSessionsQueue.Count == Constants.AD_SESSION_CAPACITY) _latestSessionsQueue.Dequeue();
                _latestSessionsQueue.Enqueue(adSession);
            } catch (Exception e) {
                //Nothing
            }
        }

        private void UpdateFirstAdSessions(AdSession adSession)
        {
            try {
                if (_firstSessionsList.Count == Constants.AD_SESSION_CAPACITY) return;
                _firstSessionsList.Add(adSession);
                PlayerPrefs.SetString(FIRST_AD_SESSIONS_KEY, JsonConvert.SerializeObject(_firstSessionsList));
            } catch (Exception e) {
                //Nothing
            }
        }
    }
}