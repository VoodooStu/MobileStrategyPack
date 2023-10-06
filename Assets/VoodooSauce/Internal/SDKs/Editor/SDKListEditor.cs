using UnityEditor;

namespace Voodoo.Sauce.Internal.SDKs
{
    public static class SDKListEditor
    {

        [MenuItem("VoodooSauce/Open 3rd Party SDK list")]
        private static void Open3rdPartySDKsPage()
        {
            SDKListGenerator.GenerateSDKList();
            VoodooSauceSDKs sdks = VoodooSauceSDKs.Load();
            Selection.activeObject = sdks;
        }
    }
}