using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    /// <summary>
    /// Utility class to manage the Unity dependencies
    /// </summary>
    public class DependencyInstaller
    {
#region Constants

        private const string TAG = "[Dependency Installer]";

#endregion
        
#region Properties

        private readonly string _manifestFilePath;

#endregion

#region Constructors

        public DependencyInstaller(string manifestPath = null) => _manifestFilePath = manifestPath ?? DirectoryUtils.ManifestFile;

#endregion
        
#region Packages

        /// <summary>
        /// Install a package if it is not installed yet.
        /// If a previous version is already installed, this version will be uninstalled before installing the new version.
        /// </summary>
        /// <param name="packageIdentifier">The identifier of the package to install</param>
        /// <param name="packageVersion">The version of the package to install</param>
        public bool InstallPackage(string packageIdentifier, string packageVersion) 
        {
            string fullQualifiedPackageIdentifier = string.Join("@", packageIdentifier, packageVersion);
            
            // Do not reinstall the package.
            if (IsPackageAlreadyInstalled(packageIdentifier, packageVersion)) {
                return true;
            }

            // Install the package. 
            AddRequest addRequest = Client.Add(fullQualifiedPackageIdentifier);
            while (addRequest.IsCompleted == false) { }

            if (addRequest.Error != null) {
                Debug.LogError($"{TAG} Package '{fullQualifiedPackageIdentifier}' not installed: {addRequest.Error.message}");
                return false;
            }

            Debug.Log($"{TAG} Successfully installed package '{addRequest.Result.displayName}' ({addRequest.Result.name}@{addRequest.Result.version})");
            return true;
        }

        /// <summary>
        /// Uninstall a package.
        /// </summary>
        /// <param name="packageIdentifier">The identifier of the package to uninstall</param>
        /// <returns></returns>
        public bool UninstallPackage(string packageIdentifier)
        {
            if (!IsPackageAlreadyInstalled(packageIdentifier)) {
                return true;
            }

            RemoveRequest removeRequest = Client.Remove(packageIdentifier);
            while (removeRequest.IsCompleted == false) { }
            
            if (removeRequest.Error != null) {
                Debug.LogError($"{TAG} Unable to uninstall the package '{packageIdentifier}': {removeRequest.Error.message}");
                return false;
            }
            
            Debug.Log($"{TAG} Package '{packageIdentifier}' uninstalled");
            return true;
        }

        /// <summary>
        /// Returns true if a package with an identifier and a version (optional) is installed.
        /// </summary>
        /// <param name="packageIdentifier">The identifier of the package</param>
        /// <param name="packageVersion">The version of the package</param>
        /// <returns>True is the package is installed</returns>
        public bool IsPackageAlreadyInstalled(string packageIdentifier, string packageVersion = null)
        {
            ListRequest req = Client.List();
            while (req.IsCompleted == false) { }

            return req.Result?.FirstOrDefault(p => p.name == packageIdentifier && (packageVersion == null || p.version == packageVersion)) != null;
        }

#endregion
        
#region Scoped Registries
        
        public bool AddScopedRegistry(ScopedRegistry scopedRegistry) {
            try {
                string manifestJson = File.ReadAllText(_manifestFilePath);
 
                var manifest = JsonConvert.DeserializeObject<ManifestJson>(manifestJson);
                if (manifest.IsScopedRegistryAlreadyInstalled(scopedRegistry)) {
                    return true;
                }
                
                // Remove if needed an outdated registry
                manifest.scopedRegistries.Remove(scopedRegistry);
                manifest.scopedRegistries.Add(scopedRegistry);
 
                File.WriteAllText(_manifestFilePath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
                
                Debug.Log($"{TAG} Scoped registry '{scopedRegistry.name}' added");
            } catch (Exception e) {
                Debug.LogError($"{TAG} Unable to add the scoped registry '{scopedRegistry.name}': {e.Message}");
                return false;
            }
            
            return true;
        }
        
#endregion
    }

    /// <summary>
    /// Represents a scope registry from the Package Manager
    /// </summary>
    public class ScopedRegistry {
        public string name;
        public string url;
        public List<string> scopes;

        public override bool Equals(object obj)
        {
            var other = obj as ScopedRegistry;
            return obj != null && other != null && url == other.url;
        }

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => (url != null ? url.GetHashCode() : 0);
    }

    /// <summary>
    /// Represents the manifest.json file from the packages of this project
    /// </summary>
    internal class ManifestJson {
        public readonly Dictionary<string,string> dependencies = new Dictionary<string, string>();
        public readonly List<ScopedRegistry> scopedRegistries = new List<ScopedRegistry>();

        public bool IsScopedRegistryAlreadyInstalled(ScopedRegistry scopedRegistry) =>
            scopedRegistries.FirstOrDefault(current => current.url == scopedRegistry.url && current.scopes.SequenceEqual(scopedRegistry.scopes)) != null;
    }
}