using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class ABTestClient : AbstractClient, IABTestClient
	{
		public ICohortClient Cohort { get; }
		
		public ABTestClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "/{1}/ab-tests";
			Header = header;
			Cohort = new CohortClient(BaseURL, Header);
		}

		public async Task<IReadOnlyList<ABTest>> GetAll(string appId, string versionReference, string layerId)
		{
			string url = string.Format(BaseURL, appId, versionReference) + "?layerId=" + layerId;
			return await VoodooTuneRequest.GetAsync<IReadOnlyList<ABTest>>(url, Header);
		}

		public async Task<IReadOnlyList<ABTest>> GetAll(string appId, string versionReference, List<ABTestState> states = null)
		{
			string url = string.Format(BaseURL, appId, versionReference);
			
			if (states != null)
			{
				for (var i = 0; i < states.Count; i++)
				{
					url += i == 0 ? "?" : "&";
					url += "state[]=" + states[i];
				}
			}
			
			return await VoodooTuneRequest.GetAsync<IReadOnlyList<ABTest>>(url, Header);
		}

		public async Task<ABTest> Get(string appId, string versionReference, string abTestId)
		{
			string url = string.Format(BaseURL, appId, versionReference) + "/" + abTestId;
			return await VoodooTuneRequest.GetAsync<ABTest>(url, Header);
		}

		public async Task<ABTestResponse> Create(string appId, NewABTest abTest)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip");
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(abTest, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PostAsync<ABTestResponse>(url, content, Header);
#else
			return await Task.FromResult<ABTestResponse>(null);
#endif
		}

		public async Task<string> Duplicate(string appId, string abTestId)
		{
			string url = string.Format(BaseURL, appId, "wip") + "/duplicate";
			string content = string.Concat("{\"abTestId\":\"", abTestId, "\"}");
			
			return await VoodooTuneRequest.PostAsync<string>(url, content, Header);
		}

		public async Task<ABTestResponse> Update(string appId, string abTestId, NewABTest abTest)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip") + "/" + abTestId;
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(abTest, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PutAsync<ABTestResponse>(url, content, Header);
#else
			return await Task.FromResult<ABTestResponse>(null);
#endif
		}

		public async Task<ABTestResponse> UpdateStatus(string appId, string abTestId, ABTestState state)
		{
			string url = string.Format(BaseURL, appId, "wip") + "/" + abTestId + "/state";
			string content = string.Concat("{\"state\":\"", state.ToString(), "\"}");
			
			return await VoodooTuneRequest.PutAsync<ABTestResponse>(url, content, Header);
		}
	}
}