using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(SubHeaderAttribute))]
    public class SubHeaderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var headerStyle = new GUIStyle {
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                normal = {textColor = Color.grey},
                alignment = TextAnchor.UpperCenter,
                wordWrap = true
            };

            GUI.Label(position, (attribute as SubHeaderAttribute)?.header, headerStyle);
        }

        public override float GetHeight() => 24;
    }
}