using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(CustomLabelAndValueAttribute))]
    public class CustomLabelAndValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = attribute as CustomLabelAndValueAttribute;
            EditorGUILayout.LabelField(att?.Name, property.stringValue);

            if (att?.Separator ?? false) {
                SeparatorDrawer.Draw();
            }
        }
    }
}