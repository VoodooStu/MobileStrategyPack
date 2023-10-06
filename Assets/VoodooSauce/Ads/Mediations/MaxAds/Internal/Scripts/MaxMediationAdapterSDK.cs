using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.SDKs;
using Voodoo.Sauce.Internal.SDKs.Models;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    [Preserve]
    public class MaxMediationAdapterSDK : IMediationAdapterSDK
    {
        private const string NAME = "MaxAds";
        private const string MEDIATION_FOLDER_PATH = "/VoodooSauce/Ads/Mediations/MaxAds/3rdParty/MaxSdk/";
        private const string MEDIATION_CUSTOM_AD_NETWORKS_FOLDER_PATH = "/VoodooSauce/Ads/Mediations/MaxAds/Internal/Mediation/";
        private readonly string _adNetworksFolderPath = $"{MEDIATION_FOLDER_PATH}Mediation/";
        private const string AD_NETWORK_DEPENDENCIES_PATH = "/Editor/Dependencies.xml";

        public MediationSDK GetSDKInformations()
        {
            if (!Directory.Exists(Application.dataPath + MEDIATION_FOLDER_PATH)) return null;
            Dependencies applovinDependencies = Dependencies.GetDependencies(
                Application.dataPath + 
                MEDIATION_FOLDER_PATH + 
                "AppLovin" + 
                AD_NETWORK_DEPENDENCIES_PATH); 

            MediationSDK mediation = new MediationSDK();
            mediation.name = NAME;
            mediation.icon = "Resources/" + NAME.Replace(" ", "") + ".png";
            mediation.versions = new SDKVersions();
            mediation.versions.unity = MaxSdk.Version;
            mediation.versions.android = applovinDependencies.AndroidPackages?.AndroidPackage
                                                       .Find(x => x.Spec.Contains("applovin")).Spec.Split(':')
                                                       .Last();
            mediation.versions.ios = applovinDependencies.IosPods.IosPod[0].Version;
            mediation.adNetworks = new List<AdNetworkSDK>();
            
            AddAdNetworks(mediation, _adNetworksFolderPath, true);
            AddAdNetworks(mediation, MEDIATION_CUSTOM_AD_NETWORKS_FOLDER_PATH, false);

            foreach (IMaxAdsCustomNetworkSDK customNetworks in AssembliesUtils.InstantiateInterfaceImplementations<IMaxAdsCustomNetworkSDK>())
            {
                mediation.adNetworks.Add(customNetworks.GetSDKInformation());
            }
            
            return mediation;
        }

        private static void AddAdNetworks(MediationSDK mediation, string directoryPath, bool isCertified)
        {
            foreach (string directory in Directory.GetDirectories(Application.dataPath + directoryPath))
            {
                string dependencyFilePath = directory + AD_NETWORK_DEPENDENCIES_PATH;
                if (File.Exists(dependencyFilePath) == false)
                {
                    continue;
                }
                
                AdNetworkSDK adNetwork = new AdNetworkSDK();
                adNetwork.name = directory.Split('/').Last();
                adNetwork.icon = "Resources/" + adNetwork.name + ".png";
                
                UpdateAdNetwork(adNetwork, dependencyFilePath, isCertified);
                
                mediation.adNetworks.Add(adNetwork);
            }
        }
        
        internal static void UpdateAdNetwork(AdNetworkSDK adNetworkSDK, string dependencyFilePath, bool certified)
        {
            Dependencies dependencies = Dependencies.GetDependencies(dependencyFilePath);
            
            UpdateAdNetworkAndroid(adNetworkSDK, dependencies, certified);
            UpdateAdNetworkiOS(adNetworkSDK, dependencies, certified);
        }

        private static void UpdateAdNetworkAndroid(AdNetworkSDK adNetworkSDK, Dependencies dependencies, bool isCertified)
        {
            string androidAdapterVersion = GetAndroidAdapterVersionName(dependencies);
            
            if (androidAdapterVersion != null)
            {
                adNetworkSDK.android = new AdNetworkPlatform
                {
                    certified = isCertified,
                    version = androidAdapterVersion.RemoveVersionLastDigit(),
                    adapter = new Adapter {version = androidAdapterVersion}
                };
            }
        }

        private static string GetAndroidAdapterVersionName(Dependencies dependencies)
        {
            string androidAdapterVersion = dependencies.AndroidPackages?.AndroidPackage
                .Find(x => x.Spec.Contains("adapter"))
                ?.Version;

            if (string.IsNullOrEmpty(androidAdapterVersion))
            {
                androidAdapterVersion = dependencies.AndroidPackages?.AndroidPackage[0].Version;
            }

            return androidAdapterVersion;
        }
        
        private static void UpdateAdNetworkiOS(AdNetworkSDK adNetworkSDK, Dependencies dependencies, bool isCertified)
        {
            string iOSAdapterVersion = GetIosAdapterVersionName(dependencies);
            
            if (iOSAdapterVersion != null)
            {
                adNetworkSDK.ios = new AdNetworkPlatform
                {
                    certified = isCertified,
                    version = iOSAdapterVersion.RemoveVersionLastDigit(),
                    adapter = new Adapter {version = iOSAdapterVersion}
                };
            }
        }

        private static string GetIosAdapterVersionName(Dependencies dependencies)
        {
            string iOSAdapterVersion = dependencies.IosPods?.IosPod
                .Find(x => x.Name.Contains("Adapter"))?.Version;
                
            if (string.IsNullOrEmpty(iOSAdapterVersion))
            {
                iOSAdapterVersion = dependencies.IosPods?.IosPod.First()?.Version;
            }

            return iOSAdapterVersion;
        }
    }
}