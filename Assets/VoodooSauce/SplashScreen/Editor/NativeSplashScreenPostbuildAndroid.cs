#if UNITY_ANDROID
using System.Collections.Generic;
using System.IO;
using UnityEditor.Android;
using UnityEngine;
using Voodoo.Sauce.Internal.Extension;

namespace Voodoo.Sauce.Internal.SplashScreen
{
    public class NativeSplashScreenPostBuildAndroid : IPostGenerateGradleAndroidProject
    {
        private const string DEFAULT_THEME = "@android:style/Theme.NoTitleBar";
        private const string NATIVE_SPLASHSCREEN_THEME = "@style/NativeSplashScreenTheme";
        private const string COMPILE_SDK_VERSION = "compileSdkVersion";
        private const string NATIVE_SPLASHSCREEN_MAVEN_URL =
            "        maven {\n" +
            "            url \"https://voodoosauce-sdk.s3.eu-west-2.amazonaws.com/native-splashscreen/android\"\n" +
            "        }";

        private const string NATIVE_SPLASHSCREEN_DEPENDENCY = "implementation 'io.voodoo.nativesplashscreen:sdk:1.0'";

        private const string GRADLE_FILE_PATH = "build.gradle";
        private const string MANIFEST_FILE_PATH = "src/main/AndroidManifest.xml";
        private const string GRADLE_PROJECT_REPOSITORIES_SECTION = "def unityProjectPath";
        private const string GRADLE_DEPENDENCIES_SECTION = "implementation fileTree";
        private const int ANDROID_12_API = 31;

        public int callbackOrder => 99;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            //Check files
            string gradleBuildFilePath = Path.Combine(path, GRADLE_FILE_PATH);
            if (!File.Exists(gradleBuildFilePath)) return;
            string manifestFilePath = Path.Combine(path, MANIFEST_FILE_PATH);
            if (!File.Exists(manifestFilePath)) return;
            //Parse compile sdk version from the gradle file
            int compileSdkVersion = ParseCompileSdkVersion(gradleBuildFilePath);
            Debug.Log("OnPostGenerateGradleAndroidProject ParseCompileSdkVersion: " + compileSdkVersion);

            //Add and use native splashscreen lib
            if (compileSdkVersion < ANDROID_12_API) return;
            if (AddSplashScreenDependency(gradleBuildFilePath)) {
                UseSplashScreenLib(manifestFilePath);
            }
        }

        private static bool AddSplashScreenDependency(string gradleBuildFilePath)
        {
            try {
                var projectRepositoriesSectionMatched = false;
                var dependenciesSectionMatched = false;

                var outputLines = new List<string>();
                IEnumerable<string> gradleBuildContents = File.ReadAllLines(gradleBuildFilePath);
                foreach (string line in gradleBuildContents) {
                    outputLines.Add(line);
                    //project Repositories Section
                    if (!projectRepositoriesSectionMatched && line.Contains(GRADLE_PROJECT_REPOSITORIES_SECTION)) {
                        projectRepositoriesSectionMatched = true;
                        outputLines.Add(NATIVE_SPLASHSCREEN_MAVEN_URL);
                    }
                    //dependencies Section
                    if (!dependenciesSectionMatched && line.Contains(GRADLE_DEPENDENCIES_SECTION)) {
                        dependenciesSectionMatched = true;
                        outputLines.Add($"    {NATIVE_SPLASHSCREEN_DEPENDENCY}");
                    }
                }

                if (projectRepositoriesSectionMatched && dependenciesSectionMatched) {
                    File.WriteAllLines(gradleBuildFilePath, outputLines);
                    return true;
                }
            } catch {
                // ignored
            }

            return false;
        }

        private static void UseSplashScreenLib(string manifestFilePath)
        {
            try {
                string contents = File.ReadAllText(manifestFilePath).Replace(DEFAULT_THEME, NATIVE_SPLASHSCREEN_THEME);
                File.WriteAllText(manifestFilePath, contents);
            } catch {
                // ignored
            }
        }

        private static int ParseCompileSdkVersion(string gradleBuildFilePath)
        {
            try {
                string[] lines = File.ReadAllLines(gradleBuildFilePath);
                foreach (string line in lines) {
                    if (line.Contains(COMPILE_SDK_VERSION)) {
                        return int.Parse(line.Remove(COMPILE_SDK_VERSION).Trim());
                    }
                }
            } catch {
                // ignored
            }

            return 0;
        }
    }
}
#endif