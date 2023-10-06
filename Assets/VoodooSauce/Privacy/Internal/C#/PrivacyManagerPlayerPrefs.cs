using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Privacy
{
    internal partial class PrivacyManager : PrivacyCore
    {
        private const string PLAYER_PREF_IS_SIXTEEN_OR_OLDER = "Voodoo.Sauce.Privacy.IsOverSixteen";
        private const string PLAYER_PREF_ADS_CONSENT = "Voodoo.Sauce.Privacy.AdsConsent";
        private const string PLAYER_PREF_ANALYTICS_CONSENT = "Voodoo.Sauce.Privacy.AnalyticsConsent";
        private const string PLAYER_PREF_PRIVACY_VERSION = "Voodoo.Sauce.Privacy.PrivacyVersion";
        private const string PLAYER_PREF_NEED_SEND_CONSENT = "Voodoo.Sauce.Privacy.NeedSendConsent";
        private const string PLAYER_PREF_LAST_CONSENT_DATE = "Voodoo.Sauce.Privacy.LastConsentDate";
        private const string PLAYER_PREF_LAST_PLAY_DATE = "Voodoo.Sauce.Privacy.LastPlayDate";
        private const string PLAYER_PREF_LAST_PLAY_TIME = "Voodoo.Sauce.Privacy.LastPlayTime";
        private const string PLAYER_PREF_AD_TRACKING_ENABLED = "Voodoo.Sauce.Privacy.AdTrackingEnabled";
        private const string PLAYER_PREF_PRIVACY_LIST = "Voodoo.Sauce.Privacy.List";
        private const string PLAYER_PREF_USER_REQUEST_TO_BE_FORGOTTEN = "Voodoo.Sauce.Privacy.UserRequestedToBeForgotten";
        
        private void CacheAdsConsent(bool consent) => PlayerPrefs.SetInt(PLAYER_PREF_ADS_CONSENT, consent ? 1 : 0);

        private bool GetAdsConsent() => PlayerPrefs.GetInt(PLAYER_PREF_ADS_CONSENT, -1) == 1;

        private void CacheAnalyticsConsent(bool consent) => PlayerPrefs.SetInt(PLAYER_PREF_ANALYTICS_CONSENT, consent ? 1 : 0);

        private bool GetAnalyticsConsent() => PlayerPrefs.GetInt(PLAYER_PREF_ANALYTICS_CONSENT, -1) == 1;

        private string GetPrivacyVersion() => PlayerPrefs.GetString(PLAYER_PREF_PRIVACY_VERSION, "0");

        private void CachePrivacyVersion(string privacyVersion) => PlayerPrefs.SetString(PLAYER_PREF_PRIVACY_VERSION, privacyVersion);

        private bool GetAlreadyAppliedGdpr() => PlayerPrefs.HasKey(PLAYER_PREF_PRIVACY_VERSION);

        private void CacheSixteenOrOlder(bool isSixteenOrOlder) => PlayerPrefs.SetInt(PLAYER_PREF_IS_SIXTEEN_OR_OLDER, isSixteenOrOlder ? 1 : 0);

        private bool GetSixteenOrOlder() => PlayerPrefs.GetInt(PLAYER_PREF_IS_SIXTEEN_OR_OLDER, -1) == 1;

        private void CacheNeedSendConsent(bool needSend) => PlayerPrefs.SetInt(PLAYER_PREF_NEED_SEND_CONSENT, needSend ? 1 : 0);

        private bool NeedSendConsent() => PlayerPrefs.GetInt(PLAYER_PREF_NEED_SEND_CONSENT, -1) == 1;

        private static void CacheConsentDate() => PlayerPrefs.SetInt(PLAYER_PREF_LAST_CONSENT_DATE, TimeUtils.NowAsTimeStamp());

        private static int GetConsentDate() => PlayerPrefs.GetInt(PLAYER_PREF_LAST_CONSENT_DATE, 0);

        private void CacheAdTrackingEnabled(bool adTrackingEnabled) => PlayerPrefs.SetInt(PLAYER_PREF_AD_TRACKING_ENABLED, adTrackingEnabled ? 1 : 0);

        private bool GetCachedAdTrackingEnabled() => PlayerPrefs.GetInt(PLAYER_PREF_AD_TRACKING_ENABLED, 0) == 1;

        private void CachePrivacyList() => PlayerPrefs.SetString(PLAYER_PREF_PRIVACY_LIST, string.Join(",", PrivacyUtils.GetPrivacyPolicyUrls().ToArray()));

        private string[] GetPrivacyList() => PlayerPrefs.GetString(PLAYER_PREF_PRIVACY_LIST, "").Split(',');

        private bool IsCachePresent() => PlayerPrefs.HasKey(PLAYER_PREF_IS_SIXTEEN_OR_OLDER);

        private void CacheUserRequestedToBeForgotten(bool value) => PlayerPrefs.SetInt(PLAYER_PREF_USER_REQUEST_TO_BE_FORGOTTEN, value ? 1 : 0);

        public override bool UserRequestedToBeForgotten() => PlayerPrefs.GetInt(PLAYER_PREF_USER_REQUEST_TO_BE_FORGOTTEN, -1) == 1;

        private void CacheCurrentPlayDate() => PlayerPrefs.SetInt(PLAYER_PREF_LAST_PLAY_DATE, TimeUtils.NowAsTimeStamp());

        private int GetLastPlayDate() => PlayerPrefs.GetInt(PLAYER_PREF_LAST_PLAY_DATE, 0);
        
        private void CacheCurrentPlaytime(int playtimeInSecond) => PlayerPrefs.SetInt(PLAYER_PREF_LAST_PLAY_TIME, playtimeInSecond);

        private int GetLastPlaytimeInSecond() => PlayerPrefs.GetInt(PLAYER_PREF_LAST_PLAY_TIME, 0);
    }
}