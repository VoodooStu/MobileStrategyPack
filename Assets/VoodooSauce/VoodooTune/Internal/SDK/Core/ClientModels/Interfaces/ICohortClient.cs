using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface ICohortClient 
	{
		Task<Cohort> Get(string appId, string versionReference, string abTestId, string cohortId);
		Task<CohortResponse> Create(string appId, string abTestId, NewCohort cohort);
		Task<string> Update(string appId, string abTestId, string cohortId, NewCohort cohort);
		Task<string> UpdateAllocationRates(string appId, string abTestId, Dictionary<string,int> cohortIdToAllocationRate);
	}
}