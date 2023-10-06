using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.DebugScreen;
using Voodoo.Sauce.Internal.IntegrationCheck;
using Voodoo.Sauce.Privacy.UI;

namespace Voodoo.Sauce.Privacy
{
    /// <summary>
    /// This is the core class of the Privacy module.
    /// A "core" module class defines what the module's methods do when the module is not available/implemented.
    /// </summary>
    internal class PrivacyCore : IModule
    {
        public const string LIMITED_AD_TRACKING_ID = "00000000-0000-0000-0000-000000000000";
        
        public class GdprConsent
        {
            public bool IsGdprApplicable;
            public bool ExplicitConsentGivenForAds;
            public bool ExplicitConsentGivenForAnalytics;
            public bool IsEmbargoedCountry;
            public bool IsAdsEnforcement;
			public bool IsAdjustEnforcement;
			public bool IsVANEnforcement;
            
            public override string ToString() =>
                $"Applicable: {IsGdprApplicable}, Ads: {ExplicitConsentGivenForAds}, "
                + $"Analytics: {ExplicitConsentGivenForAnalytics}, Embargoed: {IsEmbargoedCountry},"
                + $"Ads Enforcement: {IsAdsEnforcement}" 
                + $"Adjust Enforcement: {IsAdjustEnforcement}";
        }
        
        protected string _idfa = LIMITED_AD_TRACKING_ID;
        protected bool _adTrackingEnabled = false;
        protected bool _adsConsent = false;
        protected bool _analyticsConsent = false;
        protected bool _isGdprApplicable = true;
        protected bool _isCcpaApplicable = false;
        protected GdprConsent _gdprConsent;
        protected string _vendorId;
        
        public class PrivacyManagerParameters
        {
            public PrivacyConfiguration privacyConfiguration;
            public VoodooSettings voodooSettings;
            public Action<GdprConsent> onConsentGiven;
            public Action<GdprConsent> onConsentUpdate;
            public Action<bool> onPrivacyShown;
            public Action onInternetAvailable;
            public Action onGameStarted;
            public Action onGameFinished;
            public Func<bool> hasGameStarted;
            public Action tryOpenDebugger;
            public Action onDeleteDataRequest;
            public IPrivacyCanvas canvas;
            public IRequest request;
            public IPrivacyAPI privacyAPI;// for testing purpose
            public IPrivacyUIManager privacyUIManager;// for testing purpose
        }
        
        protected IdfaAuthorizationStatus _idfaAuthorizationStatus = IdfaAuthorizationStatus.NotDetermined;

#pragma warning disable 1998
        // The warning is disabled here because it's a virtual and async method.
        public virtual async Task Instantiate(VoodooSettings voodooSettings) {
#pragma warning restore 1998
        }

        public virtual List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            // Nothing to display
            return null;
        }

        public virtual void Initialize(PrivacyManagerParameters p)
        {
            
        }

        public virtual bool HasLimitAdTrackingEnabled()
        {
            return !_adTrackingEnabled;
        }

        public virtual string GetAdvertisingId(bool forceZerosForLimitedAdTracking = true)
        {
            return _idfa;
        }

        public virtual string GetVendorId()
        {
            return _vendorId ?? "";
        }

        public virtual bool HasAdsConsent()
        {
            return _adsConsent;
        }

        public virtual bool HasAnalyticsConsent()
        {
            return _analyticsConsent;
        }

        public virtual bool IsGdprApplicable()
        {
            return _isGdprApplicable;
        }

        public virtual bool IsAdsEnforcementEnabled()
        {
            return _gdprConsent != null ? _gdprConsent.IsAdsEnforcement : true;
        }

        public virtual bool IsAdjustEnforcementEnabled()
        {
            return _gdprConsent != null ? _gdprConsent.IsAdjustEnforcement : true;
        }

        public virtual bool IsCcpaApplicable()
        {
            return _isCcpaApplicable;
        }

        public virtual bool UserRequestedToBeForgotten()
        {
            return false;
        }

        internal virtual void OpenPrivacySettings(Action onSettingsClosed = null)
        {
            
        }

        internal virtual void ShowPrivacyAuthorization()
        {
            
        }
        
        public virtual IdfaAuthorizationStatus GetAuthorizationStatus()
        {
            return _idfaAuthorizationStatus;
        }
    }
}
