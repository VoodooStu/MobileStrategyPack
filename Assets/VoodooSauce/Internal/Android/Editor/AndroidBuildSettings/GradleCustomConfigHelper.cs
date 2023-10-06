using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
#if UNITY_ANDROID
using UnityEditor;
using UnityEditor.Android;
#endif

namespace Voodoo.Sauce.Internal.Editor
{
    public static class GradleCustomConfigHelper
    {
        private const string GRADLE_VERSION_REGEX = "launcher-\\d{1,3}\\.\\d{1,3}(?:\\.\\d{1,6})|launcher-\\d{1,3}\\.\\d{1,3}";
        private static readonly Dictionary<string, string> AgpToGradleToolsVersionMapping =
            new Dictionary<string, string> {
                { "3.4.3", "5.1.1" },
                { "4.0.1", "6.1.1" },
                { "7.1.2", "7.2" }
            };

        public static Version GetRecommendedGradleVersion()
        {
            string agpVersion = VoodooAndroidBuildSettingsManager.GetAndroidGradlePluginVersion();
            string version = AgpToGradleToolsVersionMapping[agpVersion];
            return Version.Parse(version);
        }

        /**
         * GetGradleVersion
         * Used to get configured gradle tools version by checking the gradle jar file name of the gradle
         * launcher. By default gradle tools will always put the version in the gradle-launcher.jar file
         */
        [CanBeNull]
        public static Version GetGradleVersion(string path)
        {
            Debug.Log("Reading gradle version from " + path);
            string[] gradleJarFiles = Directory.GetFiles(Path.Combine(path, "lib"), "gradle-launcher-*.jar",
                SearchOption.TopDirectoryOnly);
            if (gradleJarFiles.Length == 0) return null;
            string gradleLauncherJarFileName = gradleJarFiles[0];
            Debug.Log("gradleLauncherJarFileName " + gradleLauncherJarFileName);
            string version = Regex.Match(gradleLauncherJarFileName, GRADLE_VERSION_REGEX).Value
                                  .Replace("launcher-","");
            return Version.Parse(version);
        }

        [CanBeNull]
        public static Version GetCurrentGradleVersion()
        {
#if UNITY_ANDROID
            return GetGradleVersion(AndroidExternalToolsSettings.gradlePath);
#else
            return null;
#endif
        }

        public static void SetAndroidCustomGradlePath(string path)
        {
#if UNITY_ANDROID
            AndroidExternalToolsSettings.gradlePath = path;
            Debug.Log("Custom Gradle path has been updated to use: " + path);
#endif
        }
    }
}