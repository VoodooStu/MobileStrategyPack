using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using QueryParameters = Voodoo.Sauce.Internal.Common.Utils.QueryParameters;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Models;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Parameters;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.CrossPromo.Mercury
{
    public class MercuryAPI : MonoBehaviour
    {
        private const string TAG = "MercuryAPI";
        
        private static PrivacyCore _privacy => VoodooSauceCore.GetPrivacy();
        
        private static MercuryAPI _instance;
        public static MercuryAPI Instance {
            get {
                if (!_instance) {
                    var obj = new GameObject("MercuryAPI");
                    _instance = obj.AddComponent<MercuryAPI>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        private const string MERCURY_URL = "https://crosspromo.voodoo.io";
        private const string MERCURY_TOKEN = "Token 16aaae9ea829470dc2993d0afe865d1165230589";

        public static void GetGameInfo(GetGameInfoParameters parameters, Action<MercuryRequestInfo> onSuccess, Action<MercuryRequestInfo> onFailure)
        {
            Instance.StartCoroutine(GetGameInfoCoroutine(parameters, onSuccess, onFailure));
        }

        private static IEnumerator GetGameInfoCoroutine(GetGameInfoParameters parameters, Action<MercuryRequestInfo> onSuccess, Action<MercuryRequestInfo> onFailure)
        {
            VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"GetGameInfo with: {parameters.ToJsonString()}");
            UnityWebRequest webRequest = CreateGameInfoRequest(parameters);
            
            VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"URL Used: {webRequest.url}");
            
            webRequest.timeout = 5;
            float startTime = Time.time;
            yield return webRequest.SendWebRequest();
            float elapsed = Time.time - startTime;

            bool hasTimeout = elapsed >= webRequest.timeout;

            var requestInfo = new MercuryRequestInfo {
                downloadTime = elapsed,
                hasTimeout = hasTimeout,
                httpResponse = webRequest.responseCode.ToString(),
                gamesPromoted = ""
            };

            if (hasTimeout) {
                VoodooLog.LogWarning(Module.CROSS_PROMO, TAG, $"Timeout while getting info: {webRequest.error}");
                onFailure?.Invoke(requestInfo);
            } else if (webRequest.isNetworkError || webRequest.isHttpError) {
                VoodooLog.LogWarning(Module.CROSS_PROMO, TAG, $"Error while getting info: {webRequest.error}");
                onFailure?.Invoke(requestInfo);
            } else {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"Success while getting info: {webRequest.downloadHandler.text}");
                if (!string.IsNullOrEmpty(webRequest.downloadHandler.text)) {
                    var waterfall = JsonUtility.FromJson<MercuryWaterfall>(webRequest.downloadHandler.text);
                    requestInfo.waterfall = waterfall;
                    requestInfo.data = webRequest.downloadHandler.text;
                }
                onSuccess?.Invoke(requestInfo);
            }
        }

        public static UnityWebRequest CreateGameInfoRequest(GetGameInfoParameters info)
        {
            var baseUrl = $"{MERCURY_URL}/api/sdk/game/";
            
            var queryParams = new QueryParameters(baseUrl);
            queryParams.Add("vs_version", VoodooSauce.Version());
            queryParams.Add("osType", info.osType);
            queryParams.Add("bundleId", info.bundleId);
            queryParams.Add("cpFormat", info.cpFormat);
            queryParams.Add("screen_resolution", info.screenResolution);

            if (!info.restrictedPrivacy) {
                queryParams.Add("advertising_id", info.adId);
                queryParams.Add("idfv", info.idfv);
                queryParams.Add("user_id", info.userId);
                queryParams.Add("session_id", info.sessionId);
                queryParams.Add("session_count", info.sessionCount);
                queryParams.Add("app_open_count", info.appOpenCount);
                queryParams.Add("user_game_count", info.userGameCount);
                queryParams.Add("manufacturer", info.manufacturer);
                queryParams.Add("model", info.deviceModel);
                queryParams.Add("app_version", info.appVersion);
                queryParams.Add("game_win_ratio", info.gameWinRatio);
                
                queryParams.Add("waterfallGameList", info.waterfallGameList);
                
                if (!string.IsNullOrEmpty(info.waterfallId)) {
                    queryParams.Add("waterfallId", info.waterfallId);
                }
            }

            if (MercuryTestModeManager.Instance.IsTestModeEnabled()) {
                queryParams.Add("is_test_mode_active", 1);
            }
            
            string url = queryParams.GetFormattedUrl();
            
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.timeout = 10;
            webRequest.SetRequestHeader("Authorization", MERCURY_TOKEN);
            return webRequest;
        }
        
        public static GetGameInfoParameters CreateDefaultGameInfoParameters(string format, string gameList, string waterfallId, bool restrictedPrivacy)
        {
            string advertisingId = _privacy.GetAdvertisingId();
            
            AnalyticsSessionManager sessionManager = AnalyticsSessionManager.Instance();
        
            var osType = "ios";
            
            if (PlatformUtils.UNITY_ANDROID) {
                osType = "android";
            }
            
            return new GetGameInfoParameters {
                restrictedPrivacy = restrictedPrivacy,
                cpFormat = format,
                waterfallId = waterfallId,
                waterfallGameList = gameList,
                bundleId = Application.identifier,
                osType = osType,
                adId = advertisingId,
                idfv = SystemInfo.deviceUniqueIdentifier,
                userId = AnalyticsUserIdHelper.GetUserId(),
                sessionId = sessionManager.SessionInfo.id,
                sessionCount = sessionManager.SessionInfo.count,
                appOpenCount = AnalyticsStorageHelper.Instance.GetAppLaunchCount(),
                userGameCount = AnalyticsStorageHelper.Instance.GetGameCount(),
                manufacturer = DeviceUtils.Manufacturer,
                deviceModel = DeviceUtils.Model,
                screenResolution = $"{Screen.width}x{Screen.height}",
                appVersion = Application.version,
                gameWinRatio = AnalyticsStorageHelper.Instance.GetWinRate().ToString("0.##", CultureInfo.CreateSpecificCulture("en-US")),
            };
        }
    }
}