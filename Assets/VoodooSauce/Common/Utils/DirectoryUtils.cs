using System.IO;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Utils
{
    public static class DirectoryUtils
    {
        public static string ManifestFile => Directory.GetCurrentDirectory() + "/Packages/manifest.json";
    }
}