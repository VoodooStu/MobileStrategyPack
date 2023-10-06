using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Voodoo.Tune.Network;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR || UNITY_IOS
using UnityEngine.iOS;
#endif
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Voodoo.Tune.Utils;
#endif

namespace Voodoo.Tune.Core
{
    public class VoodooTuneConfigurationManager
    {
        private const string DefaultFolder = "DefaultJsons/";
        private const string BaseConfigurationURL = "https://remote-settings-v2.voodoo-{0}.io/api/settings";
        private readonly string BaseConfigURL;

        private Dictionary<Type, List<object>> _configurations = new Dictionary<Type, List<object>>();
        private Dictionary<string, string> _jsonConfigurations = new Dictionary<string, string>();
        
        public VoodooConfig VoodooConfig { get; private set; }
        public VoodooDebug VoodooDebug { get; private set; }

        public VoodooTuneConfigurationManager() : this(VoodooTunePersistentData.SavedServer) { }

        public VoodooTuneConfigurationManager(Server server)
        {
            BaseConfigURL = string.Format(BaseConfigurationURL, server);
            LoadLocalConfiguration();
        }

        public void LoadLocalConfiguration()
        {
            Reset();
            
            string config = VoodooTunePersistentData.SavedConfig;
            
            if (config != null)
            {
                ParseConfiguration(config);
            }
        }

        private void Reset()
        {
            _configurations = new Dictionary<Type, List<object>>();
            _jsonConfigurations = new Dictionary<string, string>();
            VoodooConfig = null;
            VoodooDebug = null;
        }
        
        public void SaveConfig(string config)
        {
            VoodooTunePersistentData.SavedConfig = config;
        }

        public void SaveAndRefreshConfig(string config)
        {
            VoodooTunePersistentData.SavedConfig = config;
            LoadLocalConfiguration();
        }
        
#region Items
        /// <summary>
        /// Returns a list of instances of the given type T and its subtypes, from the VoodooTune cache.
        /// if there is no instance present in the cache, the list will contain an instance with the default values defined in the class T.
        /// </summary>
        /// <returns>A list of instances of the given type T and its subtypes.</returns>
        public List<T> GetSubclassesItems<T>() where T : class, new()
        {
            var list = new List<T>();
            List<Type> types = Assembly.GetAssembly(typeof(T))
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
                .ToList();

            types.Add(typeof(T));

            foreach (Type type in types)
            {
                try
                {
                    if (_configurations.ContainsKey(type) == false)
                    {
                        continue;
                    }

                    List<object> rawList = _configurations[type];
                    foreach (object item in rawList)
                    {
                        list.Add((T)item);
                    }
                }
                catch (Exception e)
                {
                    string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("fr-FR"));
                    string message = $"{date} - VOODOO_TUNE/ConfigurationManager: No items for the type: {e.Message}";
                    Debug.LogError(message);
                }
            }

            return list;
        }

        /// <summary>
        /// Returns a list of instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the list will contain an instance with the default values defined in the class T.
        /// </summary>
        /// <returns>A list of instances of the given type T.</returns>
        public List<T> GetItems<T>() where T : class, new()
        {
            var list = new List<T>();

            try
            {
                if (_configurations.ContainsKey(typeof(T)))
                {
                    List<object> rawList = _configurations[typeof(T)];
                    foreach (object item in rawList)
                    {
                        list.Add((T)item);
                    }
                }
            }
            catch (Exception e)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("fr-FR"));
                string message = $"{date} - VOODOO_TUNE/ConfigurationManager: No items for the type: {e.Message}";
                Debug.LogError(message);
            }

            return list;
        }
        
        /// <summary>
        /// Returns a list of instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the method returns a list of instances with the default values defined in the Resources json files.
        /// </summary>
        /// <returns>An instance of the given type T.</returns>
        public List<T> GetItemsOrDefaults<T>() where T : class, new()
        {
            List<T> list = GetItems<T>();
            if (list.Count == 0)
            {
                list = GetDefaults<T>();                
            }
            return list;
        }

        private List<T> GetDefaults<T>() where T : class, new()
        {
            var list = new List<T>();

            TextAsset ta = Resources.Load<TextAsset>(DefaultFolder + typeof(T).FullName);
            if (ta != null)
            {
                list = VoodooTuneObjects(typeof(T), ta.text).Cast<T>().ToList();
            }

            if (list.Count == 0)
            {
                list.Add(new T());
            }
            
            return list;
        }

        /// <summary>
        /// Returns the first instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the method returns null.
        /// </summary>
        /// <returns>An instance of the given type T or null.</returns>
        public T GetItem<T>() where T : class, new()
        {
            List<T> list = GetItems<T>();
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// Returns the first instances of the given type T, from the VoodooTune cache.
        /// if there is no instance present in the cache, the method returns an instance with the default values defined in the class T.
        /// </summary>
        /// <returns>An instance of the given type T. </returns>
        public T GetItemOrDefault<T>() where T : class, new()
        {
            List<T> list = GetItems<T>();
            return list.Count > 0 ? list[0] : new T();
        }

        /// <summary>
        /// Returns a string instance representing the JSON content of a given type.
        /// </summary>
        /// <param name="type">The type as string</param>
        /// <returns>A string instance of JSON content.</returns>
        public string GetJsonConfigurations(string type)
        {
            bool isValidType = _jsonConfigurations != null && type != null && _jsonConfigurations.ContainsKey(type);
            return isValidType ? _jsonConfigurations[type] : null;
        }

        /// <returns>The parsed configuration as it is in the json file</returns>
        public Dictionary<string, string> GetJsonConfigurations() => _jsonConfigurations;
#endregion

#region urls
        public string CreateDefaultConfigurationURL(Dictionary<string, string> parameters, string bundleId, string platform, string version)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }
            
            parameters.Add("bundle_id", bundleId);
            parameters.Add("app_version", version);
            parameters.Add("platform", platform);

            string url = BaseConfigURL + "?" + string.Join("&", parameters.Select(x => x.Key + "=" + x.Value).ToArray());

            return url;
        }
        
        public string CreateSimulationURL(string appId, string platform, string version, string versionId, string[] segmentIds = null, string abtestId = null, string cohortId = null)
        {
            return CreateSimulationURL(appId, platform, version, versionId, segmentIds, new[] { abtestId }, new[] { cohortId });
        }

        public string CreateSimulationURL(string appId, string platform, string version, string versionId, string[] segmentIds = null, string[] abtestIds = null, string[] cohortIds = null)
        {
            string url = BaseConfigURL + $"/simulations?appId={appId}&platform={platform}&appVersion={version}&versionId={versionId}";
            
            if (segmentIds != null && segmentIds.Length > 0)
            {
                url += $"&segments={string.Join(",", segmentIds)}";
            }
            
            if (abtestIds != null && cohortIds != null && cohortIds.Length == abtestIds.Length && abtestIds.Length > 0)
            {
                url += $"&abTestId={string.Join(",", abtestIds)}";
                url += $"&cohortId={string.Join(",", cohortIds)}";
            }

            return url;
        }

        public string CreateSandboxURL(string appId, string sandBoxId)
        {
            return BaseConfigURL + $"/sandboxes?appId={appId}&sandboxId={sandBoxId}";
        }

        public string GetConfigurationURL()
        {
            string configurationURL;
            string platform =
#if UNITY_ANDROID
                "android";
#else
                "ios";
#endif
            if (VoodooTunePersistentData.SavedCohorts?.Count > 0 || VoodooTunePersistentData.SavedSegments?.Count > 0)
            {
                string appId = VoodooTunePersistentData.SavedAppId;
                string versionId = VoodooTunePersistentData.SavedVersionId;
                string[] segmentIds = VoodooTunePersistentData.SavedSegments?.Select(x => x.Id).ToArray();
                string[] abTestIds = VoodooTunePersistentData.SavedABTests?.Select(x => x.Id).ToArray();
                string[] cohortIds = VoodooTunePersistentData.SavedCohorts?.Select(x => x.Id).ToArray();
                configurationURL = CreateSimulationURL(appId, platform, Application.version, versionId, segmentIds, abTestIds, cohortIds);
            }
            else if (VoodooTunePersistentData.SavedSandbox != null)
            {
                string appId = VoodooTunePersistentData.SavedAppId;
                string sandboxId = VoodooTunePersistentData.SavedSandbox?.Id;
                configurationURL = CreateSandboxURL(appId, sandboxId);
            }
            else
            {
                configurationURL = CreateDefaultConfigurationURL(VoodooSauceVariables.GetVSParams(), Application.identifier, platform, Application.version);
            }

            return configurationURL;
        }
        
#endregion
        
        public async Task<ConfigResponse> LoadConfiguration()
        {
            string url = GetConfigurationURL();
            return await LoadConfigurationAsync(url);
        }
        
        public async Task<ConfigResponse> LoadConfigurationAsync(string url)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            //Done to avoid first stopWatch wrong duration
            stopWatch.Stop();
            stopWatch.Restart();

            UnityWebRequest uwr = await WebRequest.GetAsync(url, null, 120); //timeout 120 seconds
            stopWatch.Stop();

            return new ConfigResponse(url, uwr, stopWatch.ElapsedMilliseconds);
        }

        public async Task<Dictionary<Type, List<object>>> LoadAndParseConfiguration()
        {
            string url = GetConfigurationURL();
            return await LoadAndParseConfigurationAsync(url);
        }

        public async Task<Dictionary<Type, List<object>>> LoadAndParseConfigurationAsync(string url)
        {
            Dictionary<object, object> jsonDictionary = await VoodooTuneRequest.GetAsync<Dictionary<object, object>>(url);
            Dictionary<Type, List<object>> configurations = ParseConfiguration(jsonDictionary);
            
            return configurations;
        }
        
        /// <summary>
        /// This method isn't using the actual configuration but the one from the json file
        /// If you want to get the actual config, use the VoodooConfig variable.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>The VoodooConfig class inside the json file</returns>
        public VoodooConfig GetVoodooConfig(string json)
        {
#if NEWTONSOFT_JSON
            string VOODOO_CONFIG_KEY = "Voodoo_Config";
            
            Dictionary<object, object> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<object, object>>(json);
            if (jsonDictionary == null || jsonDictionary.ContainsKey(VOODOO_CONFIG_KEY) == false)
            {
                return null;
            }
            
            string value = jsonDictionary[VOODOO_CONFIG_KEY].ToString();
            return JsonConvert.DeserializeObject<VoodooConfig>(value);
#endif
            return null;
        }

        public void ParseConfiguration(string json)
        {
            _configurations = new Dictionary<Type, List<object>>();
            
#if NEWTONSOFT_JSON
            Dictionary<object, object> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<object, object>>(json);
            _configurations = ParseConfiguration(jsonDictionary);
#endif
        }

        private Dictionary<Type, List<object>> ParseConfiguration(Dictionary<object, object> jsonDictionary)
        {
            Dictionary<Type, List<object>> configurations = new Dictionary<Type, List<object>>();
            _jsonConfigurations = new Dictionary<string, string>();

            if (jsonDictionary == null)
            {
                return configurations;
            }
            
#if NEWTONSOFT_JSON
            foreach (KeyValuePair<object, object> kvp in jsonDictionary)
            {
                string key = kvp.Key.ToString();
                string value = kvp.Value.ToString();
                
                _jsonConfigurations.Add(key, JToken.FromObject(kvp.Value).ToString(Formatting.None));
                
                if (key == "Voodoo_Config")
                {
                    VoodooConfig = JsonConvert.DeserializeObject<VoodooConfig>(value);
                    List<object> configs = new List<object> { VoodooConfig };
                    configurations.Add(typeof(VoodooConfig), configs);
                    
                    continue;
                }
               
                if (key == "Voodoo_Debug")
                {
                    VoodooDebug = JsonConvert.DeserializeObject<VoodooDebug>(value);
                    List<object> configs = new List<object> { VoodooDebug };
                    configurations.Add(typeof(VoodooDebug), configs);
                    
                    continue;
                }

                Type type = TypeUtility.GetType(key);

                if (type != null) 
                {
                    configurations.Add(type, VoodooTuneObjects(type, value));
                }
            }
#endif

            return configurations;
        }
        
        private List<object> VoodooTuneObjects(Type type, string json)
        {
#if NEWTONSOFT_JSON
            List<JObject> rawItems = JsonConvert.DeserializeObject<List<JObject>>(json);
            
            var items = new List<object>();
            foreach (JObject rawItem in rawItems)
            {
                try
                {
                    object item = JsonConvert.DeserializeObject(rawItem.ToString(), type, VoodooTuneRequest.SerializerSettings);
                    items.Add(item);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error when deserializing object: " + type.Name + " " + e.Message + "\n" + rawItem);
                }
            }

            return items;
#else
            return null;
#endif
        }
    }
}