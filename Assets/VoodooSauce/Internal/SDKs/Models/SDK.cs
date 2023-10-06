using System;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Internal.SDKs
{
    [Serializable]
    public class SDK
    {
        public string name;
        public string icon;
        public SDKVersions versions;

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            SDK sdk = (SDK) obj;
            if ((name ?? "") != (sdk.name ?? "") ||
                (icon ?? "") != (sdk.icon ?? "") ||
                (versions == null || versions.IsEmpty()) && sdk.versions != null && !sdk.versions.IsEmpty() ||
                versions != null && !versions.Equals(sdk.versions))
                return false;
            return true;
        }
        
        public string[] GetVersionsAsArray()
        {
            return new [] {
                versions.unity,
                versions.ios,
                versions.android
            };
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (icon != null ? icon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (versions != null ? versions.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(name) &&
                string.IsNullOrEmpty(icon) &&
                (versions == null || versions.IsEmpty()))
                return true;
            return false;
        }
    }
    
    [Serializable]
    public class SDKVersions
    {
        public string unity;
        public string ios;
        public string android;

        public string[] GetVersionsAsArray()
        {
            return new [] {
                unity, ios, android
            };
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            SDKVersions versions = (SDKVersions) obj;
            if ((unity ?? "") != (versions.unity ?? "") ||
                (ios ?? "") != (versions.ios ?? "") ||
                (android ?? "") != (versions.android ?? ""))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = (unity != null ? unity.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ios != null ? ios.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (android != null ? android.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(unity) &&
                string.IsNullOrEmpty(ios) &&
                string.IsNullOrEmpty(android))
                return true;
            return false;
        }
    }
}