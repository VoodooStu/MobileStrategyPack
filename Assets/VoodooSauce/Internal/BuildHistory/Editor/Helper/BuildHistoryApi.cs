// ReSharper disable CheckNamespace
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class BuildHistoryApi
    {
        private static HttpClient _client;
        private static string RemoteUrl => "https://vs-app-metadata.voodoo-tech.io/vs-app-metadata-api";
        
        private static HttpClient DefaultVoodooAnalyticsHttpClient()
        {
            if (_client == null) {
                _client = new HttpClient();
            }

            return _client;
        }

        internal static async void SendEvents(string jsonBody)
        {
            var data = new StringContent(jsonBody);
            data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            data.Headers.Add("Charset", "UTF-8");

            try {
                HttpResponseMessage response = await DefaultVoodooAnalyticsHttpClient().PostAsync(RemoteUrl, data);
                if (response.IsSuccessStatusCode) {
                    Debug.Log("Build History Sent: "+jsonBody);
                } else {
                    string error = response.Content?.ReadAsStringAsync().Result ?? response.ReasonPhrase;
                    Debug.LogWarning("Error happened while sending analytics data, this wont break the build,"
                        + " but please share the exception below to the VoodooSauce Team");
                    Debug.LogWarning(error);
                }

                response.Dispose();
            } catch (Exception e) {
                Debug.LogError("Error happened while sending analytics data, this wont break the build,"
                    + " but please share the exception below to the VoodooSauce Team");
                Debug.LogException(e);
            }
        }
    }
}