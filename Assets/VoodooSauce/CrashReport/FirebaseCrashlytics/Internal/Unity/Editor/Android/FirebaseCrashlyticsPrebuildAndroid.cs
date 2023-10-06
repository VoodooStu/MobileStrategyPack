#if UNITY_ANDROID

using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.CrashReport
{
    public class FirebaseCrashlyticsPrebuildAndroid : IPreprocessBuildWithReport
    {
        private const string BUILD_ID_RESOURCE = "crashlytics_build_id.xml";
        private const string UNITY_VERSION_RESOURCE = "crashlytics_unity_version.xml";
        private const string FIREBASE_CRASHLYTICS_MANIFEST_PATH = "Assets/VoodooSauce/CrashReport/FirebaseCrashlytics/Internal/Unity/Editor/Android/FirebaseCrashlyticsManifest.xml";
        private const string TEMPLATE_XML =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><resources>" +
            "<{0} name=\"{1}\" translatable=\"false\">{2}</{0}>" +
            "</resources>";
        private const string CRASHLYTICS_XML_FILE_PATH = "Plugins/Android/FirebaseCrashlytics.androidlib/res/values";
        
        public int callbackOrder => 99;

        public void OnPreprocessBuild(BuildReport report)
        {
            ManifestUtils.Add(FIREBASE_CRASHLYTICS_MANIFEST_PATH);
            
            // Need to run the function below to execute to create crashlytics's.xml for firebase
            // Unfortunately it was never being run on CI/CD batch mode so we need to do it manually here
            // And If we call the firebase's method, it will cause CI/CD run to fail since the docker image
            // only have module for it's supported platform 
            if (Application.isBatchMode && PlatformUtils.IS_LINUX && PlatformUtils.UNITY_ANDROID)
            {
                ProcessCrashlyticsXmlInCiCd();
            }
        }

        private void ProcessCrashlyticsXmlInCiCd()
        {
            WriteBuildIdIfExist();
            WriteUnityVersionIfExist();
        }
        
        private static void WriteBuildIdIfExist()
        {
            string buildIdPath = Path.Combine(Application.dataPath, CRASHLYTICS_XML_FILE_PATH, BUILD_ID_RESOURCE);
            if (File.Exists(buildIdPath))
            {
                return;
            }

            var generatedBuildId = Guid.NewGuid().ToString();
            if (!WriteXmlResource(buildIdPath, "com.crashlytics.android.build_id", generatedBuildId, "string"))
            {
                Console.WriteLine("Writing crashlytics build_id file failed");
            }
        }
        
        private static void WriteUnityVersionIfExist()
        {
            string unityVersionPath = Path.Combine(Application.dataPath, CRASHLYTICS_XML_FILE_PATH, UNITY_VERSION_RESOURCE);
            if (File.Exists(unityVersionPath))
            {
                return;
            }
            
            string unityVersion = Application.unityVersion;
            if (WriteXmlResource(unityVersionPath, "com.google.firebase.crashlytics.unity_version", unityVersion, "string")) 
            {
                Console.WriteLine("Writing crashlytics unity version file failed");
            }
        }
        
        /// Write the given key and value to an XML resource file at the given path.
        private static bool WriteXmlResource(string filePath, string key, string value, string resourceType)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (!File.Exists(filePath))
                {
                    fileInfo.Directory?.Create();
                    File.WriteAllText(filePath, String.Format(TEMPLATE_XML, resourceType, key, value));
                }
                
                Debug.Log("Generated resource for " + key + " : " + value);
            }
            catch (Exception e)
            {
                Debug.Log("Could not write " + key + " resource file." + e.Message);
                return false;
            }
            
            return true;
        }
    }
}

#endif