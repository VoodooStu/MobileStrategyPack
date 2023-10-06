using System.Text.RegularExpressions;

#if UNITY_IOS

using System;
using System.IO;
using UnityEditor.iOS.Xcode;

#endif

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Utils
{
    public static class XcodeUtils
    {
        private const string SKADNETWORK_PATTERN = "^[a-z0-9]+.skadnetwork$";
        private static readonly Regex SkAdNetworkIdRegExp = new Regex(SKADNETWORK_PATTERN);
        
#if UNITY_IOS 
        
        /// <summary>
        /// Method to easily update the info.plist file from the generated iOS project.
        /// </summary>
        /// <param name="pathToBuiltProject">Path to the Xcode project</param>
        /// <param name="action">Piece of code to execute on the info.plist file</param>
        public static void UpdateInfoPlistFile(string pathToBuiltProject, Action<PlistElementDict> action)
        {
            // Get the contents of the project file
            string plistPath = pathToBuiltProject + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;

            action(rootDict);

            // Save all the changes to the project file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
        
        /// <summary>
        /// update the pbxproj file from the generated iOS project.
        /// </summary>
        /// <param name="path">Path to the pbxproj file</param>
        /// <param name="action">Piece of code to execute on the pbxproj</param>
        public static void UpdatePBXProjectFile(string path, Action<PBXProject, string, string> action) 
        {
            string projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));

            // Methods available on unity >= 2019.3
            string mainTargetGuid = project.GetUnityMainTargetGuid();
            string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            
            action(project, mainTargetGuid, frameworkTargetGuid);

            File.WriteAllText(projectPath, project.WriteToString());
        }

#endif
        
        /// <summary>
        /// Checks if a string is a SKAdnetwork identifier.
        /// </summary>
        /// <param name="id">The identifier to check</param>
        /// <returns>True if the string is a SKAdnetwork identifier</returns>
        public static bool IsSkAdNetworkId(string id) => SkAdNetworkIdRegExp.IsMatch(id);
    }
}