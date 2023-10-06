#if UNITY_IOS || UNITY_TVOS
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.iOS.Xcode;

namespace Voodoo.Sauce.Internal.Utils
{
    public static class InfoPlistUtils
    {
        public static void UpdateRootDict(string buildPath, Dictionary<string, object> keysValuesToAdd, List<string> keysToRemove = null)
        {
            string plistPath = Path.Combine(buildPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            IDictionary<string, PlistElement> rootDictValues = plist.root.values;
            
            if (keysToRemove != null && keysToRemove.Count>0 ) {
                foreach (string key in keysToRemove.Where(key => rootDictValues.ContainsKey(key))) {
                    rootDictValues.Remove(key);
                }
            }
            
            if (keysValuesToAdd != null && keysValuesToAdd.Count>0 ) {
                foreach (string key in keysValuesToAdd.Keys) {
                    switch (keysValuesToAdd[key]) {
                        case int _:
                            rootDict.SetInteger(key, (int) keysValuesToAdd[key]);
                            break;
                        case string _:
                            rootDict.SetString(key, (string) keysValuesToAdd[key]);
                            break;
                        case float _:
                            rootDict.SetReal(key, (float) keysValuesToAdd[key]);
                            break;
                        case bool _:
                            rootDict.SetBoolean(key, (bool) keysValuesToAdd[key]);
                            break;
                        case PlistElementDict _:
                            rootDictValues.Add(key, (PlistElementDict) keysValuesToAdd[key]);
                            break;
                    }
                }
            }

            // Write the file with the updated settings.
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif