using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Privacy
{
    public static class PrivacyUtils
    {
        private const string PopupVersion = "2.2";
        
        private static readonly PrivacyInfoList _analyticsSDKs = new PrivacyInfoList();
        private static readonly PrivacyInfoList _adNetworkSDKs = new PrivacyInfoList();
        private static bool _areSDKsInitialized = false;
        
        private static readonly Regex ValidateEmailRegex = new Regex("^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$");

        public enum GDPRType
        {
            Voodoo = 0,
            VoodooSP = 1,
            ATT = 2,
        }

#if UNITY_IOS && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern string _getNativeLocale();
#endif

        public static GDPRType GetGDPRType()
        {
            return GDPRType.Voodoo;
        }

        public static string GetLocale()
        {
#if UNITY_EDITOR
            return "us";
#elif UNITY_IOS
            return _getNativeLocale();
#elif UNITY_ANDROID
            var locale = new AndroidJavaClass("java.util.Locale");
            var defautLocale = locale.CallStatic<AndroidJavaObject>("getDefault");
            var country = defautLocale.Call<string>("getCountry");
            return country;
#else
            return "ZZ";
#endif
        }

        public static string GetOsType()
        {
#if UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#else
            return "unknown";
#endif
        }

        public static string GetUserStatus()
        {
            return AnalyticsStorageHelper.Instance.IsFirstAppLaunch() ? "new" : "old";
        }

        public static string GetVSVersion()
        {
            return VoodooSauce.Version();
        }

        public static string GetAppVersion()
        {
            return Application.version;
        }

        public static string GetPopupVersion()
        {
            return PopupVersion;
        }

        public static bool IsValidEmail(string email) => ValidateEmailRegex.IsMatch(email);

        public static string[] GetAdNetworkNames()
        {
            InitializePrivacyLinks();
            return _adNetworkSDKs.GetNames();
        }

        public static IEnumerable<string> GetAdvertisingPrivacyPolicyUrls()
        {
            InitializePrivacyLinks();
            return _adNetworkSDKs.GetPrivacyPolicyUrls();
        }

        public static string[] GetAnalyticsNames()
        {
            InitializePrivacyLinks();
            return _analyticsSDKs.GetNames();
        }

        public static IEnumerable<string> GetAnalyticsPrivacyPolicyUrls()
        {
            InitializePrivacyLinks();
            return _analyticsSDKs.GetPrivacyPolicyUrls();
        }

        public static IEnumerable<string> GetPrivacyPolicyUrls()
        {
            InitializePrivacyLinks();
            return _analyticsSDKs.Select(info => info.Name).Concat(_adNetworkSDKs.Select(info => info.Name));
        }

        private static void InitializePrivacyLinks()
        {
            if (_areSDKsInitialized) {
                return;
            }
            
            List<Type> GDPRLinkTypes = AssembliesUtils.GetTypes(typeof(IPrivacyLink));

            foreach (MediationType mediationType in (MediationType[]) Enum.GetValues(typeof(MediationType))) {
                foreach (Type GDPRLinkType in GDPRLinkTypes) {
                    var GDPRLink = (IPrivacyLink) Activator.CreateInstance(GDPRLinkType);

                    string sdkName = GDPRLink.SDKName;
                    string privacyPolicyUrl = GDPRLink.PrivacyPolicyUrl;

                    if (sdkName.Length == 0 || privacyPolicyUrl.Length == 0) {
                        continue;
                    }

                    var GDPRInfo = new PrivacyInfo {Name = sdkName, PrivacyPolicyUrl = privacyPolicyUrl};

                    switch (GDPRLink.SDKType)
                    {
                        case PrivacySDKType.Analytics:
                            _analyticsSDKs.Add(GDPRInfo);
                            break;

                        case PrivacySDKType.AdNetworkMaxAds:
                            if (mediationType == MediationType.MaxAds)
                            {
                                _adNetworkSDKs.Add(GDPRInfo);
                            }

                            break;

                        case PrivacySDKType.AdNetworkIronSource:
                            if (mediationType == MediationType.IronSource)
                            {
                                _adNetworkSDKs.Add(GDPRInfo);
                            }

                            break;
                        case PrivacySDKType.AdNetwork:
                            _adNetworkSDKs.Add(GDPRInfo);
                            break;
                    }
                }

                _areSDKsInitialized = true;
            }
        }
    }
}