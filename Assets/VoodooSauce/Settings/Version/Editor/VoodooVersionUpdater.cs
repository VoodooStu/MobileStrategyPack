using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    /**
     * This class must only be used for running from command line.
     * More details here: https://docs.unity3d.com/Manual/CommandLineArguments.html
     * See the Makefile at the root of this project to see some command lines.
     */
    public class VoodooVersionUpdater
    {
        private const string VersionFilePath = "VS_VERSION";
        
        public static void BumpMajor()
        {
            Bump((version, label) => version.BumpMajor(label));
        }
        
        public static void BumpMinor()
        {
            Bump((version, label) => version.BumpMinor(label));
        }
        
        public static void BumpHotfix()
        {
            Bump((version, label) => version.BumpHotfix(label));
        }
        
        public static void BumpBuildNumber()
        {
            Bump((version, label) => version.BumpBuildNumber());
        }
        
        public static void ResetBuildNumber()
        {
            Bump((version, label) => version.ResetBuildNumber());
        }
        
        public static void DisableBuildNumber()
        {
            Bump((version, label) => version.DisableBuildNumber());
        }
        
        public static void UpdateLabel()
        {
            Bump((version, label) => version.UpdateLabel(label));
        }
        
        public static void UpdateVersion()
        {
            if (!Application.isBatchMode) {
                throw new UnityException("This method must only be run on command line.");
            }

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            int commandLineArgsCount = commandLineArgs.Length;

            var major = Convert.ToUInt32(commandLineArgs[commandLineArgsCount-5]);
            var minor = Convert.ToUInt32(commandLineArgs[commandLineArgsCount-4]);
            var hotfix = Convert.ToUInt32(commandLineArgs[commandLineArgsCount-3]);
            var buildNumber = Convert.ToInt32(commandLineArgs[commandLineArgsCount-2]);
            string label = commandLineArgs[commandLineArgsCount-1];
            
            VoodooVersion version = VoodooVersion.Load();
            version.UpdateVersion(major, minor, hotfix, buildNumber, label);
            version.Save();

            UpdateVersionFile(version);
        }
        
        private static void Bump(Action<VoodooVersion, string> update)
        {
            if (!Application.isBatchMode) {
                throw new UnityException("This method must only be run on command line.");
            }
            
            // The label must be the last argument from the command line.
            // If the last argument is the name of the method called, so the label is considered as empty.
            string label = Environment.GetCommandLineArgs().Last();
            if (label.Contains("VoodooVersionUpdater")) {
                label = "";
            }
            
            VoodooVersion version = VoodooVersion.Load();
            update(version, label);
            version.Save();

            UpdateVersionFile(version);
        }
        
        private static void UpdateVersionFile(VoodooVersion version)
        {
            if (!Application.isBatchMode) {
                throw new UnityException("This method must only be run on command line.");
            }

            var versionString = version.ToString();

            // The version is written to a file so it can be read from the outside by anyone or any tool.
            var writer = new StreamWriter(VersionFilePath, false);
            writer.WriteLine(versionString);
            writer.Close();

            Debug.Log("New Voodoo Sauce Version : " + versionString);
        }
    }
}