using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;

namespace Voodoo.Sauce.DependencyProxy.Editor.PostBuild
{
    public class DependencyProxyAndroidPostBuild : IPostGenerateGradleAndroidProject
    {
        private const string MAVEN_USER_NAME_KEY = "mavenUser";
        private const string MAVEN_USER_PASSWORD_KEY = "mavenPassword";
        private const string GRADLE_REGEX_UNITY_MAVEN_LOCATION = "def unityProjectPath = (.*)";
        private const string VOODOO_NEXUS_MAVEN_PROXY_URL = "https://"+DependencyProxyManager.NEXUS_PROXY_HOST+"/repository/voodoo-maven-proxy/";
        private const string GRADLE_PROXY_MAVEN_VALUE = "\n" +
                                                        "        maven {\n" +
                                                        "            credentials {\n" +
                                                        "                username \"$mavenUser\"\n" +
                                                        "                password \"$mavenPassword\"\n" +
                                                        "            }\n" +
                                                        "        url \""+VOODOO_NEXUS_MAVEN_PROXY_URL+"\"\n" +
                                                        "        }\n";
        
        public int callbackOrder => int.MaxValue;
        
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!DependencyProxyManager.IsDependencyProxyEnabled()) return;
            var rootProjectPath = GetRootAndroidProjectPath(path);
            AddDependencyProxyAuthSettings(rootProjectPath);
            AddDependencyProxyMavenInGradleFile(rootProjectPath);
        }
        
        private static string GetRootAndroidProjectPath(string path)
        {
            var index = path.LastIndexOf('/');
            if (index == -1) index = path.LastIndexOf('\\');

            return index == -1 ? path : path.Substring(0, index);
        }

        private static void AddDependencyProxyAuthSettings(string projectPath)
        {
            foreach (var file in Directory.EnumerateFiles(projectPath, "gradle.properties", SearchOption.AllDirectories))
            {
                string[] content = {"",
                    $"{MAVEN_USER_NAME_KEY}={DependencyProxyManager.GetNexusRepoUsername()}", 
                    $"{MAVEN_USER_PASSWORD_KEY}={DependencyProxyManager.GetNexusRepoPassword()}"
                };
                File.AppendAllLines(file, content);
            }
        }

        private static void AddDependencyProxyMavenInGradleFile(string projectPath)
        {
            foreach (var file in Directory.EnumerateFiles(projectPath, "*.gradle", SearchOption.AllDirectories))
            {
                var fileContent = File.ReadAllText(file);
                var regexMatch = Regex.Match(fileContent, GRADLE_REGEX_UNITY_MAVEN_LOCATION);
                if (!regexMatch.Success) continue;
                fileContent = fileContent.Insert(regexMatch.Index + regexMatch.Length, GRADLE_PROXY_MAVEN_VALUE);
                File.WriteAllText(file, fileContent);
            }
        }
    }
}