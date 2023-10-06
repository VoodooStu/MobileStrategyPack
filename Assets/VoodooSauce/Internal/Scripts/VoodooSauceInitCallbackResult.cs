// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    /// <summary>
    /// This class is used to return result from VoodooSauce initialization to the Initialization Finished Event
    /// </summary>
    public class VoodooSauceInitCallbackResult
    {
        /// <summary>
        /// Correspond to Ads Personalization toggle in the Privacy screen, it is enabled by default for users outside of EU
        /// </summary>
        public bool AdsConsentGranted { get; set; }
        /// <summary>
        /// Correspond to Analytics toggle in the Privacy screen, it is enabled by default for users outside of EU
        /// </summary>
        public bool AnalyticsConsentGranted { get; set; }
        /// <summary>
        /// Indicating if user is located in embargo country or not
        /// </summary>
        public bool IsEmbargoCountry { get; set; }

        public override string ToString() => "AdsConsentGranted="+AdsConsentGranted
            + ", AnalyticsConsentGranted="+AnalyticsConsentGranted+", "
            + "IsEmbargoCountry="+IsEmbargoCountry;
    }
}