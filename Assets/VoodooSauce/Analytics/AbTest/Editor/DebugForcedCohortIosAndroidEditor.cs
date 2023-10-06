using UnityEditor;

namespace Voodoo.Sauce.Internal.Analytics.Editor
{
    [CustomPropertyDrawer(typeof(DebugForcedCohortAndroid))]
    internal class DebugForcedCohortAndroidEditor : DebugForcedCohortEditor
    {
        internal override string[] RunningAbTests() => _settings.RunningAndroidABTests;
    }
}