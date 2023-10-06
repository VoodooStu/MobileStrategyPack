using System;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core.Model
{
    public static class EditorIdfa
    {
        
#region Constants
        
        private const string EDITOR_PREF_EDITOR_IDFA = "EditorIDFA";
        
#endregion
        
#region Properties
        
        private static string _value = "";
        
#endregion
        
#region Methods
        
        public static string Get()
        {
            if (!string.IsNullOrEmpty(_value)) {
                return _value;
            }
            
#if UNITY_EDITOR
            if (EditorPrefs.HasKey(EDITOR_PREF_EDITOR_IDFA)) {
                _value = EditorPrefs.GetString(EDITOR_PREF_EDITOR_IDFA);
            } else {
                _value = Guid.NewGuid().ToString();
                EditorPrefs.SetString(EDITOR_PREF_EDITOR_IDFA, _value);
            }
#endif

            return _value;
        }
        
#endregion
        
    }
}