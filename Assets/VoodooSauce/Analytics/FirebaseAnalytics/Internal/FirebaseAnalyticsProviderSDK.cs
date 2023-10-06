using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Internal.SDKs;
using Voodoo.Sauce.Internal.SDKs.Models;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    public class FirebaseAnalyticsProviderSDK : IAnalyticsProviderSDK
    {
        public SDK GetSDKInformations()
        {
            var sdk = new SDK {name = "Firebase Analytics"};
            sdk.icon = "Resources/" + sdk.name.Replace(" ", "") + ".png";
            var dependencies = Dependencies.GetDependencies(Application.dataPath + "/VoodooSauce/Analytics/FirebaseAnalytics/3rdParty/FirebaseAnalytics/Editor/AnalyticsDependencies.xml");
            sdk.versions = new SDKVersions {unity = "10.1.1"};
            string androidPackage = dependencies.AndroidPackages?.AndroidPackage.Find(x => x.Spec.Contains("com.google.firebase:firebase-analytics"))?.Spec ?? "";
            sdk.versions.android = androidPackage.Split(':').Last();
            sdk.versions.ios = dependencies.IosPods?.IosPod[0].Version;
            return sdk;
        }
    }
}