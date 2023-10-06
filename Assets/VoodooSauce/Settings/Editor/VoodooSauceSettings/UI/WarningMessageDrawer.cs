using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [CustomPropertyDrawer(typeof(WarningMessageAttribute))]
    public class WarningMessageDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            EditorGUILayout.HelpBox((attribute as WarningMessageAttribute)?.message, MessageType.Warning);
        }
    }
}