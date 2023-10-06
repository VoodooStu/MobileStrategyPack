using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Common.Utils;
using Debug = UnityEngine.Debug;

namespace Voodoo.Sauce.Firebase.Editor
{
    public class FirebasePreBuild : IPreprocessBuildWithReport
    {
        private const string AndroidFilesPath = "VoodooSauce/Common/Firebase/Internal/Android";
        private const string OldFirebaseAndroidFileDirectory = "Plugins/Android/Firebase";

        private const string GoogleServiceJsonFile = "Resources/Firebase/google-services.json";
        private const string GeneratedGoogleServiceXmlPath = "Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml";
        private const string FirebasePythonScriptPath = "VoodooSauce/Common/Firebase/3rdParty/Firebase/Editor/generate_xml_from_google_services_json.py";
        
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android) {
                return;
            }
            
            DeleteObsoleteFirebaseAndroidFolder();
            PrepareAndroidFiles();
        }

        private static void DeleteObsoleteFirebaseAndroidFolder()
        {
           if (Directory.Exists(Path.Combine(Application.dataPath, OldFirebaseAndroidFileDirectory))) {
                Directory.Delete(Path.Combine(Application.dataPath, OldFirebaseAndroidFileDirectory), true);
                Debug.Log("Firebase's obsolete folders deleted "+Path.Combine(Application.dataPath, OldFirebaseAndroidFileDirectory));
           }
        }
        
        private static void PrepareAndroidFiles()
        {
            string sourceManifestPath = Path.Combine(Application.dataPath, $"{AndroidFilesPath}/AndroidManifest.xml");
            string sourceProjectPath = Path.Combine(Application.dataPath, $"{AndroidFilesPath}/project.properties");

            if (!Directory.Exists(Path.Combine(Application.dataPath, "Plugins")))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Plugins"));
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Plugins/Android")))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Plugins/Android"));
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Plugins/Android/FirebaseApp.androidlib")))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Plugins/Android/FirebaseApp.androidlib"));

            string destManifestPath = Path.Combine(Application.dataPath, "Plugins/Android/FirebaseApp.androidlib/AndroidManifest.xml");
            string destProjectPath = Path.Combine(Application.dataPath, "Plugins/Android/FirebaseApp.androidlib/project.properties");

            File.Delete(destManifestPath);
            File.Copy(sourceManifestPath, destManifestPath);

            File.Delete(destProjectPath);
            File.Copy(sourceProjectPath, destProjectPath);

            // Need to run the function below to execute to create google-service.xml for firebase
            // Unfortunately it was never being run on CI/CD batch mode so we need to do it manually here
            // And If we call the firebase's method, it will cause CI/CD run to fail since the docker image
            // only have module for it's supported platform 
            if (Application.isBatchMode && PlatformUtils.IS_LINUX && PlatformUtils.UNITY_ANDROID)
                ExecuteCiCdPythonGoogleServiceGenerate();
            
            AssetDatabase.Refresh();
        }
        
        private static void ExecuteCiCdPythonGoogleServiceGenerate()
        {
            string sourceGoogleServiceJson = Path.Combine(Application.dataPath, GoogleServiceJsonFile);
            string destinationGoogleServiceXml = Path.Combine(Application.dataPath, GeneratedGoogleServiceXmlPath);
            string pythonScriptLocation = Path.Combine(Application.dataPath, FirebasePythonScriptPath);
            
            if (File.Exists(destinationGoogleServiceXml))
                return;
            
            try {
                var proc = new Process();
                proc.StartInfo.FileName = "python";
                proc.StartInfo.Arguments = $"{pythonScriptLocation} -i {sourceGoogleServiceJson} -o {destinationGoogleServiceXml}";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;

                proc.Start();
                string strOutput = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                Console.WriteLine(strOutput);
            } catch (Exception e) {
                Console.WriteLine("Error while executing firebase python script");
                Console.WriteLine(e);
            }
        }
    }
}