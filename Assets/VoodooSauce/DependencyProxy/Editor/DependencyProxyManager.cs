using UnityEditor;

namespace Voodoo.Sauce.DependencyProxy.Editor
{
    public static class DependencyProxyManager
    {
        public const string NEXUS_PROXY_HOST = "repository.voodoo-tech.io";

        private const string DEPENDENCY_PROXY_CONFIG_ASSET_PATH = "Assets/VoodooSauce/DependencyProxy/Editor/Resources/DependencyProxyConfig.asset";
        //user password and repo will be hardcoded for now since everyone will use the same user password
        //in the future if this nexus is proven to be useful already, then we can move this user & password to kitchen
        //for each studio / game
        private const string NEXUS_REPO_CREDENTIALS_USER = "repo-read-sdk";
        private const string NEXUS_REPO_CREDENTIALS_PASSWORD = "FTF6vbx0zya!zky8ckw";
        private static DependencyProxyConfig _config;


        private static DependencyProxyConfig GetDependencyProxyConfigFromAsset()
        {
            return (DependencyProxyConfig) AssetDatabase.LoadAssetAtPath(DEPENDENCY_PROXY_CONFIG_ASSET_PATH, typeof(DependencyProxyConfig));
        }
        public static bool IsDependencyProxyEnabled()
        {
            if (_config == null)
                _config = GetDependencyProxyConfigFromAsset();
            return _config.EnableDependencyProxy;
        }

        public static void SetDependencyProxy(bool isEnabled)
        {
            if (_config == null)
                _config = DependencyProxyConfig.Load();
            _config.EnableDependencyProxy = isEnabled;
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }

        public static string GetNexusRepoUsername()
        {
            return NEXUS_REPO_CREDENTIALS_USER;
        }
        
        public static string GetNexusRepoPassword()
        {
            return NEXUS_REPO_CREDENTIALS_PASSWORD;
        }
    }
}