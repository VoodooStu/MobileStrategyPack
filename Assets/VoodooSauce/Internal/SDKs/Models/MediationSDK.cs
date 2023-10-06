using System;
using System.Collections.Generic;

namespace Voodoo.Sauce.Internal.SDKs
{
    [Serializable]
    public class MediationSDK : SDK
    {
        public List<AdNetworkSDK> adNetworks;
        public List<SDK> otherUtilitySdk;
            

        public string[] GetVersionsAsArray()
        {
            return new[] { versions.unity, versions.ios, versions.android };
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            MediationSDK mediationSDK  = (MediationSDK)obj;
            if (adNetworks?.Count != mediationSDK.adNetworks?.Count || !base.Equals(mediationSDK))
            {
                return false;
            }
            
            for (int i = 0; i < adNetworks?.Count; i++)
            {
                if (!adNetworks[i].Equals(mediationSDK.adNetworks?[i]))
                    return false;
            }

            if (otherUtilitySdk?.Count != mediationSDK.otherUtilitySdk?.Count)
            {
                return false;
            }
            
            for (int i = 0; i < otherUtilitySdk?.Count; i++)
            {
                if (!otherUtilitySdk[i].Equals(mediationSDK.otherUtilitySdk?[i]))
                    return false;
            }
            
            return true;
        }

        public override int GetHashCode()
        {
            int adNetworkHashCode = adNetworks != null ? adNetworks.GetHashCode() : 0;
            int otherSdkHashCode = otherUtilitySdk != null ? otherUtilitySdk.GetHashCode() : 0;
            if (adNetworkHashCode != 0 && otherSdkHashCode != 0)
                //Combine both hashcode using XOR
                return adNetworkHashCode ^ otherSdkHashCode;
            if (adNetworkHashCode == 0 && otherSdkHashCode != 0)
                return otherSdkHashCode;
            if (adNetworkHashCode != 0 && otherSdkHashCode == 0)
                return adNetworkHashCode;
            return 0;
        }
    }
}