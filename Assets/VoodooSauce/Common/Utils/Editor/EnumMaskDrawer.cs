using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Common.Utils
{
    [CustomPropertyDrawer(typeof(EnumMask))]
    public class EnumMaskDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;

            var propertyValue = _property.intValue;
            var mask = attribute as EnumMask;

            position.width /= 2;
            EditorGUI.LabelField(position, label);

            position.x += position.width;
            
            var menu = new GenericMenu();
            
            string[] names = mask.GetEnumValueNames();
            int[] values = mask.GetEnumValues();

            for (var i = 0; i < names.Length; i++) {
                string name = names[i];
                menu.AddItem(new GUIContent(name), (values[i] & propertyValue) > 0, OnMenuItemClick, values[i]);
            }

            if (EditorGUI.DropdownButton(position, new GUIContent("Select Filter"), FocusType.Passive)) {
                menu.ShowAsContext();
            }
        }

        private void OnMenuItemClick(object value)
        {
            var intValue = (int) value;
            if (intValue == 0)
                _property.intValue = 0;
            else
                _property.intValue ^= intValue;
            _property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }
}