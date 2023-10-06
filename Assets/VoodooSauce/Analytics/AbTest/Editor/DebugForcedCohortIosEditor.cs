using UnityEditor;
 
 namespace Voodoo.Sauce.Internal.Analytics.Editor
 {
     [CustomPropertyDrawer(typeof(DebugForcedCohortIos))]
     internal class DebugForcedCohortIosEditor : DebugForcedCohortEditor
     {
         internal override string[] RunningAbTests() => _settings.RunningIosABTests;
     }
 }