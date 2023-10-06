using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Voodoo.Sauce.Internal.Extension;

// ReSharper disable once CheckNamespace
namespace Voodoo.Analytics
{
    internal class VanEvent
    {
        private const string EVENT_TYPE_IMPRESSION = "impression";
        private const string EVENT_TYPE_APP = "app";
        private const string EVENT_TYPE_CUSTOM = "custom";

        private Dictionary<string, object> _values;
        private string _name;
        private string _jsonData;
        private string _customVariablesData;
        private string _contextVariablesData;

        internal string GetName() => _name;

        private static readonly string[] ImpressionEvents = {
            VanEventName.fs_shown.ToString(),
            VanEventName.fs_click.ToString(),
            VanEventName.fs_watched.ToString(),
            VanEventName.fs_trigger.ToString(),
            VanEventName.fs_failed.ToString(),
            VanEventName.rv_shown.ToString(),
            VanEventName.rv_click.ToString(),
            VanEventName.rv_watched.ToString(),
            VanEventName.rv_trigger.ToString(),
            VanEventName.rv_failed.ToString(),
            VanEventName.banner_shown.ToString(),
            VanEventName.banner_click.ToString(),
            VanEventName.thumbnail_click.ToString(),
            VanEventName.thumbnail_shown.ToString(),
            VanEventName.native_shown.ToString(),
            VanEventName.native_click.ToString(),
            VanEventName.native_trigger.ToString(),
            VanEventName.native_closed.ToString(),
            VanEventName.native_failed.ToString(),
            VanEventName.ad_revenue.ToString(),
            VanEventName.cp_impression.ToString(),
            VanEventName.cp_click.ToString(),
            VanEventName.cp_response_status.ToString(),
            VanEventName.attribution_changed.ToString(),
            VanEventName.audio_ad_shown.ToString(),
            VanEventName.audio_ad_clicked.ToString(),
            VanEventName.audio_ad_trigger.ToString(),
            VanEventName.audio_ad_closed.ToString(),
            VanEventName.audio_ad_watched.ToString(),
            VanEventName.audio_ad_failed.ToString(),
            VanEventName.attribution_changed.ToString(),
            VanEventName.ao_trigger.ToString(),
            VanEventName.ao_shown.ToString(),
            VanEventName.ao_clicked.ToString(),
            VanEventName.ao_watched.ToString(),
            VanEventName.ao_failed.ToString()
        };

        private VanEvent() { }

        // UnityEngine's classes shouldn't be used here because this method may be called in a background thread 
        internal static void Create(string name,
                                    VanEventBasicParameters parameters,
                                    string dataJson,
                                    string customVariablesJson,
                                    string contextVariablesJson,
                                    string type,
                                    Action<VanEvent> complete,
                                    [CanBeNull] string eventId,
                                    VanSessionInfo sessionInfo)
        {
            eventId = eventId ?? Guid.NewGuid().ToString();
            type = type ?? GetType(name);
            string createAt = DateTime.UtcNow.ToIsoFormat();
            string trackingId = GetTrackingId(new TrackingIdParameters{
                eventId = eventId,
                name = name,
                type = type,
                sessionId = sessionInfo.id,
                createdAt = createAt,
                advertisingId = parameters.GetAdvertisingId(),
                bundleId = parameters.GetAppBundleId(),
                platform = parameters.GetPlatform(),
                developerDeviceId = parameters.GetDeveloperDeviceId(),
                secretKey = AnalyticsConstant.SECRET_KEY
            });
            var eventValues = new Dictionary<string, object> {
                {AnalyticsConstant.ID, eventId},
                {AnalyticsConstant.NAME, name},
                {AnalyticsConstant.TYPE, type},
                {AnalyticsConstant.CREATED_AT, createAt},
                {AnalyticsConstant.SESSION_ID, sessionInfo.id},
                {AnalyticsConstant.SESSION_LENGTH, sessionInfo.length},
                {AnalyticsConstant.SESSION_COUNT, sessionInfo.count},
                {AnalyticsConstant.VS_VERSION, parameters.GetVSVersion()},
                {AnalyticsConstant.USER_ID, parameters.GetUserId()},
                {AnalyticsConstant.BUNDLE_ID, parameters.GetAppBundleId()},
                {AnalyticsConstant.APP_VERSION, parameters.GetAppVersion()},
                {AnalyticsConstant.LOCALE, parameters.GetLocale()},
                {AnalyticsConstant.CONNECTIVITY, parameters.GetConnectivity()},
                {AnalyticsConstant.APP_OPEN_COUNT, parameters.GetAppLaunchCount()},
                {AnalyticsConstant.USER_GAME_COUNT, parameters.GetGameCount()},
                {AnalyticsConstant.SCREEN_RESOLUTION, parameters.GetScreenResolution()},
                {AnalyticsConstant.OS_VERSION, parameters.GetOSVersion()},
                {AnalyticsConstant.MANUFACTURER, parameters.GetManufacturer()},
                {AnalyticsConstant.MODEL, parameters.GetModel()},
                {AnalyticsConstant.DEVELOPER_DEVICE_ID, parameters.GetDeveloperDeviceId()},
                {AnalyticsConstant.ADS_CONSENT_GIVEN, parameters.GetAdsConsentGiven()},
                {AnalyticsConstant.ANALYTICS_CONSENT_GIVEN, parameters.GetAnalyticsConsentGiven()},
                {AnalyticsConstant.PLATFORM, parameters.GetPlatform()},
                {AnalyticsConstant.ADVERTISING_ID, parameters.GetAdvertisingId()},
                {AnalyticsConstant.LIMIT_AD_TRACKING, parameters.HasLimitAdTrackingEnabled()},
                {AnalyticsConstant.TRACKING_ID, trackingId},
            };

            eventValues.AddIfNotNull(AnalyticsConstant.CURRENT_LEVEL, parameters.GetCurrentLevel());
            eventValues.AddIfNotNull(AnalyticsConstant.IDFA_AUTHORIZATION_STATUS, parameters.GetIdfaAuthorizationStatus());
            eventValues.AddIfNotNull(AnalyticsConstant.MEDIATION, parameters.GetMediation());
            eventValues.AddIfNotNull(AnalyticsConstant.SEGMENT_UUID, parameters.GetSegmentationUuid());
            eventValues.AddIfNotNull(AnalyticsConstant.AB_TEST_UUID, parameters.GetAbTestUuid());
            eventValues.AddIfNotNull(AnalyticsConstant.COHORT_UUID, parameters.GetAbTestCohortUuid());
            eventValues.AddIfNotNull(AnalyticsConstant.INSTALL_STORE, parameters.GetInstallStore());
            eventValues.AddIfNotNull(AnalyticsConstant.FIRST_APP_LAUNCH_DATE, parameters.GetFirstAppLaunchDate());
            eventValues.AddIfNotNull(AnalyticsConstant.VERSION_UUID, parameters.GetAbTestVersionUuid());
            eventValues.AddIfNotNull(AnalyticsConstant.ATTRIBUTION_PROVIDER_NAME, parameters.GetAttributionDataName());

            complete(new VanEvent {
                _values = eventValues, _name = name, _jsonData = dataJson, _customVariablesData = customVariablesJson,
                _contextVariablesData = contextVariablesJson
            });
        }

        private struct TrackingIdParameters
        {
            public string eventId;
            public string name;
            public string type;
            public string sessionId;
            public string createdAt;
            public string advertisingId;
            public string bundleId;
            public string platform;
            public string developerDeviceId;
            public string secretKey;
        }

        private static string GetTrackingId(TrackingIdParameters parameters)
        {
            //the dictionary order is very important, if you do not respect it, the tracking id will be wrong and the event will be rejected
            var header = new Dictionary<string, object> {
                {AnalyticsConstant.ID, parameters.eventId},
                {AnalyticsConstant.NAME, parameters.name},
                {AnalyticsConstant.TYPE, parameters.type},
                {AnalyticsConstant.SESSION_ID, parameters.sessionId},
                {AnalyticsConstant.CREATED_AT, parameters.createdAt},
                {AnalyticsConstant.ADVERTISING_ID, parameters.advertisingId},
                {AnalyticsConstant.BUNDLE_ID, parameters.bundleId},
                {AnalyticsConstant.PLATFORM, parameters.platform},
                {AnalyticsConstant.DEVELOPER_DEVICE_ID, parameters.developerDeviceId}
            };
            Byte[] headerBytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(header.ToJson())));
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(parameters.secretKey))) {
                return Convert.ToBase64String(hmac.ComputeHash(headerBytes));
            }
        }

        private static string GetType(string name)
        {
            string type;
            if (!Enum.IsDefined(typeof(VanEventName), name)) {
                type = EVENT_TYPE_CUSTOM;
            } else if (ImpressionEvents.Contains(name)) {
                type = EVENT_TYPE_IMPRESSION;
            } else {
                type = EVENT_TYPE_APP;
            }

            return type;
        }

        internal string ToJson() => AnalyticsUtil.ConvertDictionaryToJson(_values, _jsonData, _customVariablesData, _contextVariablesData);

        public override string ToString() => _values.ToString();
    }
}