using System;
using System.IO;
using UnityEngine;
using Voodoo.Sauce.Internal.CrossPromo.Models;

namespace Voodoo.Sauce.Internal.CrossPromo.Utils
{
    /// <summary>
    /// Manage the players prefs
    /// </summary>
    internal static class PlayerPrefsUtils
    {
        /// <summary>
        /// Get the key of the asset according to the file path
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>The key</returns>
        public static string GetKey(string filePath)
        {
            return CacheManager.GetCurrentDirectory(filePath) + Path.GetFileName(filePath);
        }

        public static AssetModel GetAsset(string key)
        {
            try {
                return JsonUtility.FromJson<AssetModel>(PlayerPrefs.GetString(key));
            } catch (Exception) {
                return null;
            }
        }
    }
}