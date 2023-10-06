using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class CohortClient : AbstractClient, ICohortClient
	{
		public CohortClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "/{2}/cohorts";
			Header = header;
		}
		
		public async Task<Cohort> Get(string appId, string versionReference, string abTestId, string cohortId)
		{
			string url = string.Format(BaseURL, appId, versionReference, abTestId) + "/" + cohortId;
			return await VoodooTuneRequest.GetAsync<Cohort>(url, Header);
		}

		public async Task<CohortResponse> Create(string appId, string abTestId, NewCohort cohort)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip", abTestId);
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(cohort, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PostAsync<CohortResponse>(url, content, Header);
#else
			return await Task.FromResult<CohortResponse>(null);
#endif
		}

		public async Task<string> Update(string appId, string abTestId, string cohortId, NewCohort cohort)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip", abTestId) + "/" + cohortId;
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(cohort, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PutAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}

		public async Task<string> UpdateAllocationRates(string appId, string abTestId, Dictionary<string, int> cohortIdToAllocationRate)
		{
			string url = string.Format(BaseURL, appId, "wip", abTestId) + "/allocationRates";
			string content = string.Join(",", cohortIdToAllocationRate.Select(kvp => kvp.Key + ":" + kvp.Value).ToString());

			return await VoodooTuneRequest.PutAsync<string>(url, string.Concat("{", content, "}"), Header);
		}
	}
}