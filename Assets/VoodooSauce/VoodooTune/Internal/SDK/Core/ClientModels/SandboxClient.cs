using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class SandboxClient : AbstractClient, ISandboxClient
	{
		public SandboxClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "/{0}/sandboxes";
			Header = header;
		}

		public async Task<IReadOnlyList<Sandbox>> GetAll(string appId)
		{
			string url = string.Format(BaseURL, appId);
			return await VoodooTuneRequest.GetAsync<IReadOnlyList<Sandbox>>(url, Header);
		}

		public async Task<Sandbox> Get(string appId, string sandboxId)
		{
			string url = string.Format(BaseURL, appId) + "/" + sandboxId;
			return await VoodooTuneRequest.GetAsync<Sandbox>(url, Header);
		}

		public async Task<string> Create(string appId, NewSandbox sandbox)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId);
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(sandbox, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PostAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}

		public async Task<string> Update(string appId, string sandboxId, NewSandbox sandbox)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId) + "/" + sandboxId;
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(sandbox, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PutAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}
	}
}