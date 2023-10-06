// ReSharper disable CheckNamespace
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    public static class ShellHelper
    {
        private const string GIT_COMMAND = "git";

        public static string GetGitRepoUrl() => CleanString(ExecuteGitCommand("config --get remote.origin.url"));

        public static string GetGitActiveBranch() => CleanString(ExecuteGitCommand("rev-parse --abbrev-ref HEAD"));

        public static string GetGitUsername() => CleanString(ExecuteGitCommand("config user.name"));

        public static string GetGitCommitHash() => CleanString(ExecuteGitCommand("rev-parse HEAD"));

        public static string GetXcodeVersion() => CleanString(ExecuteCommand("xcodebuild", "-version"));

        public static string GetGitLastCommitTime() => CleanString(ExecuteGitCommand("log --date=iso -1 --format=%cd"));
        
        public static string GetLastTimeVoodooVersionUpdated() => CleanString(ExecuteGitCommand(
            "log -1 --date=iso --format=%ad Assets/VoodooSauce/Settings/Version/Resources/VoodooVersion.asset"));

        private static string GetLastVoodooVersionUpdateCommitHash() => CleanString(ExecuteGitCommand(
            "log -1 --format=%H Assets/VoodooSauce/Settings/Version/Resources/VoodooVersion.asset"));
        
        public static string GetAndroidGradleVersion()
        {
            Version gradleVersion = GradleCustomConfigHelper.GetCurrentGradleVersion();
            if (gradleVersion == null) return "";
            return gradleVersion.ToString();
        }
        
        public static bool IsVoodooVersionUpdated()
        {
            string resultString = ExecuteGitCommand(
                "diff Assets/VoodooSauce/Settings/Version/Resources/VoodooVersion.asset");
            
            List<string> results = resultString.Split('\n').Where(s => s.Contains("Major:") ||
                s.Contains("Minor:") || s.Contains("Hotfix:") || s.Contains("Label:") || s.Contains("BuildNumber:"))
                .ToList();
            
            if (results.Count < 2)
                return false;

            var comparator = new Dictionary<string, string>();
            foreach (var lineItem in results) {
                string lineItemStr = lineItem;
                if (lineItem.StartsWith("+ ")) {
                    lineItemStr = lineItemStr.Replace("+ ", "");
                }
                else if (lineItem.StartsWith("- ")) {
                    lineItemStr = lineItemStr.Replace("- ", "");
                }

                string[] lineSplit = lineItemStr.Split(':');
                if (lineSplit.Length < 2) continue;
                string key = lineSplit[0].Trim();
                string value = lineSplit[1].Trim();

                if (comparator.ContainsKey(key)) {
                    if (!comparator[key].Equals(value))
                        return true;
                } else {
                    comparator.Add(key, value);
                }
            }
            
            return false;
        }
        
        public static bool IsIconUpdated()
        {
            string resultString = ExecuteGitCommand(
                "diff ProjectSettings/ProjectSettings.asset");
            
            List<string> results = resultString.Split('\n').Where(s => s.Contains("m_Icon:")).ToList();
            if (results.Count < 2)
                return false;
            string[] guid = new string[2];
            
            var matchCollection = Regex.Matches(results[0], ", guid: ([0-9a-f]+)");
            guid[0] = matchCollection.Count > 0 ? matchCollection[0].Value : "";
            
            matchCollection = Regex.Matches(results[1],", guid: ([0-9a-f]+)");
            guid[1] = matchCollection.Count > 0 ? matchCollection[0].Value : "";
            
            return !guid[0].Equals(guid[1]);
        }

        public static string GetLastTimeIconUpdated()
        {
            var resultString = ExecuteGitCommand(
                "blame -L /m_Icon:/,+1 ProjectSettings/ProjectSettings.asset");

            String[] results = resultString.Split('\n');
            for (int i = 0; i < results.Length; i++) {
                if (results[i].Contains("m_Icon:") && i > 0) {
                    resultString = results[i];
                    break;
                }
            }

            MatchCollection collection = Regex.Matches(resultString,
                "20[0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-9][0-9]:[0-9][0-9] \\+[0-9][0-9][0-9][0-9] ");

            return collection.Count > 0 ? CleanString(collection[0].Value) : "";
        }

        private static string ExecuteGitCommand(string withArguments) => ExecuteCommand(GIT_COMMAND, withArguments);

        private static string ExecuteCommand(string filename, string withArguments)
        {
            var proc = new Process();
            proc.StartInfo.FileName = filename;
            proc.StartInfo.Arguments = withArguments;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;

            proc.Start();

            string strOutput = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return strOutput;
        }

        private static string CleanString(string stringInput) => stringInput.Replace("\n", "").Trim();
    }
}