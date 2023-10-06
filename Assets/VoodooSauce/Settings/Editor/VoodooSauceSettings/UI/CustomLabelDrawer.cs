using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(CustomLabelAttribute))]
    public class CustomLabelDrawer : PropertyDrawer
    {
        private GUIContent _label = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_label == null)
            {
                string name = (attribute as CustomLabelAttribute)?.Name;
                _label = new GUIContent(name);
            }

            EditorGUI.PropertyField(position, property, _label);
        }
    }
}