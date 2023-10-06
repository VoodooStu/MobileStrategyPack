using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    public static class EnvironmentSettings
    {
        private const string ANALYTICS_SERVER_KEY = "AnalyticsServer";
        private const string PROXY_SERVER_KEY = "ProxyServer";
        
        public enum Server
        {
            Tech = 0,
            Staging = 1,
            Dev = 2
        }

        // This property is internal and not readonly
        // because it could be reassigned by unit/integration tests
        internal static bool IsDevelopmentBuild = Debug.isDebugBuild;

        internal static Server GetAnalyticsServer()
        {
            Server defaultServer = IsDevelopmentBuild ? Server.Staging : Server.Tech;
            return (Server)PlayerPrefs.GetInt(ANALYTICS_SERVER_KEY, (int) defaultServer);
        }

        /// <summary> 
        ///    <param>Return the full formatted url.</param>
        ///     <param name="path">Url with {0} instead of the server name</param>
        ///    <example>https://vs-api.voodoo-{0}.io/push-analytics-v2</example>
        /// </summary>
        internal static string GetAnalyticsURL(string path) => string.Format(path, GetAnalyticsServer());
       
        internal static void SaveAnalyticsServer(Server server) => PlayerPrefs.SetInt(ANALYTICS_SERVER_KEY, (int) server);
        
        internal static string GetProxyServer() => PlayerPrefs.GetString(PROXY_SERVER_KEY);

        internal static void SaveProxyServer(string proxyServer) => PlayerPrefs.SetString(PROXY_SERVER_KEY, proxyServer);
    }
}