using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Analytics.VoodooAnalytics._3rdParty.Common;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Voodoo.Sauce.Core;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
// ReSharper disable once CheckNamespace
namespace Voodoo.Analytics
{
    internal static class AnalyticsApi
    {
        private const string TAG = "Analytics - Sender";
        private static HttpClient _client;
        private static string AnalyticsGatewayUrl { get; set; }
        public static string ProxyServer { get; set; }

        private static HttpClient DefaultVoodooAnalyticsHttpClient()
        {
            if (_client == null) {
                HttpMessageHandler handler = string.IsNullOrEmpty(ProxyServer)
                    ? null
                    : new HttpClientHandler {
                        // The proxy should be manually configured to make VAN debugging possible with Charles Proxy
                        Proxy = new VoodooWebProxy(ProxyServer),
                        UseProxy = true
                    };

                handler = new AnalyticsHttpLoggingClientHandler(handler ?? new HttpClientHandler());
                _client = new HttpClient(handler);
            }

            return _client;
        }

        // Needs to be set externally 
        // As the code here runs in a separate thread 
        // That can no longer rely on Unity calls
        internal static void SetAnalyticsGatewayUrl(string url)
        {
            AnalyticsGatewayUrl = url;
        }

        //Make the client set(able) from outside to enable integration test and simple inversion of control
        //Set it to null to use the default httpclient
        internal static void SetHttpClient([CanBeNull] HttpClient httpClient)
        {
            _client = httpClient;
        }

        internal static async void SendEvents(List<string> events,string bundleId, Action<bool,HttpStatusCode?> complete)
        {
            AnalyticsLog.Log(TAG, "Send " + events.Count + " event(s)");
            var json = "[" + string.Join(",", events) + "]";
            var data = new StringContent(json);
            data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            data.Headers.Add("Charset","UTF-8");
            data.Headers.Add("X-Voodoo-App",bundleId);

            try {
                HttpResponseMessage response = await DefaultVoodooAnalyticsHttpClient().PostAsync(AnalyticsGatewayUrl, data);
                AnalyticsLog.Log(TAG, "Response: " + response);
                complete(response.IsSuccessStatusCode,response.StatusCode);
                if (response.IsSuccessStatusCode) {
                    AnalyticsLog.Log(TAG, "Successfully pushed " + events.Count + " events");
                    AnalyticsEventLogger.GetInstance()
                                        .LogEventsSentSuccessfully(events);
                } else {
                    AnalyticsLog.Log(TAG, "Error when sending events: " + response.StatusCode + " " + response.ReasonPhrase);
                    string error = response.Content?.ReadAsStringAsync().Result ?? response.ReasonPhrase;
                    AnalyticsEventLogger.GetInstance().LogEventsSentError(events, error);
                }

#if ALTTESTER
                AnalyticsTextFileLogger.LogToText(response);
#endif
                
                response.Dispose();
            } catch (Exception e) {
                //Only log in crashlytics if its not HttpRequestException, not WebException and not TaskCanceledException 
                if (!(e is HttpRequestException || e is WebException || e is TaskCanceledException)) {
                    VoodooSauceCore.GetCrashReport().LogException(e);
                }

                AnalyticsEventLogger.GetInstance()
                                    .LogEventsSentError(events, e.ToString());
                complete(false,null);
                
#if ALTTESTER
                AnalyticsTextFileLogger.LogExceptionToText(e);
#endif
            }
        }
    }
}