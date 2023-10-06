using System;
using System.IO;
using UnityEngine;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Models;

namespace Voodoo.Sauce.Internal.CrossPromo.Mercury
{
    public static class MercuryCache
    {
        private const string TAG = "CrossPromoMercuryCache";
        private const string CACHE_DIRECTORY_NAME = "Mercury";
        private static string _parentDirectory = Application.temporaryCachePath;
        private static string _cachePath = Path.Combine(_parentDirectory, CACHE_DIRECTORY_NAME);

        public static MercuryPromotedAsset GetAsset(string assetId)
        {
            if (string.IsNullOrEmpty(assetId)) return null;

            string assetPath = GetAssetPath(assetId);
            if (File.Exists(assetPath)) {
                string text = File.ReadAllText(assetPath);
                var asset = JsonUtility.FromJson<MercuryPromotedAsset>(text);
                return asset;
            }

            return null;
        }

        public static MercuryWaterfall GetWaterfall(string waterfallId)
        {
            if (string.IsNullOrEmpty(waterfallId)) return null;

            string assetPath = GetAssetPath(waterfallId);
            if (File.Exists(assetPath)) {
                string text = File.ReadAllText(assetPath);
                var waterfall = JsonUtility.FromJson<MercuryWaterfall>(text);
                return waterfall;
            }

            return null;
        }

        public static bool IsCached(string assetId)
        {
            if (string.IsNullOrEmpty(assetId)) return false;

            string assetPath = GetAssetPath(assetId);
            return File.Exists(assetPath);
        }

        public static void SaveAsset(MercuryPromotedAsset asset, byte[] videoBytes)
        {
            if (asset == null) return;
            if (string.IsNullOrEmpty(asset.id)) return;
            if (videoBytes == null) return;

            try {
                string assetPath = GetAssetPath(asset.id);
                CreateDirectoryForFile(assetPath);

                string videoPath = GetVideoPath(asset.id);
                File.WriteAllBytes(videoPath, videoBytes);

                asset.videoUrl = videoPath;

                string json = JsonUtility.ToJson(asset);
                File.WriteAllText(assetPath, json);
            } catch (Exception exception) {
                VoodooLog.LogError(Module.CROSS_PROMO,TAG, exception.Message);
            }
        }

        public static void SaveWaterfall(MercuryWaterfall waterfall)
        {
            if (waterfall == null) return;
            if (string.IsNullOrEmpty(waterfall.id)) return;

            try {
                string assetPath = GetAssetPath(waterfall.id);
                ClearCache();
                CreateDirectoryForFile(assetPath);

                string json = JsonUtility.ToJson(waterfall);
                File.WriteAllText(assetPath, json);
            } catch (Exception exception) {
                VoodooLog.LogError(Module.CROSS_PROMO,TAG, exception.Message);
            }
        }

        private static void CreateDirectoryForFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string directory = fileInfo.DirectoryName;

            if (string.IsNullOrEmpty(directory)) return;
            
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }
        }

        public static void ClearCache()
        {
            if (Directory.Exists(_cachePath)) {
                Directory.Delete(_cachePath, true);
            }
        }

        private static string GetVideoPath(string assetId)
        {
            var fileName = $"{assetId}.mp4";
            return Path.Combine(_cachePath, fileName);
        }

        private static string GetAssetPath(string assetId)
        {
            var fileName = $"{assetId}.json";
            return Path.Combine(_cachePath, fileName);
        }
    }
}