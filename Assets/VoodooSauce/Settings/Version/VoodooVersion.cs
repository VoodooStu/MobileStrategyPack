using System;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal
{
    [Serializable]
    public class VoodooVersion : ScriptableObject
    {
        public const string NAME = "VoodooVersion";

        public static VoodooVersion Load() => Resources.Load<VoodooVersion>(NAME);

        public new string ToString() => $"{Major}.{Minor}.{Hotfix}{(Label.Length == 0 ? "" : $".{Label}")}{(BuildNumber < 0 ? "" : $".{BuildNumber}")}";

        [ReadOnly]
        public uint Major = 0;
        
        [ReadOnly]
        public uint Minor = 0;
        
        [ReadOnly]
        public uint Hotfix = 0;
        
        [ReadOnly]
        public string Label = "";
        
        [ReadOnly]
        public int BuildNumber = -1;

        public void BumpMajor(string label = "")
        {
            Major++;
            Minor = 0;
            Hotfix = 0;
            Label = label;
        }
        
        public void BumpMinor(string label = "")
        {
            Minor++;
            Hotfix = 0;
            Label = label;
        }
        
        public void BumpHotfix(string label = "")
        {
            Hotfix++;
            Label = label;
        }

        public void BumpBuildNumber()
        {
            BuildNumber++;
        }

        public void ResetBuildNumber()
        {
            BuildNumber = 0;
        }

        public void DisableBuildNumber()
        {
            BuildNumber = -1;
        }
        
        public void UpdateLabel(string label = "")
        {
            Label = label;
        }

        public void UpdateVersion(uint major, uint minor, uint hotfix, int buildNumber, string label = "")
        {
            Major = major;
            Minor = minor;
            Hotfix = hotfix;
            Label = label;
            BuildNumber = buildNumber;
        }

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}