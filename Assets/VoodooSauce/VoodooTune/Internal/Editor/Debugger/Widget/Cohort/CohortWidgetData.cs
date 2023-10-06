using System.Collections.Generic;
using System.Linq;
using Voodoo.Tune.Core;

namespace Voodoo.Tune.Debugger.Editor
{
	public class CohortWidgetData
	{
		public ABTest abTest;
		public int abTestIndex;
		
		public Cohort cohort;
		public int cohortIndex;
		
		public string[] CohortDisplayOptions;
		
		public CohortWidgetData(CohortWidgetData other)
		{
			abTest = other.abTest;
			abTestIndex = other.abTestIndex;
			
			cohort = other.cohort;
			cohortIndex = other.cohortIndex;
			
			CohortDisplayOptions = other.CohortDisplayOptions;
		}
		
		public CohortWidgetData(ABTest abTest, Cohort cohort, int abTestIndex, int cohortIndex)
		{
			this.abTest = abTest;
			this.cohort = cohort;
			
			this.abTestIndex = abTestIndex;
			this.cohortIndex = cohortIndex;

			CohortDisplayOptions = abTest == null ? new[] { "-" } : abTest.Cohorts.Select(x => x.Name.Replace("/", "_")).ToArray();
		}

		public void UpdateABTest(List<ABTest> abtests)
		{
			abTest = abtests[abTestIndex];
			
			cohortIndex = 0;
			cohort = abTest?.Cohorts[cohortIndex];
			CohortDisplayOptions = abTest == null ? new[] { "-" } : abTest.Cohorts.Select(x => x.Name.Replace("/", "_")).ToArray();
		}

		public void UpdateCohort()
		{
			cohort = abTest.Cohorts[cohortIndex];
		}
	}
}