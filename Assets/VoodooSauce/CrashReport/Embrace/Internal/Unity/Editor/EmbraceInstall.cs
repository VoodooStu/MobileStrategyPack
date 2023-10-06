using System.Collections.Generic;
using UnityEditor;
using Voodoo.Sauce.Internal.Editor;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    public static class EmbraceInstall
    {
        [InitializeOnLoadMethod]
        private static void Install()
        {
            var dependencyInstaller = new DependencyInstaller();
            
            dependencyInstaller.AddScopedRegistry(new ScopedRegistry {
                name = EmbraceDependency.SCOPED_REGISTRY_NAME,
                url = EmbraceDependency.SCOPED_REGISTRY_URL,
                scopes = new List<string> {
                    EmbraceDependency.EMBRACE_SCOPE
                }
            });
            
            dependencyInstaller.InstallPackage(
                EmbraceDependency.PACKAGE_IDENTIFIER,
                EmbraceDependency.PACKAGE_VERSION);
        }
    }
}