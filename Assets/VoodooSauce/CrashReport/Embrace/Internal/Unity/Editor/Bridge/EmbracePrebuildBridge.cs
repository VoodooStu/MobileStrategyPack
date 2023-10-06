#if VS_EMBRACE_SDK
using EmbraceSDK.EditorView;
using UnityEditor;
using UnityEngine;
#endif

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    public static class EmbracePrebuildBridge
    {
        public static void SetDataDirectory(string dataDirectory)
        {
#if VS_EMBRACE_SDK
            AssetDatabaseUtil.EmbraceDataDirectory = dataDirectory;
            Debug.Log($"[VoodooSauce] force the Embrace configuration 'dataDirectory' to the value: '{dataDirectory}'");
#endif
        }
        public static void SetKeys(string androidAppId, string androidApiToken, string iosAppId, string iosApiToken)
        {
#if VS_EMBRACE_SDK
            var environments = AssetDatabaseUtil.LoadEnvironments();
            
            var androidConfiguration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(environments);
            androidConfiguration.AppId = androidAppId;
            androidConfiguration.SymbolUploadApiToken = androidApiToken;
            EditorUtility.SetDirty(androidConfiguration);
            
            Debug.Log($"[VoodooSauce] Embrace Android configuration applied with appId: '{androidAppId}' and apiToken: '{androidApiToken}'");

            var iOSConfiguration = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(environments);
            iOSConfiguration.AppId = iosAppId;
            iOSConfiguration.SymbolUploadApiToken = iosApiToken;
            EditorUtility.SetDirty(iOSConfiguration);
            
            Debug.Log($"[VoodooSauce] Embrace iOS configuration applied with appId: '{iosAppId}' and apiToken: '{iosApiToken}'");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif
        }
    }
}