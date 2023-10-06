using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Firebase.Interfaces;
using Voodoo.Sauce.Internal.SDKs;
using Voodoo.Sauce.Internal.SDKs.Models;

[Preserve]
public class FirebaseCrashlyticsProviderSDK : ICrashlyticsProviderSDK
{
    public SDK GetSDKInformations()
    {
        var sdk = new SDK {name = "Firebase Crashlytics"};
        sdk.icon = "Resources/" + sdk.name.Replace(" ", "") + ".png";
        var dependencies = Dependencies.GetDependencies(Application.dataPath + "/VoodooSauce/CrashReport/FirebaseCrashlytics/3rdParty/FirebaseCrashlytics/Editor/CrashlyticsDependencies.xml");
        sdk.versions = new SDKVersions {unity = "10.1.1"};
        string androidPackage = dependencies.AndroidPackages?.AndroidPackage.Find(x => x.Spec.Contains("com.google.firebase:firebase-crashlytics"))?.Spec ?? "";
        sdk.versions.android = androidPackage.Split(':').Last();
        sdk.versions.ios = dependencies.IosPods?.IosPod[0].Version;
        return sdk;
    }
}
