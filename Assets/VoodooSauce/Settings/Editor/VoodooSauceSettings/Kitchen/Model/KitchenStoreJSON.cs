using System;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This class represents the JSON structure 'store_setting' returning by the Kitchen server.
     */

    [Serializable]
    public class KitchenStoreJSON
    {
        public const string PLATFORM_IOS = "ios";
        public const string PLATFORM_ANDROID = "android";
        
        public string id;
        public string platform;
        public string store;
        public string name;
        public string description;
        
        [SerializeField] private string last_update = "";
        public DateTime LastUpdate {
            get => DateTimeHelper.StringToDateTime(last_update);
            set => last_update = DateTimeHelper.DateTimeToString(value);
        }
        
        [SerializeField] private string bundle_id;
        public string BundleId {
            get => bundle_id;
            set => bundle_id = value;
        }

        public KitchenKeysJSON settings;

        public bool IsInitialized => last_update != "";

        public KitchenStoreJSON()
        {
            settings = new KitchenKeysJSON();
        }

        public static string GetCurrentPlatform()
        {
            if (PlatformUtils.UNITY_IOS)
                return PLATFORM_IOS;
            if (PlatformUtils.UNITY_ANDROID)
                return PLATFORM_ANDROID;
            return null;
        }
    }
}