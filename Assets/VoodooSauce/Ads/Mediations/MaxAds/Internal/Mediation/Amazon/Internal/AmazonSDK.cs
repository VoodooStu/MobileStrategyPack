using System.IO;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Internal.SDKs;
using Voodoo.Sauce.Internal.SDKs.Models;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    [Preserve]
    public class AmazonSDK : IMaxAdsCustomNetworkSDK
    {
        private const string AMAZON_BASE_PATH =
            "/VoodooSauce/Ads/Mediations/MaxAds/Internal/Mediation/Amazon/3rdParty/Scripts";

        private static readonly string AdapterDependencyPath =
            (Application.dataPath + AMAZON_BASE_PATH + "/Mediations/AppLovinMediation/Editor/Dependencies.xml").Replace(
                '/', Path.DirectorySeparatorChar);

        private static readonly string SdkDependencyPath =
            (Application.dataPath + AMAZON_BASE_PATH + "/Editor/AmazonDependencies.xml").Replace('/',
                Path.DirectorySeparatorChar);

        public AdNetworkSDK GetSDKInformation()
        {
            var adNetworkSDK = new AdNetworkSDK();
            // Retrieve Android and iOS SDK version from the dependency file.
            MaxMediationAdapterSDK.UpdateAdNetwork(adNetworkSDK, AdapterDependencyPath, true);
            adNetworkSDK.name = "Amazon";
            adNetworkSDK.icon = "Resources/APS.png";
            Dependencies sdkDependencies = Dependencies.GetDependencies(SdkDependencyPath);
            adNetworkSDK.android.version = sdkDependencies.AndroidPackages.AndroidPackage
                .Find(dependency => dependency.Spec.Contains("aps-sdk"))
                ?.Version;
            adNetworkSDK.ios.version = sdkDependencies.IosPods.IosPod
                .Find(dependency => dependency.Name.Contains("AmazonPublisherServicesSDK"))
                ?.Version;
            return adNetworkSDK;
        }
    }
}