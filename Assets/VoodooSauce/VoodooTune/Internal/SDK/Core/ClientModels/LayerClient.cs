using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class LayerClient : AbstractClient, ILayerClient
	{
		public LayerClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "/{1}/layers";
			Header = header;
		}
		
		public async Task<IReadOnlyList<Layer>> GetAll(string appId, string versionReference)
		{
			string url = string.Format(BaseURL, appId, versionReference);
			return await VoodooTuneRequest.GetAsync<List<Layer>>(url, Header);
		}

		/// <summary>
		/// Returns the newly created layer id
		/// </summary>
		public async Task<string> Create(string appId, NewLayer layer)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip");
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(layer, VoodooTuneRequest.SerializerSettings);

			return await VoodooTuneRequest.PostAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}

		/// <summary>
		/// Returns the updated layer
		/// </summary>
		public async Task<Layer> Update(string appId, string layerId, NewLayer layer)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip") + "/" + layerId;
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(layer, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PutAsync<Layer>(url, content, Header);
#else
			return await Task.FromResult<Layer>(null);
#endif
		}
	}
}