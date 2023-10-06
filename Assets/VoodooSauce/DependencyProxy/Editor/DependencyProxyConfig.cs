using System;
using UnityEngine;

namespace Voodoo.Sauce.DependencyProxy.Editor
{
    [Serializable]
    public class DependencyProxyConfig: ScriptableObject
    {
        public bool EnableDependencyProxy;
        public static DependencyProxyConfig Load() => Resources.Load<DependencyProxyConfig>("DependencyProxyConfig");
    }
}