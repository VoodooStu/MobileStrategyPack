using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Internal.Editor;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public static class AdjustHelper
    {
        public const string SIGNATURE_FOLDER = "/Plugins/AdjustSignature";
        public const string IOS_FILE_NAME = "AdjustSigSdk.a";
        public const string ANDROID_FILE_NAME = "AdjustSigSdk.aar";
        
        private static readonly string IOSSignatureFolder = $"Assets{SIGNATURE_FOLDER}/iOS/";
        private static readonly string AndroidSignatureFolder = $"Assets{SIGNATURE_FOLDER}/Android/";

        // Old signature files 
        private static readonly string IOSSignatureFileV1 = IOSSignatureFolder + "adjust-ios.a";
        private static readonly string AndroidSignatureFileV1 = AndroidSignatureFolder + "adjust-android.aar";

        // New signature files
        private static readonly string IOSSignatureFileV2 = IOSSignatureFolder + IOS_FILE_NAME;
        private static readonly string AndroidSignatureFileV2 = AndroidSignatureFolder + ANDROID_FILE_NAME;

        // Copy the cached iOS file to the live location. 
        public static bool CopyIOSFile(string sourcePath) => CopyFile(sourcePath, IOSSignatureFileV2, BuildTarget.iOS);
        
        // Copy the cached Android file to the live location. 
        public static bool CopyAndroidFile(string sourcePath) => CopyFile(sourcePath, AndroidSignatureFileV2, BuildTarget.Android);

        // Remove all the iOS files.
        public static void RemoveIOSFiles() => RemoveFiles(BuildTarget.iOS);

        // Remove all the Android files.
        public static void RemoveAndroidFiles() => RemoveFiles(BuildTarget.Android);

#region Private methods
        private static void RemoveFiles(BuildTarget target)
        {
            var files = new List<string>();

            switch (target) {
                case BuildTarget.iOS:
                    files.Add(IOSSignatureFileV1);
                    files.Add(IOSSignatureFileV2);
                    break;
                case BuildTarget.Android:
                    files.Add(AndroidSignatureFileV1);
                    files.Add(AndroidSignatureFileV2);
                    break;
            }

            FileUtility.DeleteFiles(files);
        }
        
        private static bool CopyFile(string sourcePath, string destinationPath, BuildTarget target)
        {
            if (!File.Exists(sourcePath)) {
                Debug.Log($"File does not exist at '{sourcePath}'");
                return false;
            }
            
            RemoveFiles(target);
            FileUtility.CopyFile(sourcePath, destinationPath);
            
            var pluginImporter = AssetImporter.GetAtPath(destinationPath) as PluginImporter;
            if (pluginImporter != null) {
                pluginImporter.SetCompatibleWithAnyPlatform(false);
                pluginImporter.SetCompatibleWithPlatform(target, true);
                pluginImporter.SaveAndReimport();
            }
            AssetDatabase.Refresh();

            return true;
        }
#endregion
        
#region Public methods
        public static string GetAndroidSignaturePath() => AndroidSignatureFileV2;

        public static string GetIOSSignaturePath() => IOSSignatureFileV2;

        public static bool AndroidSignatureExists()
        {
            try {
                return Directory.EnumerateFiles(AndroidSignatureFolder, "*djust*.aar").Any();
            } catch (DirectoryNotFoundException) {
                return false;
            }
        }

        public static bool IOSSignatureExists()
        {
            try {
                return Directory.EnumerateFiles(IOSSignatureFolder, "*djust*.a").Any();
            } catch (DirectoryNotFoundException) {
                return false;
            }
        }
#endregion
    }
}