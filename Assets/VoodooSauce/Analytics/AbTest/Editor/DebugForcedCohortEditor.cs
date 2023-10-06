using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Extension;

namespace Voodoo.Sauce.Internal.Analytics.Editor
{
    internal class DebugForcedCohortEditor : PropertyDrawer
    {
        protected VoodooSettings _settings;
        private int _selectedIndex;

        internal virtual string[] RunningAbTests() => new string[]{};

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_settings == null) {
                _settings = Resources.Load<VoodooSettings>("VoodooSettings");
            }

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (_settings != null && RunningAbTests() != null) {
                var options = new string[RunningAbTests().Length + 3];
                options[0] = "No Debug Forced Cohort";
                options[1] = "None (no tracking)";
                options[2] = "Control";
                for (var i = 3; i < options.Length; i++) {
                    options[i] = RunningAbTests()[i - 3].Replace("/", "\u2215");
                }

                _selectedIndex = property.FindPropertyRelative("index").intValue;
                _selectedIndex = EditorGUI.Popup(position, _selectedIndex, options);
                property.FindPropertyRelative("index").intValue = _selectedIndex;
            } else {
                EditorGUI.LabelField(position, "Couldn't load VoodooSettings");
            }

            EditorGUI.EndProperty();
        }
    }
}