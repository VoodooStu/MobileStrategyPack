using JetBrains.Annotations;
using Voodoo.Analytics;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class VoodooAnalyticsEventParameters : VanEventBasicParameters
    {
        private const string FAKE_VALUE = "fake";

#region Private properties

        private static readonly PrivacyCore Privacy = VoodooSauceCore.GetPrivacy();

        [CanBeNull]
        private string _vsVersion;

        [CanBeNull]
        private string _appBundleId;

        [CanBeNull]
        private string _appVersion;

        [CanBeNull]
        private string _platform;

        [CanBeNull]
        private string _developerDeviceId;

        [CanBeNull]
        private string _firstAppLaunchDate;

        [CanBeNull]
        private string _mediation;

        [CanBeNull]
        private string _segmentationUuid;

        [CanBeNull]
        private string _abTestUuid;

        [CanBeNull]
        private string _abTestCohortUuid;

        [CanBeNull]
        private string _abTestVersionUuid;

        [CanBeNull]
        private AttributionData _attributionData;

#endregion

#region VanEventBasicParameters setter methods

        internal override void SetVsVersion(string vsVersion)
        {
            _vsVersion = vsVersion;
        }

        internal override void SetAppVersion(string appVersion)
        {
            _appVersion = appVersion;
        }

        internal override void SetAppBundleId(string appBundleId)
        {
            _appBundleId = appBundleId;
        }

        internal override void SetMediation(string mediation)
        {
            _mediation = mediation;
        }

        internal override void SetSegmentationUuid(string segmentationUuid)
        {
            _segmentationUuid = segmentationUuid;
        }

        internal override void SetAbTestUuid(string abTestUuid)
        {
            _abTestUuid = abTestUuid;
        }

        internal override void SetAbTestCohortUuid(string abTestCohortUuid)
        {
            _abTestCohortUuid = abTestCohortUuid;
        }

        internal override void SetAbTestVersionUuid(string abTestVersionUuid)
        {
            _abTestVersionUuid = abTestVersionUuid;
        }

#endregion

#region VanEventBasicParameters getter methods

        public override string GetPlatform()
        {
            if (_platform != null) {
                return _platform;
            }

            if (PlatformUtils.UNITY_EDITOR) {
                _platform = "editor";
            } else if (PlatformUtils.UNITY_IOS) {
                _platform = "ios";
            } else if (PlatformUtils.UNITY_ANDROID) {
                _platform = "android";
            }

            return _platform;
        }

        public override string GetVSVersion() => _vsVersion;

        public override string GetAppBundleId() => _appBundleId;

        public override string GetAppVersion() => _appVersion;

        public override string GetScreenResolution() => DeviceUtils.GetResolution();

        public override string GetOSVersion() => DeviceUtils.OperatingSystemVersion;

        public override string GetManufacturer() => DeviceUtils.Manufacturer;

        public override string GetModel() => DeviceUtils.Model;

        public override string GetUserId() => AnalyticsUserIdHelper.GetUserId();

        public override string GetLocale() => DeviceUtils.GetLocale();

        public override string GetConnectivity() => DeviceUtils.GetConnectivity();

        public override int GetAppLaunchCount() => AnalyticsStorageHelper.Instance.GetAppLaunchCount();

        public override int GetGameCount() => AnalyticsStorageHelper.Instance.GetGameCount();

        public override string GetDeveloperDeviceId()
        {
            if (_developerDeviceId != null) {
                return _developerDeviceId;
            }

            _developerDeviceId = Privacy.GetVendorId().Replace("-", "").ToLower();

            return _developerDeviceId;
        }

        public override bool GetAdsConsentGiven() => Privacy.HasAdsConsent();

        public override bool GetAnalyticsConsentGiven() => Privacy.HasAnalyticsConsent();

        public override string GetAdvertisingId() => Privacy.GetAdvertisingId();

        public override bool HasLimitAdTrackingEnabled() => PlatformUtils.UNITY_EDITOR || Privacy.HasLimitAdTrackingEnabled();

        public override string GetIdfaAuthorizationStatus()
        {
#if UNITY_IOS
            return NativeWrapper.GetAuthorizationStatus().ToString();
#else
            return Privacy.HasLimitAdTrackingEnabled() ? IdfaAuthorizationStatus.Denied.ToString() : IdfaAuthorizationStatus.Authorized.ToString();
#endif
        }

        public override string GetInstallStore() => DeviceUtils.GetAndroidInstallStore();

        public override string GetFirstAppLaunchDate()
        {
            if (_firstAppLaunchDate != null) {
                return _firstAppLaunchDate;
            }

            _firstAppLaunchDate = AnalyticsStorageHelper.Instance.GetFirstAppLaunchDate()?.ToIsoFormat();

            return _firstAppLaunchDate;
        }

        public override string GetMediation() => _mediation;

        public override string GetSegmentationUuid() => _segmentationUuid;

        public override string GetAbTestUuid() => _abTestUuid;

        public override string GetAbTestCohortUuid() => _abTestCohortUuid;

        public override string GetAbTestVersionUuid() => _abTestVersionUuid;

        public override string GetAttributionDataName()
        {
            if (PlatformUtils.UNITY_EDITOR) return FAKE_VALUE;

            if (string.IsNullOrEmpty(_attributionData?.Name)) {
                _attributionData = AnalyticsManager.GetAttributionData();
            }

            return _attributionData?.Name;
        }

        public override string GetCurrentLevel()
        {
            string currentLevel = AnalyticsStorageHelper.Instance.GetCurrentLevel();
            return string.IsNullOrEmpty(currentLevel) ? null : currentLevel;
        }

#endregion
    }
}