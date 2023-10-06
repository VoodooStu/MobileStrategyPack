using System;

namespace Voodoo.Sauce.Internal.Analytics
{
    class AttributionData
    {
        public string Name;
    }
    internal interface IAnalyticsAttributionProvider
    {
        /// <summary>
        /// Returns the attribution data (name, user id)
        /// </summary>
        /// <returns>Returns an instance of GetAttributionData that encapsulates the attribution data if the analytics provider is the official attribution, null otherwise</returns>
        AttributionData GetAttributionData();
    }
}