using System;
using System.Collections.Generic;

namespace Voodoo.Sauce.Internal.SDKs
{
    [Serializable]
    public class SDKList
    {
        public string vsVersion;
        public string lastUpdateDate;
        public List<MediationSDK> ads;
        public List<SDK> analytics;
        public List<SDK> crashlytics;
        public List<SDK> audioAds;
        
        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            SDKList sdks = (SDKList)obj;
            if (vsVersion != sdks.vsVersion ||
                ads == null && sdks.ads != null || ads != null && sdks.ads == null ||
                analytics == null && sdks.analytics != null || analytics != null && sdks.analytics == null ||
                crashlytics == null && sdks.crashlytics != null || crashlytics != null && sdks.crashlytics == null ||
                audioAds == null && sdks.audioAds != null || audioAds != null && sdks.audioAds == null ||
                ads.Count != sdks.ads.Count ||
                analytics.Count != sdks.analytics.Count ||
                crashlytics.Count != sdks.crashlytics.Count ||
                audioAds.Count != sdks.audioAds.Count)
                return false;
            int i;
            for (i = 0; i < ads.Count; i++)
            {
                if (!ads[i].Equals(sdks.ads[i]))
                    return false;
            }
            for (i = 0; i < analytics.Count; i++)
            {
                if (!analytics[i].Equals(sdks.analytics[i]))
                    return false;
            }
            for (i = 0; i < crashlytics.Count; i++)
            {
                if (!crashlytics[i].Equals(sdks.crashlytics[i]))
                    return false;
            }
            for (i = 0; i < audioAds.Count; i++)
            {
                if (!audioAds[i].Equals(sdks.audioAds[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = (vsVersion != null ? vsVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (lastUpdateDate != null ? lastUpdateDate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ads != null ? ads.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (analytics != null ? analytics.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (crashlytics != null ? crashlytics.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (audioAds != null ? audioAds.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
