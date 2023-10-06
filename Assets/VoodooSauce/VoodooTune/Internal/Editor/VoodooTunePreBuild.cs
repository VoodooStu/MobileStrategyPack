using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.VoodooTune
{
    public class VoodooTunePreBuild : IPreprocessBuildWithReport
    {
        private const string JSON_EXTENSION = ".json";
        private const string DEFAULT_JSON_FOLDER = "DefaultJsons";
        private const string RESOURCES_FOLDER = "Resources";

        private static readonly string NewDefaultJsonFolder =
            Path.Combine(Application.dataPath, RESOURCES_FOLDER, DEFAULT_JSON_FOLDER);

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!Directory.Exists(Path.Combine(NewDefaultJsonFolder))) {
                Directory.CreateDirectory(NewDefaultJsonFolder);   
            }
            
            CopyDefaultJsonFolderInResources(Application.dataPath);
        }

        private void CopyDefaultJsonFolderInResources(string folderPath)
        {
            switch (Path.GetFileName(folderPath))
            {
                case RESOURCES_FOLDER:
                    return;
                case DEFAULT_JSON_FOLDER:
                {
                    string[] files = Directory.GetFiles(folderPath);
                    foreach (string file in files) {
                        if (Path.GetExtension(file) != JSON_EXTENSION) {
                            continue;
                        }
                        
                        var destinationPath = Path.Combine(NewDefaultJsonFolder, Path.GetFileName(file));
                        File.Copy(file, destinationPath, true);
                    }

                    break;
                }
                default:
                {
                    var subDirectories = Directory.GetDirectories(folderPath);
                    foreach (var subdirectory in subDirectories) {
                        CopyDefaultJsonFolderInResources(subdirectory);   
                    }
                    break;
                }
            }
        }
    }
}