using System;
using System.Threading.Tasks;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Privacy
{
    public interface IPrivacyAPI
    {
        void Initialize(IRequest request);

        Task<NeedConsent> NeedConsent(NeedConsentParams data);

        Task<bool> ConsentInsights(ConsentInsightsParams data);
        
        Task<PrivacyRequest> DeleteDataRequest(DeleteDataParameters p);
        
        Task<PrivacyRequest> AccessDataRequest(AccessDataParameters p);
    }
    
    [Serializable]
    public class DeleteDataParameters
    {
        public string uuid;
        public string appVersion;
        public string vsVersion;
        public string bundleId;
        public string platform;
        public string locale;
        public string email;
        public string studioName;
        public int gdprType;
        public string vendor_id;
        public string voodoo_user_id;
    }
    
    [Serializable]
    public class AccessDataParameters
    {
        public string uuid;
        public string appVersion;
        public string vsVersion;
        public string bundleId;
        public string platform;
        public string locale;
        public string email;
        public string studioName;
        public int gdprType;
        public string vendor_id;
        public string voodoo_user_id;
    }

    [Serializable]
    public class PrivacyRequestParams
    {
        public string uuid;
        public string appVersion;
        public string vsVersion;
        public string bundleId;
        public string platform;
        public string locale;
        public string action;
        public string phoneNumber;
        public string phoneArea;
        public string email;
        public string studioName;
        public int gdprType;
        public string vendor_id;
        public string voodoo_user_id;
    }

    [Serializable]
    public class PrivacyRequest
    {
        public string success;
        public string errorMessage;
    }
    
    [Serializable]
    public class NeedConsent
    {
        public bool need_consent; //always true, before gdpr_applicable
        public bool already_consent;
        public bool embargoed_country;
        public string country_code;
        public bool ads_consent;
        public bool analytics_consent;
        public string texts;
        public bool is_gdpr; //new field
        public string privacy_version; //new field
        public bool is_ccpa; //new field
        public bool ads_enforcement; //new CNIL enhancement field
        public bool adjust_enforcement; //new CNIL enhancement field
        public bool van_enforcement; //new CNIL enhancement field
        public int consent_version;

        public OfferWallStatus OfferWall => (OfferWallStatus)consent_version;
        
        public enum OfferWallStatus
        {
            Disabled = 0,
            Enabled = 1,
            LimitedPlayTime = 2
        }
    }

    [Serializable]
    public class NeedConsentParams
    {
        public string bundle_id;
        public string user;
        public string popup_version;
        public string os_type;
        public string app_version;
        public string vs_version;
        public string locale; //mendatory
        public string uuid;
        public string studio_name;
        public int gdpr_type;
        public string vendor_id;
        public string voodoo_user_id;
        public string idfa_authorization_status;

        public override string ToString() => $"?locale={locale}";
    }

    [Serializable]
    public class ConsentInsightsParams
    {
        public string bundle_id; //mendatory
        public string user;
        public string popup_version;
        public bool ads_consent; //mendatory
        public bool analytics_consent; //mendatory
        public string os_type;
        public string app_version;
        public string vs_version;
        public string locale; //mendatory
        public string uuid; //mendatory
        public string studio_name;
        public int gdpr_type;
        public string vendor_id;
        public string voodoo_user_id;
        public string idfa_authorization_status;

        public override string ToString()
        {
            var parameters = string.Format(
                "?bundle_id={0}&ads_consent={1}&analytics_consent={2}&locale={3}&uuid={4}",
                bundle_id, ads_consent, analytics_consent, locale, uuid);
            
            if (!string.IsNullOrEmpty(user)) {
                parameters += $"&user={user}";
            }
            
            if (!string.IsNullOrEmpty(popup_version)) {
                parameters += $"&popup_version={popup_version}";
            }
            
            if (!string.IsNullOrEmpty(os_type)) {
                parameters += $"&os_type={os_type}";
            }
            
            if (!string.IsNullOrEmpty(app_version)) {
                parameters += $"&app_version={app_version}";
            }
            
            if (!string.IsNullOrEmpty(vs_version)) {
                parameters += $"&vs_version={vs_version}";
            }
            
            if (!string.IsNullOrEmpty(studio_name)) {
                parameters += $"&studio_name={studio_name}";
            }

            if (!string.IsNullOrEmpty(vendor_id)) {
                parameters += $"&vendor_id={vendor_id}";
            }

            if (!string.IsNullOrEmpty(voodoo_user_id)) {
                parameters += $"&voodoo_user_id={voodoo_user_id}";
            }
            
            if (!string.IsNullOrEmpty(idfa_authorization_status)) {
                parameters += $"&idfa_authorization_status={idfa_authorization_status}";
            }

            return parameters;

        }
    }
}
