using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    public class MaxAdsPrebuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 99;

        public void OnPreprocessBuild(BuildReport report)
        {
            VoodooSettings settings = VoodooSettings.Load();
            var needSaveAssets = false;
            //Ensure that MAX Ad Review is enabled
            if (!AppLovinSettings.Instance.QualityServiceEnabled) {
                AppLovinSettings.Instance.QualityServiceEnabled = true;
                needSaveAssets = true;
            }

            //Sync Max sdk key : needed to generate MAX Ad Review  key
            string maxSdkKey = settings.MaxSdkKey;
            if (maxSdkKey != AppLovinSettings.Instance.SdkKey) {
                AppLovinSettings.Instance.SdkKey = maxSdkKey;
                needSaveAssets = true;
            }
            
            if (needSaveAssets) AssetDatabase.SaveAssets();
        }
    }
}