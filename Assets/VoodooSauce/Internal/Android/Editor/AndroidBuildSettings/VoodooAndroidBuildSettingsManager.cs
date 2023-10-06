using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class VoodooAndroidBuildSettingsManager
    {
        private const string ANDROID_GRADLE_PLUGIN_GROUP_ARTIFACT = "com.android.tools.build:gradle";
        private static readonly VoodooSettings VoodooSettings = VoodooSettings.Load();

        //For now all are forced to 4.0.1 (for unity 2021) and 7.1.2 (for unity 2022), we didnt take out the logic since we might need it to handle
        //next year's google BuildTargetApi enforcement to 34 and 35
        private static readonly Dictionary<VoodooSauceAndroidBuildTargetEnum, string> AndroidGradlePluginMapping =
            new Dictionary<VoodooSauceAndroidBuildTargetEnum, string> {
#if UNITY_2022_2_OR_NEWER
                { VoodooSauceAndroidBuildTargetEnum.BuildTargetApi33, "7.1.2" }
#else
                { VoodooSauceAndroidBuildTargetEnum.BuildTargetApi33, "4.0.1" }
#endif
            };

        public static VoodooSauceAndroidBuildTargetEnum GetAndroidBuildTarget() =>
            VoodooSettings.AndroidBuildTargetEnum;

        public static string GetAndroidGradlePluginVersion()
        {
            try
            {
                return AndroidGradlePluginMapping[GetAndroidBuildTarget()];
            }
            catch
            {
                VoodooSettings.AndroidBuildTargetEnum = VoodooSauceAndroidBuildTargetEnum.BuildTargetApi33;
                return AndroidGradlePluginMapping[GetAndroidBuildTarget()];
            }
        }

        public static List<string> ValidateAndroidBuild(string path)
        {
            var result = new List<string>();
            result.Add("Gradle version is not higher than the required gradle");
            return result;
        }

        public static void UpdateAllAndroidGradlePluginVersion(string rootSearchPath)
        {
            string designatedVersions =
                AndroidGradlePluginMapping[GetAndroidBuildTarget()];
            try {
                Debug.Log("Scanning Gradle file from root: " + rootSearchPath + " \n with designated version "
                    + designatedVersions);
                //Find all gradle files
                if (rootSearchPath.EndsWith("unityLibrary"))
                    rootSearchPath = rootSearchPath.Replace("unityLibrary", "");
                string[] gradleFiles = Directory.GetFiles(rootSearchPath, "*.gradle",
                    SearchOption.AllDirectories);

                foreach (string gradleFile in gradleFiles) {
                    Debug.Log("Checking gradle file: " + gradleFile);
                    //Read all gradle files
                    string[] gradleLines = File.ReadAllLines(gradleFile);
                    //If it doesnt have any reference to Android Gradle Plugin then skip
                    if (!gradleLines.Any(s => s.Contains(ANDROID_GRADLE_PLUGIN_GROUP_ARTIFACT))) continue;
                    Debug.Log("Modifying gradle file: " + gradleFile);

                    //If it contains any reference to Android Gradle Plugin then, get all the line index that have reference
                    List<int> allIndexWithGradlePlugin = gradleLines.Select((s, i) => new { i, s })
                                                                    .Where(t => t.s.Contains(
                                                                         ANDROID_GRADLE_PLUGIN_GROUP_ARTIFACT))
                                                                    .Select(t => t.i)
                                                                    .ToList();

                    //Go through all the lines
                    var oldVersion = "";
                    foreach (int idx in allIndexWithGradlePlugin) {
                        string stringLine = gradleLines[idx];
                        //Regex to find the version of AGP
                        var regexVersionFinder = new Regex("\\d{1,3}\\.\\d{1,3}(?:\\.\\d{1,6})");
                        oldVersion = regexVersionFinder.Match(stringLine).Value;
                        if (string.IsNullOrEmpty(oldVersion))
                            continue;
                        //Replace it with the desired versions
                        string newStringLine = regexVersionFinder.Replace(stringLine, designatedVersions);
                        //Set it back to the string arrays
                        gradleLines.SetValue(newStringLine, idx);
                    }

                    //Write back to gradle file
                    File.WriteAllLines(gradleFile, gradleLines);
                    Debug.Log("Android Gradle Plugin in file " + gradleFile + " has been updated from version "
                        + oldVersion
                        + " to version " + designatedVersions);
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public static void ConfigureCustomGradleVersionFromEnvVariable()
        {
            Version currentGradleVersion = GradleCustomConfigHelper.GetCurrentGradleVersion();
            Version recommendedGradleVersion = GradleCustomConfigHelper.GetRecommendedGradleVersion();
            if (currentGradleVersion == null) {
                Debug.LogError("Cannot retrieve current gradle version");
                return;
            }

            Debug.Log("Current Gradle Tools version: " + currentGradleVersion);
            if (currentGradleVersion.CompareTo(recommendedGradleVersion) == -1) {
                string gradlePathEnvVar = Environment.GetEnvironmentVariable("VS_CUSTOM_GRADLE_TOOLS_PATH");
                if (gradlePathEnvVar == null) {
                    Debug.LogError("Custom Gradle environment variable is not configured"
                        + " in VS_CUSTOM_GRADLE_TOOLS_PATH");
                    return;
                }

                Version newGradleVersion = recommendedGradleVersion;
                if (newGradleVersion == null) {
                    Debug.LogError("Cannot retrieve gradle version from the configured VS_CUSTOM_GRADLE_TOOLS_PATH"
                        + ". Please double check whether configured path is a correct gradle path");
                    return;
                }

                if (newGradleVersion.CompareTo(recommendedGradleVersion) == -1) {
                    Debug.LogError("New configured gradle version is lower than"
                        + $" {GradleCustomConfigHelper.GetRecommendedGradleVersion()}. Please configure gradle with recommended version.");
                    return;
                }

                GradleCustomConfigHelper.SetAndroidCustomGradlePath(gradlePathEnvVar);
            } else {
                Debug.Log(
                    $"Current Gradle Version is equal or higher than {GradleCustomConfigHelper.GetRecommendedGradleVersion()}."
                    + "No need to do anything");
            }
        }
    }
}