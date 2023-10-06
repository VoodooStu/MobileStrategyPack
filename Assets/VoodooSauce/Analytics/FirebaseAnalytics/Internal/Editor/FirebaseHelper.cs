using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Internal.Editor;

namespace Voodoo.Sauce.Internal.Analytics
{
    public static class FirebaseHelper
    {
        private const string CONFIGS_FOLDER = "Assets/Resources/Firebase/";
        private const string IOS_CONFIG_FILE = CONFIGS_FOLDER + "GoogleService-Info.plist";
        private const string ANDROID_CONFIG_FILE = CONFIGS_FOLDER + "google-services.json";
        private const string ANDROID_GENERATED_CONFIG_FILE = "Assets/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml";

        // Copy the cached iOS file to the live location. 
        public static bool CopyIOSFile(string sourcePath) => CopyFile(sourcePath, IOS_CONFIG_FILE, BuildTarget.iOS);
        
        // Copy the cached Android file to the live location. 
        public static bool CopyAndroidFile(string sourcePath) => CopyFile(sourcePath, ANDROID_CONFIG_FILE, BuildTarget.Android);

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
                    files.Add(IOS_CONFIG_FILE);
                    break;
                case BuildTarget.Android:
                    files.Add(ANDROID_CONFIG_FILE);
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
            AssetDatabase.Refresh();

            return true;
        }
#endregion

#region Public methods
        public static string GetAndroidConfigFilePath() => ANDROID_CONFIG_FILE;

        public static string GetAndroidGeneratedFilePath() => ANDROID_GENERATED_CONFIG_FILE;

        public static string GetIOSConfigFilePath() => IOS_CONFIG_FILE;

        public static bool IOSConfigFileExists() => File.Exists(IOS_CONFIG_FILE);

        public static bool AndroidConfigFileExists() => File.Exists(ANDROID_CONFIG_FILE);

        public static bool AndroidGeneratedConfigFileExists() => File.Exists(ANDROID_GENERATED_CONFIG_FILE);
#endregion
    }
}