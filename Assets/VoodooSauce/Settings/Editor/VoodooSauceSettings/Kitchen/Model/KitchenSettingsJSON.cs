using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This class represents the JSON root structure returned by the Kitchen server.
     */
    
    [Serializable]
    public class KitchenSettingsJSON
    {
        [SerializeField]
        private List<KitchenStoreJSON> store_settings = new List<KitchenStoreJSON>();
        public List<KitchenStoreJSON> StoreSettings {
            get => store_settings;
            set => store_settings = value;
        }

        [SerializeField]
        private string last_update = "";
        public DateTime LastUpdate {
            get => DateTimeHelper.StringToDateTime(last_update);
            set => last_update = DateTimeHelper.DateTimeToString(value);
        }

        public bool IsInitialized => last_update != "";

        // Returns true if these settings are up-to-date.
        public bool IsUpToDate(KitchenSettingsJSON other) => IsInitialized && LastUpdate >= other.LastUpdate;

        // Returns all the keys from a store configuration.
        public KitchenKeysJSON GetKeysFromID(string settingsId) => store_settings.Where(s => s.id == settingsId).Select(s => s.settings).FirstOrDefault();

        // Returns all the keys from a store configuration and a platform.
        public KitchenStoreJSON GetSettingsFromStoreAndPlatform(string store, string platform) => store_settings.FirstOrDefault(s => s.store == store && s.platform == platform);
        
        // Returns settings for the current selected store and platform
        public KitchenStoreJSON GetSettingsFromCurrentStoreAndPlatform()
        {
            VoodooSettings settings = VoodooSettings.Load();
            
            string store = settings.Store;
            string platform = KitchenStoreJSON.GetCurrentPlatform();
            return GetSettingsFromStoreAndPlatform(store, platform);
        }

        // Returns true if the asked store is in the cached settings.
        public bool IsStoreSupported(string store) => store_settings.FirstOrDefault(s => s.store == store) != null;

        // Returns all the stores available.
        public IEnumerable<string> GetStores() => store_settings.Select(s => s.store).Distinct();

        // Creates and returns a KitchenSettingsJSON object from a JSON string.
        public static KitchenSettingsJSON CreateFromJson(string json) => JsonUtility.FromJson<KitchenSettingsJSON>(json);

        // Load the cached KitchenSettingsJSON.
        public static KitchenSettingsJSON Load()
        {
            try {
                string content = File.ReadAllText(CachedSettingsHelper.GetSettingsPath(), Encoding.UTF8);
                return CreateFromJson(content);
            } catch (Exception ex) {
                Debug.Log($"Can not load the Kitchen Settings: {ex.Message}");
            }

            return new KitchenSettingsJSON();
        }
    }
}