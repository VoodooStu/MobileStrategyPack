using mixpanel;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class MixpanelPostBuild
    {
        [PostProcessBuild]
        private static void PostBuild(BuildTarget target, string pathToBuildProject)
        {
            MixpanelSettings.Instance.ManualInitialization = true; 
        }
                
    }
}