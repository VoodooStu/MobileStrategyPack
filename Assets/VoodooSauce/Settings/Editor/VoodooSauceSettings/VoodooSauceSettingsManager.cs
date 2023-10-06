using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    public static class VoodooSauceSettingsManager
    {
        private const string SYNCHRONIZATION_PROGRESS_BAR_TITLE = "Synchronize VoodooSauce Settings";

        /// <summary>
        /// Get all variables from Kitchen and update the VoodooSettings.
        /// </summary>
        public static IEnumerator RefreshVoodooSauceSettings(bool showDialogErrors = true, Action<bool, string> onCompleteCallback = null)
        {
            if(!Application.isBatchMode) 
                UnityEditor.EditorUtility.DisplayProgressBar(SYNCHRONIZATION_PROGRESS_BAR_TITLE, "", 0.4f);
            
            // Get the current cached settings, these settings' values will be compared to the new ones.
            KitchenSettingsJSON oldKitchenSettings = KitchenSettingsJSON.Load();
            
            return KitchenAPI.DownloadSettings(delegate(KitchenSettingsJSON settings, bool refreshed, string error) {
                if (!Application.isBatchMode) {
                    UnityEditor.EditorUtility.DisplayProgressBar(SYNCHRONIZATION_PROGRESS_BAR_TITLE, "", 1f);
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
                
                if (!string.IsNullOrEmpty(error)) {
                    if (showDialogErrors && !Application.isBatchMode) {
                        UnityEditor.EditorUtility.DisplayDialog("Unable to Get VoodooSauce Settings", error, "ok");
                    }
                    onCompleteCallback?.Invoke(false, error);
                    return;
                }

                // The kitchen settings are downloaded and cached, now the VS Settings are updated from these kitchen settings.
                try {
                    VoodooSauceSettingsHelper.ReloadSettings();
                } catch (Exception ex) {
                    if (showDialogErrors && !Application.isBatchMode) {
                        UnityEditor.EditorUtility.DisplayDialog("Unable to Get VoodooSauce Settings", ex.Message, "ok");
                    }

                    Debug.LogError($"Unable to Get VoodooSauce Settings: {ex.Message}");
                    VoodooSauce.LogException(ex);
                    onCompleteCallback?.Invoke(false, ex.Message);
                    return;
                }
                
                // Compare the old and the new values from Kitchen.
                List<KeyValuePair<string, KeyValuePair<string, string>>> changedValuesList = CompareKitchenSettings(oldKitchenSettings, settings);
                if (changedValuesList.Count > 0 && !Application.isBatchMode)
                {
                    VoodooSettingsChangedWindow.ShowWindow(settings.LastUpdate.ToString(CultureInfo.CurrentCulture), changedValuesList);
                }
                onCompleteCallback?.Invoke(true, "");
            });
        }

        /// <summary>
        /// Get all variables from Kitchen and update the VoodooSettings.
        /// </summary>
        private static List<KeyValuePair<string, KeyValuePair<string, string>>> CompareKitchenSettings(KitchenSettingsJSON oldSettings, KitchenSettingsJSON newSettings)
        {
            var changedValuesList = new List<KeyValuePair<string, KeyValuePair<string, string>>>();
            
            // The KitchenKeysJSON values will be compared, so the KitchenKeysJSON type is created.
            Type kitchenKeysType = typeof(KitchenKeysJSON);
            FieldInfo[] kitchenKeysFields = kitchenKeysType.GetFields();
            
            // The KitchenKeysJSON values from every store are compared.
            foreach (KitchenStoreJSON newStore in newSettings.StoreSettings) {
                string platform = newStore.platform;
                string store = newStore.store;
                
                KitchenStoreJSON oldStore = oldSettings.GetSettingsFromStoreAndPlatform(store, platform) ?? new KitchenStoreJSON();
                KitchenKeysJSON oldKeyValues = oldStore.settings;
                KitchenKeysJSON newKeyValues = newStore.settings;
                
                foreach (FieldInfo fieldInfo in kitchenKeysFields) {
                    string fieldName = fieldInfo.Name;

                    var oldValue = (KitchenValueJSON)fieldInfo.GetValue(oldKeyValues);
                    var newValue = (KitchenValueJSON)fieldInfo.GetValue(newKeyValues);

                    // Two KitchenValueJSON objects are considered as equal if they have the same version and the same value,
                    // because the first version of a value is 0 not 1.
                    // The values are compared too because the downloadable files can have the same value but not the same version number.
                    if (oldValue.value == newValue.value && oldValue.version == newValue.version) {
                        continue;
                    }
                    
                    Debug.Log($"--- name: {fieldName}---");
                    Debug.Log($"- old - value: {oldValue.value}, version: {oldValue.version}---");
                    Debug.Log($"- new - value: {newValue.value}, version: {newValue.version}---");
                    
                    changedValuesList.Add(new KeyValuePair<string, KeyValuePair<string, string>>(
                        $"[{platform} {store}] {fieldName}",
                        new KeyValuePair<string, string>(oldValue.value + " (version " + oldValue.version + ")", newValue.value + " (version " + newValue.version + ")")
                    ));
                }
            }

            return changedValuesList;
        }
    }
}