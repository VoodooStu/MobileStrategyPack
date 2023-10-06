#if UNITY_ANDROID
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using Voodoo.Sauce.Internal.Ads;

public class VoodooSSLPostBuildAndroid : IPostGenerateGradleAndroidProject
{
    private const string NETWORK_SECURITY_CONFIG_TEMPLATE_PATH = AdsConstants.ROOT_FOLDER_PATH + "/Editor/Android/NetworkSecurityConfig.xml";
    public int callbackOrder => 2;

    public void OnPostGenerateGradleAndroidProject(string projectPath)
    {
        // When exporting a gradle project projectPath points to the the parent folder of the project
        // instead of the actual project
        if (!Directory.Exists(Path.Combine(projectPath, "src"))) {
            projectPath = Path.Combine(projectPath, PlayerSettings.productName);
        }

        AddSecurityFile(projectPath);
    }

    private static void AddSecurityFile(string projectPath)
    {
        string content = File.ReadAllText(NETWORK_SECURITY_CONFIG_TEMPLATE_PATH);

        var fileInfo = new FileInfo($"{projectPath}/src/main/res/xml/network_security_config.xml");
        if (fileInfo.Directory != null) {
            fileInfo.Directory.Create();
            File.WriteAllText(fileInfo.FullName, content);
        }
    }
}
#endif