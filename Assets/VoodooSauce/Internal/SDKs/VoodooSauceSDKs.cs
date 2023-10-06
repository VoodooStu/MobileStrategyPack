using UnityEngine;

namespace Voodoo.Sauce.Internal.SDKs
{
    [CreateAssetMenu(fileName = "Assets/VoodooSauce/SDKs/Resources", menuName = "VoodooSauce/VoodooSauceSDKs", order = 0)]
    public class VoodooSauceSDKs : ScriptableObject
    {
        public const string NAME = "VoodooSauceSDKs";
        public const string FOLDER = "Assets/Resources/";
        public const string PATH = FOLDER + "VoodooSauceSDKs.asset";

        [ReadOnly]
        public SDKList SDKs;
        
        public static VoodooSauceSDKs Load() => Resources.Load<VoodooSauceSDKs>(NAME);
    }
}