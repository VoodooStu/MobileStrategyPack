using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voodoo.Tune.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.VoodooTune
{
    internal class DebugVTConfiguration
    {
        private const string TAG = "DebugVTConfiguration";

        internal Sandbox SelectedSandbox { get; set; }
        internal List<Segment> SelectedSegments { get; set; } = new List<Segment>();
        internal List<ABTest> SelectedAbTests { get; set; } = new List<ABTest>();
        internal List<Cohort> SelectedCohorts { get; set; } = new List<Cohort>();
        internal string VersionId { get; set; }

        internal bool IsDefaultConfiguration => SelectedSandbox == null &&
                                                SelectedCohorts.Count == 0 &&
                                                SelectedAbTests.Count == 0 &&
                                                SelectedSegments.Count == 0 &&
                                                string.IsNullOrEmpty(VersionId);

        internal bool IsSandbox => SelectedSandbox != null &&
                                   SelectedCohorts.Count == 0 &&
                                   SelectedAbTests.Count == 0 &&
                                   SelectedSegments.Count == 0 &&
                                   string.IsNullOrEmpty(VersionId);

        internal bool IsSimulation => SelectedSandbox == null
                                      && ((SelectedCohorts.Count > 0
                                           && SelectedAbTests.Count > 0 && SelectedCohorts.Count == SelectedAbTests.Count)
                                          || SelectedSegments.Count > 0)
                                      && !string.IsNullOrEmpty(VersionId);
        
        internal bool HasNoneSelected => SelectedAbTests.Count == 0
            && SelectedCohorts.Count == 0
            && SelectedSegments.Count == 0;

        internal string SelectedAbTestsToString => "[" + string.Join(",", SelectedAbTests.Select(x => x.Name)) + "]";
        internal string SelectedCohortsToString => "[" + string.Join(",", SelectedCohorts.Select(x => x.Name)) + "]";
        internal string SelectedSegmentsToString => "[" + string.Join(",", SelectedSegments.Select(x => x.Name)) + "]";
        internal string SelectedABTestsAndCohortsToString
        {
            get
            {
                string res = "";
                for (int i = 0; i < SelectedAbTests.Count; i++)
                {
                    res += $"{SelectedAbTests[i].Name} - {SelectedCohorts[i].Name}\n";
                }
                return res;
            }
        }

        /// <summary>
        /// Check if the debug configuration is ready to be applied.
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise.</returns>
        internal bool IsValid => IsDefaultConfiguration || IsSandbox || IsSimulation;
        
        public override bool Equals(object obj)
        {
            if (obj is DebugVTConfiguration other)
            {
                bool isEqualSandbox = Equals(other.SelectedSandbox, SelectedSandbox);
                bool isEqualVersion = (other.VersionId ?? "") == (VersionId ?? "");
                bool isEqualAbTests = SelectedAbTests == null && other.SelectedAbTests == null || SelectedAbTests != null && SelectedAbTests.SequenceEqual(other.SelectedAbTests);
                bool isEqualCohorts = SelectedCohorts == null && other.SelectedCohorts == null || SelectedCohorts != null && SelectedCohorts.SequenceEqual(other.SelectedCohorts);
                bool isEqualSegments = SelectedSegments == null && other.SelectedSegments == null || SelectedSegments != null && SelectedSegments.SequenceEqual(other.SelectedSegments);
                
                return isEqualSandbox && isEqualVersion && isEqualAbTests && isEqualCohorts && isEqualSegments;
            }

            return false;
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => SelectedSandbox.GetHashCode();

        /// <summary>
        /// Save this <see cref="DebugVTConfiguration"/> in the <see cref="PlayerPrefs"/>.
        /// This save is processed only if <see cref="IsValid"/> returns true.
        /// </summary>
        internal void PersistOnPlayerPrefs()
        {
            if (IsDefaultConfiguration)
            {
                VoodooTunePersistentData.Clear();
                VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, "Clear VT debug configuration");
            }
            else if (IsSandbox)
            {
                VoodooTunePersistentData.Save(SelectedSandbox);
                VoodooTunePersistentData.Clear(VoodooTuneDataType.Cohort | VoodooTuneDataType.Segment | VoodooTuneDataType.Version);

                VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, $"Saved VT debug configuration - Sandbox '{SelectedSandbox.Name}'");
            }
            else if (IsSimulation)
            {
                VoodooTunePersistentData.Clear(VoodooTuneDataType.Sandbox);
                if (SelectedCohorts.Count > 0)
                {
                    VoodooTunePersistentData.Save(SelectedAbTests, SelectedCohorts);
                }
                else
                {
                    VoodooTunePersistentData.Clear(VoodooTuneDataType.Cohort);
                }
                
                if (SelectedSegments.Count > 0)
                {
                    VoodooTunePersistentData.Save(SelectedSegments);
                }
                else
                {
                    VoodooTunePersistentData.Clear(VoodooTuneDataType.Segment);
                }
                
                VoodooTunePersistentData.SavedVersionId = VersionId;

                VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, $"Saved VT debug configuration - Version '{VersionId}'" + 
                                                                      (SelectedCohorts.Count > 0 ? $" - Cohorts '{SelectedCohorts}'" : "") +
                                                                      (SelectedSegments.Count > 0 ? $" - Segments '{SelectedSegments}'" : ""));
            }
        }

        /// <summary>
        /// Retrieve the <see cref="DebugVTConfiguration"/> from the <see cref="PlayerPrefs"/>.
        /// </summary>
        /// <returns>The configuration found in the local storage of the game.</returns>
        internal static DebugVTConfiguration GetFromPlayerPrefs()
        {
            //Force reload
            VoodooTunePersistentData.Load();
            
            return new DebugVTConfiguration
            {
                SelectedSandbox = VoodooTunePersistentData.SavedSandbox,
                SelectedAbTests = VoodooTunePersistentData.SavedABTests,
                SelectedCohorts = VoodooTunePersistentData.SavedCohorts,
                SelectedSegments = VoodooTunePersistentData.SavedSegments,
                VersionId = VoodooTunePersistentData.SavedVersionId
            };;
        }
    }
}