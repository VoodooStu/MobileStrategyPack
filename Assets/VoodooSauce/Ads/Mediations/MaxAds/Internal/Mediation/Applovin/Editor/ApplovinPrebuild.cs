using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation.Google.Editor
{
    public class ApplovinPrebuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 99;

        public void OnPreprocessBuild(BuildReport report)
        {
            // By syncing the SDK key here, Max will handle updating the info.plist file and the the AndroidManifest file.
            VoodooSettings settings = VoodooSettings.Load();
            AppLovinSettings.Instance.SdkKey = PlatformUtils.UNITY_IOS ? settings.MaxIosSdkKey : settings.MaxAndroidSdkKey;
        }
    }
}