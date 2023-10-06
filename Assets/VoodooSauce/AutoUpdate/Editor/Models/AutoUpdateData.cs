using System;
using System.Collections.Generic;

namespace Voodoo.Sauce.Internal.Editor
{
    [Serializable]
    public class MetadataResponse
    {
        public List<VersionMetadata> Versions;
        public List<HotfixMetadata> Hotfixes;
    }

    [Serializable]
    public class VersionMetadata : Metadata
    {
        public string Name;
        public string Deadline;
        public string UpdateInstructionsUrl;

        public Package ToPackage()
        {
            var package = new Package {
                Version = Version,
                Title = "New Version Available : " + Version,
                SubTitle = "VoodooSauce " + Version + " is available!",
                Message = Message,
                WarningMessage = "Please update by " + Deadline,
                PackageUrl = PackageUrl,
                UpdateInstructionsUrl = UpdateInstructionsUrl,
                DirectoriesToRemove = DirectoriesToRemove,
                FailedMessage = "An error occurred during the download of Voodoo Sauce " + Version,
                SuccessMessage = "Voodoo Sauce updated with version " + Version
            };
            return package;
        }
    }

    [Serializable]
    public class HotfixMetadata : Metadata
    {
        public List<string> VoodooSauceVersion;

        public Package ToPackage()
        {
            var package = new Package {
                Version = Version,
                Title = "New Hotfix Available : " + Version,
                SubTitle = "A new hotfix for Voodoo Sauce v." + VoodooSauce.Version() + " is available!",
                Message = Message,
                WarningMessage = null,
                PackageUrl = PackageUrl,
                UpdateInstructionsUrl = null,
                DirectoriesToRemove = DirectoriesToRemove,
                FailedMessage = "An error occurred during the download of the Hotfix " + Version,
                SuccessMessage = "Voodoo Sauce hotfix installed with hotfix " + Version
            };
            return package;
        }
    }

    [Serializable]
    public class Metadata
    {
        public string Version;
        public string Message;
        public string PackageUrl;
        public List<string> DirectoriesToRemove;
        public List<string> BundleIdsToIgnore;
        public List<string> BundleIdsWhiteList;
    }

    [Serializable]
    public class Package
    {
        public string Version;
        public string Title;
        public string SubTitle;
        public string Message;
        public string WarningMessage;
        public string PackageUrl;
        public string UpdateInstructionsUrl;
        public List<string> DirectoriesToRemove;
        public string FailedMessage;
        public string SuccessMessage;
    }
}