#if UNITY_IOS || UNITY_TVOS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.IdfaAuthorization
{
    public class NativeWrapperPostBuild
    {
        private const string InfoPlistStringsFileName = "InfoPlist.strings"; 
        private const string UserTrackingUsageDescriptionKey = "NSUserTrackingUsageDescription";
        private const string LocalizationsDirectory = "VoodooSauceATTLocalizations"; 

        [PostProcessBuild(1000)]
        public static void PostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) 
                return;

            var plistPath = Path.Combine(buildPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElement element = plist.root[UserTrackingUsageDescriptionKey];
            var usage = IdfaAuthorizationConfig.Get<IdfaAuthorizationPopupLocalisation>(SystemLanguage.English)?.body;
            if (element == null && !string.IsNullOrEmpty(usage))
            {
                plist.root.SetString(UserTrackingUsageDescriptionKey, usage);
                plist.WriteToFile(plistPath);
            }

            CreateInfoPlistLocalizations(buildPath);
        }

        private static void CreateInfoPlistLocalizations(string buildPath)
        {
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string targetGuid = project.GetUnityMainTargetGuid();

            // if the localizations directory doesn't exist -> create it
            var localizationsDirectoryPath = Path.Combine(buildPath, LocalizationsDirectory);
            if (!Directory.Exists(localizationsDirectoryPath)) {
                Directory.CreateDirectory(localizationsDirectoryPath);
            }
            
            var languages = IdfaAuthorizationConfig.GetLanguages<IdfaAuthorizationPopupLocalisation>();
            foreach (var language in languages)
            {
                //fetch the language
                var localizedBody = IdfaAuthorizationConfig.Get<IdfaAuthorizationPopupLocalisation>(language)?.body;
                if (string.IsNullOrEmpty(localizedBody)) {
                    localizedBody = IdfaAuthorizationConfig.Get<IdfaAuthorizationPopupLocalisation>(SystemLanguage.English)?.body;
                }

                if (string.IsNullOrEmpty(localizedBody)) {
                    continue;
                }
                
                // fetch the iso language code
                var isoLanguage = LanguageUtils.Get2LetterISOCodeFromSystemLanguage(language);

                // if the language directory doesn't exist -> create it
                var languageDirectoryName = $"{isoLanguage}.lproj";
                var languageDirectoryPath = Path.Combine(buildPath, LocalizationsDirectory, languageDirectoryName);
                if (!Directory.Exists(languageDirectoryPath)) {
                    Directory.CreateDirectory(languageDirectoryPath);
                }
                
                // if the language plist resources file doesn't exist -> create it
                var languagePlistFilePath = Path.Combine(languageDirectoryPath, InfoPlistStringsFileName);
                if (!File.Exists(languagePlistFilePath)) {
                    File.Create(languagePlistFilePath).Dispose();

                    // add the language folder
                    var languageDirectoryRelativePath = Path.Combine(LocalizationsDirectory, languageDirectoryName);
                    var guid = project.AddFolderReference(languageDirectoryRelativePath, languageDirectoryRelativePath);
                    project.AddFileToBuild(targetGuid, guid);
                }

                // add the localized text to the strings file
                var localizedContent = File.ReadAllText(languagePlistFilePath);
                localizedContent = localizedContent + $"{Environment.NewLine}\"{UserTrackingUsageDescriptionKey}\"=\"{localizedBody}\";";
                File.WriteAllText(languagePlistFilePath, localizedContent);
            }
            
            project.WriteToFile(projectPath);
        }
    }
}
#endif