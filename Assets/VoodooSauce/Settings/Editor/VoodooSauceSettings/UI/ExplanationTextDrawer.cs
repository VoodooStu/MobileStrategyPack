using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(ExplanationTextAttribute))]
    public class ExplanationTextDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var explanationTextAttribute = attribute as ExplanationTextAttribute;
            string text = explanationTextAttribute?.Text;
            Draw(text);
        }

        public static void Draw(string text)
        {
            SmallSpacerDrawer.Draw();
            
            Color color = GUI.contentColor;
            GUI.contentColor = new Color(0.7f, 0.7f, 0.7f, 0.85f);
            EditorGUILayout.LabelField(text, new GUIStyle(GUI.skin.label) {
                fontStyle = FontStyle.Italic,
                wordWrap = true,
            });
            GUI.contentColor = color;
        }
    }
}