using System;
using System.IO;
using UnityEngine;
using Voodoo.Sauce.Internal.Editor;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    public static class CachedSettingsHelper
    {
        // The store settings and the downloadable files (firebase, adjust, etc.) are cached in this directory.
        private static string SaveDirectory
        {
            get
            {
                var dir = new DirectoryInfo(Application.dataPath);
                return Path.Combine(dir.Parent.FullName, "StoreSettings");
            }
        }

        // When the settings and the downloadable files are refreshed, they are cached first in this directory before being moved to the final location.
        private static readonly string TempDirectory = Path.Combine(SaveDirectory, "_temp");
        
        // The store settings are saved to this filename.
        private const string FILENAME = "settings.json";
        
        // Save the store settings into the TEMP directory.
        internal static bool SaveTempSettings(string data)
        {
            var areSettingsSaved = false;
            
            try
            {
                string path = Path.Combine(TempDirectory, FILENAME);
                FileUtility.WriteFile(data, path);
                
                areSettingsSaved = true;
                Debug.Log($"Kitchen settings saved to {path}");
            }
            catch (Exception ex)
            {
                areSettingsSaved = false;
                Debug.LogError($"Can not save the Kitchen settings: {ex.Message}");
                VoodooSauce.LogException(ex);
            }
            
            return areSettingsSaved;
        }

        // Move the settings and the files from the temporary directory to the final location.
        internal static bool SaveSettingsAndFiles()
        {
            var areSettingsSaved = false;
            
            try
            {
                // Move the settings file.
                string settingsPath = GetSettingsPath();
                string sourcePath = Path.Combine(TempDirectory, FILENAME);
                FileUtility.MoveFile(sourcePath, settingsPath);
                Debug.Log($"Kitchen settings saved to {settingsPath}");
            
                // Move the directories and the other files.
                FileUtility.MoveDirectories(TempDirectory, SaveDirectory);

                areSettingsSaved = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Can not move the Kitchen settings files: {ex.Message}");
                VoodooSauce.LogException(ex);
            }
            
            // Clean all the temporary files.
            FileUtility.DeleteDirectory(TempDirectory);
            
            return areSettingsSaved;
        }

        // Returns the path of the cached store settings.
        internal static string GetSettingsPath() => Path.Combine(SaveDirectory, FILENAME);

        // Returns the path of the cached store files.
        public static string GetStoreFilesDirectory(string storeId) => Path.Combine(SaveDirectory, storeId);
        
        // Returns the path of the temporary store files.
        internal static string GetTempStoreFilesDirectory(string storeId) => Path.Combine(TempDirectory, storeId);
    }
}