using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class SegmentClient : AbstractClient, ISegmentClient
	{
		public SegmentClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "/{1}/segments";
			Header = header;
		}
		
		public async Task<IReadOnlyList<Segment>> GetAll(string appId, string versionReference)
		{
			string url = string.Format(BaseURL, appId, versionReference);
			return await VoodooTuneRequest.GetAsync<List<Segment>>(url, Header);
		}

		public async Task<Segment> Get(string appId, string versionReference, string segmentId)
		{
			string url = string.Format(BaseURL, appId, versionReference) + "/" + segmentId;
			return await VoodooTuneRequest.GetAsync<Segment>(url, Header);
		}

		public async Task<string> Create(string appId, NewSegment segment)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip");
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(segment, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PostAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}

		public async Task<string> Update(string appId, string segmentId, NewSegment segment)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip") + "/" + segmentId;
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(segment, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PutAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}
	}
}