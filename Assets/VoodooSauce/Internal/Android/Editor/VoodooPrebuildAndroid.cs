#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using GooglePlayServices;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    public class VoodooPrebuildAndroid : IPreprocessBuildWithReport
    {
        private const string SDK_FOLDER_PATH = "VoodooSauce/";
        private const string SOURCE_FOLDER_PATH = "VoodooSauce/Internal/Android/Editor";
        private const string PLUGIN_FOLDER_PATH = "Plugins";
        private const string ANDROID_FOLDER_PATH = "Plugins/Android";
        private const string ANDROID_TEMPLATE_FILE_NAME = "AndroidTemplate.json";

        private const string ANDROID_MANIFEST_FILE_NAME = "AndroidManifest.xml";
        private static readonly string SourceAndroidManifestPath = $"{SOURCE_FOLDER_PATH}/{ANDROID_MANIFEST_FILE_NAME}";
        private static readonly string DestAndroidManifestPath = $"{ANDROID_FOLDER_PATH}/{ANDROID_MANIFEST_FILE_NAME}";
        
        private const string LAUNCHER_MANIFEST_FILE_NAME = "LauncherManifest.xml";
        private static readonly string SourceLauncherManifestPath = $"{SOURCE_FOLDER_PATH}/{LAUNCHER_MANIFEST_FILE_NAME}";
        private static readonly string DestLauncherManifestPath = $"{ANDROID_FOLDER_PATH}/{LAUNCHER_MANIFEST_FILE_NAME}";

        private const string MAIN_TEMPLATE_GRADLE_FILE_NAME = "mainTemplate.gradle";
        private static readonly string SourceMainTemplateGradlePath = $"{SOURCE_FOLDER_PATH}/{MAIN_TEMPLATE_GRADLE_FILE_NAME}";
        private static readonly string DestMainTemplateGradlePath = $"{ANDROID_FOLDER_PATH}/{MAIN_TEMPLATE_GRADLE_FILE_NAME}";
        
        private const string LAUNCHER_TEMPLATE_GRADLE_FILE_NAME = "launcherTemplate.gradle";
        private static readonly string SourceLauncherTemplateGradlePath = $"{SOURCE_FOLDER_PATH}/{LAUNCHER_TEMPLATE_GRADLE_FILE_NAME}";
        private static readonly string DestLauncherTemplateGradlePath = $"{ANDROID_FOLDER_PATH}/{LAUNCHER_TEMPLATE_GRADLE_FILE_NAME}";
        
        private const string BASE_PROJECT_TEMPLATE_GRADLE_FILE_NAME = "baseProjectTemplate.gradle";
        private static readonly string SourceBaseProjectTemplateGradlePath = $"{SOURCE_FOLDER_PATH}/{BASE_PROJECT_TEMPLATE_GRADLE_FILE_NAME}";
        private static readonly string DestBaseProjectTemplateGradlePath = $"{ANDROID_FOLDER_PATH}/{BASE_PROJECT_TEMPLATE_GRADLE_FILE_NAME}";
        
        private const string GRADLE_TEMPLATE_PROPERTIES_FILE_NAME = "gradleTemplate.properties";
        private static readonly string SourceGradleTemplatePropertiesPath = $"{SOURCE_FOLDER_PATH}/{GRADLE_TEMPLATE_PROPERTIES_FILE_NAME}";
        private static readonly string DestGradleTemplatePropertiesPath = $"{ANDROID_FOLDER_PATH}/{GRADLE_TEMPLATE_PROPERTIES_FILE_NAME}";
        
        public int callbackOrder => -1;
        
        private static readonly AndroidTemplateProperties TemplateProperties = new AndroidTemplateProperties();

        public void OnPreprocessBuild(BuildReport report) 
        {
            GetTemplatePropertiesToAdd();
            CreateAndroidFolder();
            UpdateAndroidManifest();
            UpdateLauncherManifest();
            UpdateBaseProjectTemplateGradle();
            UpdateMainTemplateGradle();
            UpdateLauncherTemplateGradle();
            UpdateGradleTemplateProperties();
            PreparePlayerSettings();
            PrepareResolver();
            VoodooAndroidBuildSettingsManager.UpdateAllAndroidGradlePluginVersion(
                Path.Combine(Application.dataPath, ANDROID_FOLDER_PATH));

            // If batch mode then set CustomGradleVersion with Env Variable
            if (Application.isBatchMode)
                VoodooAndroidBuildSettingsManager.ConfigureCustomGradleVersionFromEnvVariable();
        }

        private static string GetAndroidGradlePluginString()
        {
            string agpVersion = VoodooAndroidBuildSettingsManager.GetAndroidGradlePluginVersion();
            return $"classpath 'com.android.tools.build:gradle:{agpVersion}'";
        }

        private static string GetTargetApiString()
        {
            //To uncomment when we will need to support API 34/35
            // VoodooSauceAndroidBuildTargetEnum buildTarget = VoodooAndroidBuildSettingsManager.GetAndroidBuildTarget();
            // return buildTarget == VoodooSauceAndroidBuildTargetEnum.BuildTargetApi33 ? "33" : "31";
            return "33";
        }

        private static void PreparePlayerSettings()
        {
            // Set Android ARM64/ARMv7 Architecture   
            PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup,
                ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
#if UNITY_2021_1_OR_NEWER
            // Set Android min version
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel22) {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
            }
#else
            // Set Android min version
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel21) {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
            }
#endif
        }

        private static void PrepareResolver()
        {
            // Force playServices Resolver
            PlayServicesResolver.Resolve(null, true);
        }

        private static void CreateAndroidFolder()
        {
            string pluginPath = Path.Combine(Application.dataPath, PLUGIN_FOLDER_PATH);
            string androidPath = Path.Combine(Application.dataPath, ANDROID_FOLDER_PATH);
            if (!Directory.Exists(pluginPath))
                Directory.CreateDirectory(pluginPath);
            if (!Directory.Exists(androidPath))
                Directory.CreateDirectory(androidPath);
        }

        private static void UpdateAndroidManifest()
        {
            string sourcePath = Path.Combine(Application.dataPath, SourceAndroidManifestPath);
            string content = File.ReadAllText(sourcePath)
                                 .Replace("attribute='**APPLICATION_ATTRIBUTES**'", string.Empty)
                                 .Replace("**APPLICATION_ATTRIBUTES_REPLACE**", string.Empty);
            string destPath = Path.Combine(Application.dataPath, DestAndroidManifestPath);
            File.Delete(destPath);
            File.WriteAllText(destPath, content);
        }

        private static void UpdateLauncherManifest()
        {
            string sourcePath = Path.Combine(Application.dataPath, SourceLauncherManifestPath);
            string destPath = Path.Combine(Application.dataPath, DestLauncherManifestPath);
            File.Copy(sourcePath, destPath, true);
        }

        private static void UpdateLauncherTemplateGradle()
        {
            string sourcePath = Path.Combine(Application.dataPath, SourceLauncherTemplateGradlePath);
            string content = File.ReadAllText(sourcePath)
#if !UNITY_2022_2_OR_NEWER
                                 .Replace("ndkPath \"**NDKPATH**\"","")
#endif
                                 .Replace("**VSTARGETSDKVERSION**", GetTargetApiString())
#if !UNITY_2021_1_OR_NEWER 
                                  //Only replace if building in Unity 2020
                                 .Replace("**BUILTIN_NOCOMPRESS**", "['.unity3d', '.ress', '.resource', '.obb']")
#endif
                                 .Replace("**BUILD_SCRIPT_DEPS**", GetAndroidGradlePluginString());
            
            foreach (string plugin in TemplateProperties.launcherTemplate.applyPlugin) {
                var applyPlugin = $"apply plugin: '{plugin}'";
                if (content.Contains(applyPlugin)) {
                    continue;
                }
                content = content.Replace("**APPLY_PLUGIN**", $"{applyPlugin}\n**APPLY_PLUGIN**");
                Debug.Log($"[VoodooSauce] Apply to {LAUNCHER_TEMPLATE_GRADLE_FILE_NAME}: '{applyPlugin}'");
            }
            content = content.Replace("**APPLY_PLUGIN**", "");
            
            foreach (string configuration in TemplateProperties.launcherTemplate.configurations) {
                if (content.Contains(configuration)) {
                    continue;
                }
                content = content.Replace("**CONFIGURATIONS**", $"{configuration}\n**CONFIGURATIONS**");
                Debug.Log($"[VoodooSauce] Apply to {LAUNCHER_TEMPLATE_GRADLE_FILE_NAME}: '{configuration}'");
            }
            content = content.Replace("**CONFIGURATIONS**", "");

            var packagingOptionsContents = "";
            foreach (string packagingOption in TemplateProperties.launcherTemplate.packagingOptions) {
                if (content.Contains(packagingOption)) {
                    continue;
                }
                packagingOptionsContents = $"\t\t{packagingOption}\n{packagingOptionsContents}";
                Debug.Log($"[VoodooSauce] Apply to {LAUNCHER_TEMPLATE_GRADLE_FILE_NAME} (packagingOptions): '{packagingOption}'");
            }
            if (!string.IsNullOrEmpty(packagingOptionsContents)) {
                content = content.Replace("**PACKAGING_OPTIONS**", "\n\n\tpackagingOptions {\n" + packagingOptionsContents + "\t}**PACKAGING_OPTIONS**");
            }
            
            string destPath = Path.Combine(Application.dataPath, DestLauncherTemplateGradlePath);
            File.Delete(destPath);
            File.WriteAllText(destPath, content);
        }
        
        private static void UpdateBaseProjectTemplateGradle()
        {
            string sourcePath = Path.Combine(Application.dataPath, SourceBaseProjectTemplateGradlePath);
            string content = File.ReadAllText(sourcePath);
            
            foreach (string dependency in TemplateProperties.baseProjectTemplate.dependencies) {
                if (content.Contains(dependency)) {
                    continue;
                }
                content = content.Replace("**BUILD_SCRIPT_DEPS**", $"{dependency}\n\t\t\t**BUILD_SCRIPT_DEPS**");
                Debug.Log($"[VoodooSauce] Add dependency to {BASE_PROJECT_TEMPLATE_GRADLE_FILE_NAME}: '{dependency}'");
            }
            
            foreach (string repository in TemplateProperties.baseProjectTemplate.repositories) {
                string fullRepositoryName = repository.EndsWith("()") ? repository : $"{repository}()";
                if (content.Contains(fullRepositoryName)) {
                    continue;
                }
                content = content.Replace("**ARTIFACTORYREPOSITORY**", $"**ARTIFACTORYREPOSITORY**\n\t\t\t{fullRepositoryName}");
                Debug.Log($"[VoodooSauce] Add repository to {BASE_PROJECT_TEMPLATE_GRADLE_FILE_NAME}: '{repository}'");
            }

            string destPath = Path.Combine(Application.dataPath, DestBaseProjectTemplateGradlePath);
            File.Delete(destPath);
            File.WriteAllText(destPath, content);
        }

        private static void UpdateMainTemplateGradle()
        {
            string sourcePath = Path.Combine(Application.dataPath, SourceMainTemplateGradlePath);
            string content = File.ReadAllText(sourcePath);
            content = content.Replace("**VSTARGETSDKVERSION**", GetTargetApiString())
                             .Replace("**BUILD_SCRIPT_DEPS**", GetAndroidGradlePluginString())
                             .Replace("**APPLY_PLUGINS**", "apply plugin: 'com.android.library'")
#if !UNITY_2022_2_OR_NEWER
                             .Replace("ndkPath \"**NDKPATH**\"","")
#endif
#if !UNITY_2021_1_OR_NEWER 
                              //Only replace if building in Unity 2020 
                             .Replace("**BUILTIN_NOCOMPRESS**", "['.ress', '.resource', '.obb']")
#endif
                             .Replace("**APPLICATIONID**", string.Empty);

            string destPath = Path.Combine(Application.dataPath, DestMainTemplateGradlePath);
            File.Delete(destPath);
            File.WriteAllText(destPath, content);
        }

        private static void UpdateGradleTemplateProperties()
        {
            string sourcePath = Path.Combine(Application.dataPath, SourceGradleTemplatePropertiesPath);
            string content = File.ReadAllText(sourcePath);
            
#if UNITY_2021_1_OR_NEWER
            content = content.Replace(".unity3d**STREAMING_ASSETS**", "**STREAMING_ASSETS**");
#endif
            
#if UNITY_2022_2_OR_NEWER
            content = content.Replace("android.enableR8=**MINIFY_WITH_R_EIGHT**", "");
#endif

            foreach (string property in TemplateProperties.gradleTemplate.properties) {
                if (content.Contains(property)) {
                    continue;
                }
                content = $"{content}\n{property}";
                Debug.Log($"[VoodooSauce] Apply to {GRADLE_TEMPLATE_PROPERTIES_FILE_NAME}: '{property}'");
            }
            
            string destPath = Path.Combine(Application.dataPath, DestGradleTemplatePropertiesPath);
            File.Delete(destPath);
            File.WriteAllText(destPath, content);
        }

        private static void GetTemplatePropertiesToAdd()
        {
            // Get all the JSON files containing the template properties to add.
            string rootSearchPath = Path.Combine(Application.dataPath, SDK_FOLDER_PATH);
            string[] templateFiles = Directory.GetFiles(rootSearchPath, ANDROID_TEMPLATE_FILE_NAME, SearchOption.AllDirectories);

            // Gather all the properties.
            foreach (string templateFile in templateFiles) {
                try {
                    Debug.Log($"[VoodooSauce] Checking Android template file to apply: '{templateFile}'");
                    string content = File.ReadAllText(templateFile);
                    var templateProperties = JsonConvert.DeserializeObject<AndroidTemplateProperties>(content);
                    TemplateProperties.Merge(templateProperties);
                } catch (Exception e) {
                    Debug.LogError($"[VoodooSauce] Cannot apply the Android template file '{templateFile}': {e.Message}");
                }
            }
        }
    }

    public class AndroidTemplateProperties
    {
        public readonly AndroidLauncherTemplateGradleProperties launcherTemplate = new AndroidLauncherTemplateGradleProperties();
        public readonly AndroidBaseProjectTemplateGradleProperties baseProjectTemplate = new AndroidBaseProjectTemplateGradleProperties();
        public readonly AndroidGradleTemplatePropertiesProperties gradleTemplate = new AndroidGradleTemplatePropertiesProperties();

        public void Merge(AndroidTemplateProperties other)
        {
            launcherTemplate.Merge(other.launcherTemplate);
            baseProjectTemplate.Merge(other.baseProjectTemplate);
            gradleTemplate.Merge(other.gradleTemplate);
        }
    }

    public class AndroidLauncherTemplateGradleProperties
    {
        public readonly List<string> applyPlugin = new List<string>();
        public readonly List<string> configurations = new List<string>();
        public readonly List<string> packagingOptions = new List<string>();

        public void Merge(AndroidLauncherTemplateGradleProperties other)
        {
            applyPlugin.AddRange(other.applyPlugin);
            configurations.AddRange(other.configurations);
            packagingOptions.AddRange(other.packagingOptions);
        }
    }
    
    public class AndroidBaseProjectTemplateGradleProperties
    {
        public readonly List<string> repositories = new List<string>();
        public readonly List<string> dependencies = new List<string>();

        public void Merge(AndroidBaseProjectTemplateGradleProperties other)
        {
            repositories.AddRange(other.repositories);
            dependencies.AddRange(other.dependencies);
        }
    }
    
    public class AndroidGradleTemplatePropertiesProperties
    {
        public readonly List<string> properties = new List<string>();

        public void Merge(AndroidGradleTemplatePropertiesProperties other)
        {
            properties.AddRange(other.properties);
        }
    }
}
#endif