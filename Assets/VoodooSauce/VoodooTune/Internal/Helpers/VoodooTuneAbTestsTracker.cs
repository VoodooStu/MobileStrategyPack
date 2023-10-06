using System;
using System.Collections.Generic;
using System.Linq;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Internal.VoodooTune
{
	internal class VoodooTuneAbTestsTracker
	{
		private const string TAG = "VoodooTuneAbTestsTracker";

		private readonly List<string> _abTests;
		private readonly List<string> _cohorts;
		private readonly string _version;

		// Keep track of the saved AB tests (loaded from the VT cache) to compare them later
		// with the up-to-date AB tests (loaded from the VT request).
		internal VoodooTuneAbTestsTracker()
		{
			_abTests = VoodooTuneManager.GetAbTestUuidsAsList();
			_cohorts = VoodooTuneManager.GetAbTestCohortUuidsAsList();
			_version = VoodooTuneManager.GetAbTestVersionUuid();
		}

		public void TrackAbTestModifications(Voodoo.Tune.Core.VoodooConfig config)
		{
			TrackAbTestModifications(config.AbTestIdsToList, config.CohortIdsToList, config.VersionNumber);
		}
		
		private void TrackAbTestModifications(List<string> abTestUuids, List<string> abTestCohortUuids, string abTestVersionUuid)
		{
			if (abTestUuids.SequenceEqual(_abTests) && abTestCohortUuids.SequenceEqual(_cohorts))
			{
				return;
			}
			
			VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, "Tracking A/B Tests modifications");

			(List<VoodooTuneAbTestAnalyticsInfo> assignments, List<VoodooTuneAbTestAnalyticsInfo> exits) =
				GetAssignmentsAndExits(_abTests, _cohorts, abTestUuids, abTestCohortUuids, _version, abTestVersionUuid);

			foreach (VoodooTuneAbTestAnalyticsInfo info in assignments)
			{
				if (string.IsNullOrEmpty(info.AbTestUuid) || string.IsNullOrEmpty(info.AbTestCohortUuid) || string.IsNullOrEmpty(info.AbTestVersionUuid))
				{
					continue;
				}
				
				AnalyticsManager.TrackVoodooTuneAbTestAssignment(info);
			}

			foreach (VoodooTuneAbTestAnalyticsInfo info in exits)
			{
				if (string.IsNullOrEmpty(info.AbTestUuid) || string.IsNullOrEmpty(info.AbTestCohortUuid) || string.IsNullOrEmpty(info.AbTestVersionUuid))
				{
					continue;
				}
				
				AnalyticsManager.TrackVoodooTuneAbTestExit(info);
			}
		}

		internal static Tuple<List<VoodooTuneAbTestAnalyticsInfo>, List<VoodooTuneAbTestAnalyticsInfo>> GetAssignmentsAndExits(
			List<string> oldAbTestUuids, List<string> oldAbTestCohortUuids,
			List<string> abTestUuids, List<string> abTestCohortUuids,
			string oldAbTestVersionUuid, string abTestVersionUuid)
		{
			try
			{
				List<VoodooTuneAbTestAnalyticsInfo> oldAbTests = GetAbTestsAnalyticsInfo(oldAbTestUuids, oldAbTestCohortUuids, oldAbTestVersionUuid);
				List<VoodooTuneAbTestAnalyticsInfo> newAbTests = GetAbTestsAnalyticsInfo(abTestUuids, abTestCohortUuids, abTestVersionUuid);

				List<VoodooTuneAbTestAnalyticsInfo> assignments = newAbTests.Except(oldAbTests).ToList();
				List<VoodooTuneAbTestAnalyticsInfo> exits = oldAbTests.Except(newAbTests).ToList();
				return new Tuple<List<VoodooTuneAbTestAnalyticsInfo>, List<VoodooTuneAbTestAnalyticsInfo>>(assignments, exits);
			}
			catch
			{
				VoodooLog.LogWarning(Module.VOODOO_TUNE, TAG, $"AB tests are misconfigured: get {oldAbTestUuids.Count} saved AB test UUIDs, {oldAbTestCohortUuids.Count} saved cohort UUIDs and get {abTestUuids.Count} AB test UUIDs, {abTestCohortUuids.Count} cohort UUIDs");
				return new Tuple<List<VoodooTuneAbTestAnalyticsInfo>, List<VoodooTuneAbTestAnalyticsInfo>>(new List<VoodooTuneAbTestAnalyticsInfo>(), new List<VoodooTuneAbTestAnalyticsInfo>());
			}
		}

		private static List<VoodooTuneAbTestAnalyticsInfo> GetAbTestsAnalyticsInfo(IReadOnlyList<string> abTests, IReadOnlyList<string> cohorts, string version)
		{
			var analyticsInfos = new List<VoodooTuneAbTestAnalyticsInfo>();

			if (abTests == null || cohorts == null)
			{
				return analyticsInfos;
			}

			if (abTests.Count != cohorts.Count)
			{
				throw new Exception($"AB tests are misconfigured: get {abTests.Count} AB test UUIDs, {cohorts.Count} cohort UUIDs");
			}

			for (var i = 0; i < abTests.Count; i++)
			{
				var current = new VoodooTuneAbTestAnalyticsInfo(abTests[i], cohorts[i], version);
				analyticsInfos.Add(current);
			}

			return analyticsInfos;
		}
	}
}