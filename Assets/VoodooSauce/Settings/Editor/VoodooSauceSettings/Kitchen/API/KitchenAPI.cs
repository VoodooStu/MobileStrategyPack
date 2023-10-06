using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using UnityEngine;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    public static class KitchenAPI
    {
        private const string HOST_URL = "https://vsauce-api.voodoo-tech.io";

        // Flag to know if the VS Settings are currently being refreshed from Kitchen.
        private static bool _refreshing;
        
        // The ONLY purpose of this method is to inject fake requests for the integration tests.
        // For integration tests use mocked objects from IKitchenAPIRequest.
        public static Func<IKitchenAPIRequest> CreateWebRequest = () => new KitchenAPIRequest();

        // Download all the settings and the files from Kitchen and store them locally.
        // The callback has 3 parameters:
        // - the first one is the up-to-date settings
        // - the second one is a boolean, true if the settings have been downloaded, false if they are cached or if an error occurs
        // - the last one is a string representing the error if there is any.
        public static IEnumerator DownloadSettings(Action<KitchenSettingsJSON, bool, string> onComplete)
        {
            if (_refreshing) {
                onComplete?.Invoke(null, false, "The settings are already refreshing");
                yield break;
            }
            
            // Get the access token from the settings.
            string accessToken = GetAccessTokenID();
            if (string.IsNullOrEmpty(accessToken)) {
                onComplete?.Invoke(null, false, "Missing Access Token ID");
                yield break;
            }

            _refreshing = true;
            
            // Get the URL of the route.
            string url = HOST_URL + $"/v2/settings?access_token={accessToken}";
            Debug.Log($"Get Kitchen settings from {url}");

            // Execute the request.
            IKitchenAPIRequest webRequest = CreateWebRequest();
            webRequest.Url = url;
            webRequest.Timeout = 20;

            // If the web request is not live it means that it's an integration test.
            // Live request will update all the settings from the Voodoo servers.
            if (webRequest.IsLiveRequest) {
                yield return webRequest.SendWebRequest();
            
                // Wait for the response.
                while (!webRequest.IsDone) {
                    yield return null;
                }
            }

            // Warn the user if any error occurs.
            if (webRequest.IsHttpError || webRequest.IsNetworkError) {
                string error = webRequest.Error;
                if (webRequest.IsNetworkError) {
                    error += ". Is your Internet connection active?";
                }
                
                Debug.LogError($"Can not get the settings from Kitchen: {error}");
                
                _refreshing = false;
                onComplete?.Invoke(null, false, error);
                yield break;
            }

            // Parse the response.
            string jsonContent = webRequest.TextResponse;
            var remoteSettings = KitchenSettingsJSON.CreateFromJson(jsonContent);
            
            Debug.Log("Kitchen settings are downloaded");

            // Save the response.
            if (!CachedSettingsHelper.SaveTempSettings(jsonContent)) {
                _refreshing = false;
                onComplete?.Invoke(null, false, "Can not save the Kitchen settings");
                yield break;
            }
            
            // Handle the downloadable assets: each file is compared to the saved one if needed.
            // If the saved file is outdated, then it's replaced with the new one which is downloaded first.
            foreach (KitchenStoreJSON remoteStoreSettings in remoteSettings.StoreSettings) {
                KitchenKeysJSON remoteStoreKeys = remoteStoreSettings.settings;
                foreach (FieldInfo field in remoteStoreKeys.GetType().GetFields()) {
                    var remoteValue = (KitchenValueJSON)field.GetValue(remoteStoreKeys);
                    if (!remoteValue.IsFile()) {
                        continue;
                    }

                    string fileUrl = remoteValue.value;
                    if (string.IsNullOrEmpty(fileUrl)) {
                        Debug.LogWarning($"File URL for '{field.Name}' is missing for store: "
                            + $"{remoteStoreSettings.store} and platform: {remoteStoreSettings.platform}.");
                        continue;
                    }
                    
                    var uri = new Uri(fileUrl);
                    string filename = Path.GetFileName(uri.LocalPath);
                    using (var client = new WebClient()) {
                        string storeDirectory = CachedSettingsHelper.GetTempStoreFilesDirectory(remoteStoreSettings.id);
                        if (!Directory.Exists(storeDirectory)) {
                            Directory.CreateDirectory(storeDirectory);
                        }

                        // If the web request is not live it means that it's an integration test.
                        // Live request will update all the settings from the Voodoo servers.
                        string destinationFileName = Path.Combine(storeDirectory, filename);
                        if (webRequest.IsLiveRequest) {
                            client.DownloadFile(fileUrl, destinationFileName);
                        } else {
                            string sourceFilename = Path.Combine(webRequest.FilesPath, remoteStoreSettings.id, filename);
                            
                            if (File.Exists(destinationFileName)) {
                                File.Delete(destinationFileName);
                            }
                            
                            if (File.Exists(sourceFilename)) {
                                File.Copy(sourceFilename, destinationFileName);
                            } else {
                                Debug.Log($"File {filename} can not be copied because it does not exist at '{sourceFilename}'");
                            }
                        }
                    }
                    
                    Debug.Log($"File {filename} downloaded from {fileUrl}");
                }
            }

            // Save the settings and the files to the final location.
            if (!CachedSettingsHelper.SaveSettingsAndFiles()) {
                _refreshing = false;
                onComplete?.Invoke(null, false, "Can not save the Kitchen settings");
                yield break;
            }
            
            _refreshing = false;
            onComplete?.Invoke(remoteSettings, true, "");
        }

        private static string GetAccessTokenID()
        {
            VoodooSettings voodooSettings = VoodooSettings.Load();
            string accessTokenID = voodooSettings.AccessTokenID;
            if (string.IsNullOrEmpty(accessTokenID)) {
                Debug.LogError($"The Access Token ID is not provided");
            }

            return accessTokenID;
        }

        public static bool CheckForMissingFiles(out string missingFiles)
        {
            var files = new List<string>();
            KitchenSettingsJSON remoteSettings = KitchenSettingsJSON.Load();

            KitchenKeysJSON storeSettings = remoteSettings.GetSettingsFromCurrentStoreAndPlatform().settings;
            foreach (FieldInfo field in storeSettings.GetType().GetFields()) {
                var remoteValue = (KitchenValueJSON) field.GetValue(storeSettings);
                if (!remoteValue.IsFile()) {
                    continue;
                }
                
                string fileUrl = remoteValue.value;
                if (string.IsNullOrEmpty(fileUrl)) {
                    files.Add(field.Name);
                }
            }
            
            missingFiles = string.Join(", ", files);
            return files.Count > 0;
        }
    }
}