using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation.Google.Editor
{
    public class GooglePrebuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 99;

        public void OnPreprocessBuild(BuildReport report)
        {
            // By syncing the AdMob Id here, Max will handle updating the info.plist file and the AndroidManifest file.
            VoodooSettings settings = VoodooSettings.Load();
            AppLovinSettings.Instance.AdMobIosAppId = settings.AdMobIosAppId;
            AppLovinSettings.Instance.AdMobAndroidAppId = settings.AdMobAndroidAppId;
        }
    }
}