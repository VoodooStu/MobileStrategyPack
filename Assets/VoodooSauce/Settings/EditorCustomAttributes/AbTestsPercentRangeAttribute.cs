using System;
using UnityEngine;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.EditorCustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class AbTestsPercentRangeAttribute : PropertyAttribute
    {
        private readonly VoodooSettings setting;
        private readonly RuntimePlatform platform;

        public const float min = 0.001f;
        public float max => platform == RuntimePlatform.Android ? setting.MaxPercentOfTotalAndroidCohorts : setting.MaxPercentOfTotalIosCohorts;
        public string NewName { get ; private set; }
        
        public AbTestsPercentRangeAttribute(RuntimePlatform runtimePlatform,string newName = null)
        {
            setting = VoodooSettings.Load();
            platform = runtimePlatform;
            NewName = newName;
        }
    }
}