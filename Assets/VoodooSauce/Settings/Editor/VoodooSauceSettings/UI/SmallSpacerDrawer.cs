using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(SmallSpacerAttribute))]
    public class SmallSpacerDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            Draw();
        }

        public static void Draw()
        {
            GUILayout.Space(SmallSpacerAttribute.SIZE);
        }
    }
}