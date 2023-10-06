using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Analytics.Common.Internal;
using Voodoo.Sauce.Firebase.Interfaces;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.SDKs;
using Voodoo.Sauce.Internal.Utils;

public static class SDKListGenerator
{
    public static void GenerateSDKList()
    {
        if (!Directory.Exists(VoodooSauceSDKs.FOLDER))
            Directory.CreateDirectory(VoodooSauceSDKs.FOLDER);
        VoodooSauceSDKs newSDKList = ScriptableObject.CreateInstance<VoodooSauceSDKs>();
        newSDKList.SDKs = GetSDKList();
        SDKList oldSDKList = VoodooSauceSDKs.Load()?.SDKs;
        if (oldSDKList == null || !newSDKList.SDKs.Equals(oldSDKList))
        {
            AssetDatabase.CreateAsset(newSDKList, VoodooSauceSDKs.PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    public static SDKList GetSDKList()
    {
        SDKList sdks = new SDKList();
        sdks.vsVersion = VoodooSauce.Version();
        sdks.lastUpdateDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");;
        sdks.ads = GetMediationsInformations();
        sdks.analytics = GetAnalyticsInformations();
        sdks.crashlytics = GetCrashlyticsInformations();
        sdks.audioAds = GetAudioAdsInformations();
        return sdks;
    }
    
    private static List<MediationSDK> GetMediationsInformations()
    {
        List<MediationSDK> mediationSdks = new List<MediationSDK>();
        foreach (IMediationAdapterSDK i in AssembliesUtils.InstantiateInterfaceImplementations<IMediationAdapterSDK>())
        {
            if(i.GetSDKInformations() != null)
                mediationSdks.Add(i.GetSDKInformations());
        }
        return mediationSdks;
    }
    
    private static List<SDK> GetAnalyticsInformations()
    {
        List<SDK> mediationSdks = new List<SDK>();
        foreach (IAnalyticsProviderSDK i in AssembliesUtils.InstantiateInterfaceImplementations<IAnalyticsProviderSDK>())
        {
            if(i.GetSDKInformations() != null)
                mediationSdks.Add(i.GetSDKInformations());
        }
        return mediationSdks;
    }
    
    private static List<SDK> GetCrashlyticsInformations()
    {
        List<SDK> mediationSdks = new List<SDK>();
        foreach (ICrashlyticsProviderSDK i in AssembliesUtils.InstantiateInterfaceImplementations<ICrashlyticsProviderSDK>())
        {
            if(i.GetSDKInformations() != null)
                mediationSdks.Add(i.GetSDKInformations());
        }
        return mediationSdks;
    }

    private static List<SDK> GetAudioAdsInformations()
    {
        List<SDK> mediationSdks = new List<SDK>();
        foreach (IAudioAdsSDK i in AssembliesUtils.InstantiateInterfaceImplementations<IAudioAdsSDK>())
        {
            if(i.GetSDKInformations() != null)
                mediationSdks.Add(i.GetSDKInformations());
        }
        return mediationSdks;
    }
}
