using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Privacy
{
    [CustomPropertyDrawer(typeof(LargeHeaderAttribute))]
    public class LargeHeaderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var largeHeaderAttribute = attribute as LargeHeaderAttribute;
            string header = largeHeaderAttribute?.header;
            string subHeader = largeHeaderAttribute?.subHeader;
            Draw(subHeader, header);
        }

        public static void Draw(string subHeader, string header)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            var headerStyle = new GUIStyle {
                fontStyle = FontStyle.Bold,
                fontSize = 20,
                normal = {textColor = Color.white},
                alignment = TextAnchor.UpperCenter,
                wordWrap = true
            };
            GUILayout.Space(20);
            GUILayout.Label(header, headerStyle);

            //GUILayout.Space(5); 

            var subHeaderStyle = new GUIStyle {
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                normal = {textColor = Color.grey},
                alignment = TextAnchor.UpperCenter,
                wordWrap = true
            };
            GUILayout.Label(subHeader, subHeaderStyle);
            GUILayout.Space(20);

            GUILayout.EndVertical();
        }

        public override float GetHeight() => 24;
    }
}