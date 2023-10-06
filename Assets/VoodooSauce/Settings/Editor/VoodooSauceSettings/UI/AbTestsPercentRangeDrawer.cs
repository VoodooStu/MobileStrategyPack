using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Internal.EditorCustomAttributes;

namespace Voodoo.Sauce.Privacy
{
    [CustomPropertyDrawer(typeof(AbTestsPercentRangeAttribute))]
    public class AbTestsPercentRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rangeAttribute = (AbTestsPercentRangeAttribute) attribute;
            switch (property.propertyType) {
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Clamp(property.floatValue, AbTestsPercentRangeAttribute.min,
                        rangeAttribute.max);
                    EditorGUI.Slider(position, property, AbTestsPercentRangeAttribute.min,
                       rangeAttribute.max, (rangeAttribute.NewName == null) ? label : new GUIContent(rangeAttribute.NewName));
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = Mathf.Clamp(property.intValue, (int)AbTestsPercentRangeAttribute.min,
                        (int)rangeAttribute.max);
                    EditorGUI.IntSlider(position, property, (int) AbTestsPercentRangeAttribute.min,
                       (int) rangeAttribute.max, (rangeAttribute.NewName == null) ? label : new GUIContent(rangeAttribute.NewName));
                    break;
                default:
                    EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
                    break;
            }
        }
    }
}