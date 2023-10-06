using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class VoodooSauceSettingsChecker
    {
        private const string KITCHEN_SETTINGS_NOT_CACHED_ERROR = "the VoodooSauce settings are not cached; are the VoodooSauce settings refreshed?";
        private const string NO_STORE_SELECTED_ERROR = "no store is selected from the VoodooSauce settings.";
        private const string STORE_NOT_SUPPORTED_ERROR = "store not supported: ";
        private const string PLATFORM_NOT_SUPPORTED_ERROR = "platform not supported: ";
        private const string MD5_DIFFERENT_ERROR = "the md5 of the cached file is different from the remote one.";
        private const string INCORRECT_CHECKSUM_FORMAT_ERROR = "checksum from kitchen has an incorrect format.";

        /// <summary>
        /// Check the MD5 checksum of the Adjust signature file.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static void CheckAdjustSignature(string store, string platform)
        {
            // The JSON response from Kitchen must be cached.
            KitchenSettingsJSON kitchenSettings = KitchenSettingsJSON.Load();
            if (!kitchenSettings.IsInitialized) {
                throw new SettingsNotCreatedException(KITCHEN_SETTINGS_NOT_CACHED_ERROR);
            }
            
            // The selected store settings must be present in the cached settings.
            if (string.IsNullOrEmpty(store)) {
                throw new StoreNotSupportedException(NO_STORE_SELECTED_ERROR);
            }
            
            if (!kitchenSettings.IsStoreSupported(store)) {
                throw new StoreNotSupportedException(STORE_NOT_SUPPORTED_ERROR + store);
            }

            // The current platform must be supported by the settings from Kitchen.
            var filePath = "";
            switch (platform) {
                case KitchenStoreJSON.PLATFORM_IOS:
                    filePath = AdjustHelper.GetIOSSignaturePath();
                    break;
                case KitchenStoreJSON.PLATFORM_ANDROID:
                    filePath = AdjustHelper.GetAndroidSignaturePath();
                    break;
                default:
                    throw new PlatformNotSupportedException(PLATFORM_NOT_SUPPORTED_ERROR + platform);
            }
            
            KitchenStoreJSON storeSettings = kitchenSettings.GetSettingsFromStoreAndPlatform(store, platform);
            if (storeSettings == null) {
                throw new PlatformNotSupportedException(PLATFORM_NOT_SUPPORTED_ERROR + platform);
            }

            // The MD5s are compared.
            KitchenKeysJSON storeKeys = storeSettings.settings;
            string remoteChecksum = storeKeys.AdjustSignatureLinkV2.checksum;

            // If the remote MD5 is not provided, the checksum is not checked.
            // We consider that all is ok.
            if (string.IsNullOrEmpty(remoteChecksum)) {
                return;
            } else if (!IsChecksumFormatCorrect(remoteChecksum)) {
                throw new BadFormattedSettingValueException(INCORRECT_CHECKSUM_FORMAT_ERROR);
            }
            
            string localChecksum = FileUtility.FileMD5(filePath);
            if (!string.Equals(remoteChecksum.ToLower(), localChecksum.ToLower())) {
                throw new FileChecksumFailedException(MD5_DIFFERENT_ERROR);
            }
        }

        private static bool IsChecksumFormatCorrect(string checksum)
        {
            return Regex.IsMatch(checksum, "^[0-9a-fA-F]{32}$", RegexOptions.Compiled); 
        }
    }
}