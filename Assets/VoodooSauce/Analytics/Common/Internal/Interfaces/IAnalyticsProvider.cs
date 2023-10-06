using System;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal interface IAnalyticsProvider
    {
        /// <summary>
        /// This method is called by AnalyticsManager when the app starts
        /// </summary>
        /// <param name="mediation">Indicates the mediation currently running in the VS</param>
        void Instantiate(string mediation);
        
        /// <summary>
        /// This method is called by AnalyticsManager once the consent is known
        /// </summary>
        /// <param name="consent">Indicates whether the user already gave his consent</param>
        /// <param name="isChinaBuild">Indicates whether the app is built for China or for WW</param>
        void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild);
    }
}