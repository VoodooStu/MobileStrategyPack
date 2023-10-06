using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    public class SeparatorDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            Draw();
        }

        public static void Draw()
        {
            MediumSpacerDrawer.Draw();
            
            Rect rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.width+12, rect.y));
            EditorGUILayout.EndHorizontal();
            
            SmallSpacerDrawer.Draw();
        }
    }
}