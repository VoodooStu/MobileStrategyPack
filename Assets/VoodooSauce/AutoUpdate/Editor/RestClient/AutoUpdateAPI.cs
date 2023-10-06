using System;
using System.Net.Http;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class AutoUpdateAPI
    {
        private const string Url = "https://voodoosauce-sdk.s3.eu-west-2.amazonaws.com/version-metadata/version_metadata_v2.json";

        public delegate void OnLoadAutoUpdateMetadataDone(MetadataResponse response);

        public static async void LoadVoodooVersion(OnLoadAutoUpdateMetadataDone callback)
        {
            try {
                using (var client = new HttpClient()) {
                    string response = await client.GetStringAsync(Url);
                    callback?.Invoke(JsonUtility.FromJson<MetadataResponse>(response));
                }
            } catch (Exception exception) {
                // Error 404 exception is silent because we are used to voluntarily disable this feature with a dead link.
                if (!(exception is HttpRequestException) || !exception.Message.Contains("404")) {
                    Debug.Log("Issue with Auto Update API: " + exception.Message);
                }

                callback?.Invoke(null);
            }
        }
    }
}