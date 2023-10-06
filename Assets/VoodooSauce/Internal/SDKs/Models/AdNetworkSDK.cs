using System;

namespace Voodoo.Sauce.Internal.SDKs
{
    [Serializable]
    public class AdNetworkSDK
    {
        public string name;
        public string icon;
        public AdNetworkPlatform ios;
        public AdNetworkPlatform android;

        public string[] GetVersionsAsArray()
        {
            return new [] {
                ios.version, ios.adapter.version,
                android.version, android.adapter.version,
            };
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            AdNetworkSDK adNetworkSDK  = (AdNetworkSDK)obj;
            if ((name ?? "") != (adNetworkSDK.name ?? "") ||
                (icon ?? "") != (adNetworkSDK.icon ?? "") ||
                (ios == null || ios.IsEmpty()) && adNetworkSDK.ios != null && !adNetworkSDK.ios.IsEmpty() ||
                ios != null && !ios.Equals(adNetworkSDK.ios) ||
                (android == null || android.IsEmpty()) && adNetworkSDK.android != null && !adNetworkSDK.android.IsEmpty() ||
                android != null && !android.Equals(adNetworkSDK.android))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (icon != null ? icon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ios != null ? ios.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (android != null ? android.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(name) &&
                string.IsNullOrEmpty(icon) &&
                (ios == null || ios.IsEmpty()) &&
                (android == null || android.IsEmpty()))
                return true;
            return false;
        }
    }
    
    [System.Serializable]
    public class AdNetworkPlatform
    {
        public string version;
        public bool certified;
        public Adapter adapter;
        
        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            AdNetworkPlatform adNetworkPlatform  = (AdNetworkPlatform)obj;
            if ((version ?? "") != (adNetworkPlatform.version ?? "") ||
                certified != adNetworkPlatform.certified ||
                (adapter == null || adapter.IsEmpty()) && adNetworkPlatform.adapter != null && !adNetworkPlatform.adapter.IsEmpty() ||
                adapter != null && !adapter.Equals(adNetworkPlatform.adapter))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = (version != null ? version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ certified.GetHashCode();
                hashCode = (hashCode * 397) ^ (adapter != null ? adapter.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(version) &&
                (adapter == null || adapter.IsEmpty()))
                return true;
            return false;
        }
    }

    [System.Serializable]
    public class Adapter
    {
        public string version;
        public AdFormat banner;
        public AdFormat interstitial;
        public AdFormat rewardedVideo;
        
        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            Adapter adapter  = (Adapter)obj;
            if ((version ?? "") != (adapter.version ?? "") ||
                (banner == null || banner.IsEmpty()) && adapter.banner != null && !adapter.banner.IsEmpty() ||
                banner != null && !banner.Equals(adapter.banner) ||
                (interstitial == null || interstitial.IsEmpty()) && adapter.interstitial != null && !adapter.interstitial.IsEmpty() ||
                interstitial != null && !interstitial.Equals(adapter.interstitial) ||
                (rewardedVideo == null || rewardedVideo.IsEmpty()) && adapter.rewardedVideo != null && !adapter.rewardedVideo.IsEmpty() ||
                rewardedVideo != null && !rewardedVideo.Equals(adapter.rewardedVideo))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = (version != null ? version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (banner != null ? banner.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (interstitial != null ? interstitial.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (rewardedVideo != null ? rewardedVideo.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(version) &&
                (banner == null || banner.IsEmpty()) &&
                (interstitial == null || interstitial.IsEmpty()) &&
                (rewardedVideo == null || rewardedVideo.IsEmpty()))
                return true;
            return false;
        }
    }

    [System.Serializable]
    public class AdFormat
    {
        public string className;
        public string customData;
        
        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            AdFormat adFormat  = (AdFormat)obj;
            if ((className ?? "") != (adFormat.className ?? "") ||
                (customData ?? "") != (adFormat.customData ?? ""))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((className != null ? className.GetHashCode() : 0) * 397) ^ (customData != null ? customData.GetHashCode() : 0);
            }
        }

        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(className) && string.IsNullOrEmpty(customData))
                return true;
            return false;
        }
    }
}