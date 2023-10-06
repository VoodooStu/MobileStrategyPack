using Voodoo.Analytics;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class VoodooAnalyticsParameters
    {
        public bool UseVoodooTune { get; }
        public bool UseVoodooAnalytics { get; }
        public string LegacyABTestName { get; }
        public string ProxyServer { get; }

        public VoodooAnalyticsParameters(bool useVoodooTune, bool useVoodooAnalytics, string legacyAbTestName, string proxyServer)
        {
            UseVoodooTune = useVoodooTune;
            UseVoodooAnalytics = useVoodooAnalytics;
            LegacyABTestName = legacyAbTestName;
            ProxyServer = proxyServer;
        }

        internal VanSessionInfo GetSessionInfo()
        {
            SessionInfo sessionInfo = AnalyticsSessionManager.Instance().SessionInfo;
            return new VanSessionInfo
            {
                id = sessionInfo.id,
                count = sessionInfo.count,
                length = sessionInfo.length
            };
        }
    }
}