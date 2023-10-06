using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(MediumSpacerAttribute))]
    public class MediumSpacerDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            Draw();
        }

        public static void Draw()
        {
            GUILayout.Space(MediumSpacerAttribute.SIZE);
        }

        public override float GetHeight() => MediumSpacerAttribute.SIZE;
    }
}