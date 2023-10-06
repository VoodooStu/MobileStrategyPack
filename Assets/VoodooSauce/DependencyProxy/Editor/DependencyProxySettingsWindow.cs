using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.DependencyProxy.Editor
{
    public class DependencyProxySettingsWindow : EditorWindow
    {
        private bool _enableDependencyProxy;

        [MenuItem("VoodooSauce/Enable or Disable Dependency Proxy")]
        private static void ShowProxySettingWindow()
        {
            var window = GetWindow<DependencyProxySettingsWindow>();
            window.titleContent = new GUIContent("Dependency Proxy Settings");
            window.Init();
            window.maxSize = window.minSize = new Vector2(350, 500);
            window.Show();
        }
        private void Init()
        {
            _enableDependencyProxy = DependencyProxyManager.IsDependencyProxyEnabled();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dependency Proxy", EditorStyles.boldLabel);
            _enableDependencyProxy = EditorGUILayout.ToggleLeft("Use Dependency Proxy", _enableDependencyProxy);
            if (GUILayout.Button("Save and Close", GUILayout.Height(50)))
            {
                DependencyProxyManager.SetDependencyProxy(_enableDependencyProxy);
                Close();
            }
            EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("For Android build, the nexus authentication credentials will be automatically included in gradle files", EditorStyles.textArea);
            EditorGUILayout.LabelField("iOS", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("For iOS build, the dependency in the project are resolved with Cocoapods\n" +
                                       "In cocoapods we cannot maintain the credentials in podfile, cocoapods are using netrc file to maintain credentials\n" +
                                       "\nVoodooSauce will try to automatically create the netrc file if it's missing but, it might failed due to permissions \n" +
                                       "In that case you need to manually maintain the file by executing the command below in terminal\n", EditorStyles.textArea);
            EditorGUILayout.SelectableLabel($"echo 'machine {DependencyProxyManager.NEXUS_PROXY_HOST} login {DependencyProxyManager.GetNexusRepoUsername()} password {DependencyProxyManager.GetNexusRepoPassword()}' >>  ~/.netrc", EditorStyles.textArea);
            EditorGUILayout.LabelField(
                "For CI/CD workflow if you want to use the proxy please dont forget to maintain the .netrc file with the same command above", EditorStyles.textArea);
        }
    }
}

