using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class VersionClient : AbstractClient, IVersionClient
	{
		public IClassClient Class { get; }
		public ISegmentClient Segment { get; }
		public IABTestClient AbTest { get; }
		public ILayerClient Layer { get; }

		public VersionClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "/{0}/version";
			Header = header;
			AbTest = new ABTestClient(BaseURL, Header);
			Segment = new SegmentClient(BaseURL, Header);
			Class = new ClassClient(BaseURL, Header);
			Layer = new LayerClient(BaseURL, Header);
		}

		public async Task<OldVersionMetadata> GetOldMetadata(string appId, string versionId)
		{
			string url = string.Format(BaseURL, appId) + "/" + versionId + "/metadata";
			return await VoodooTuneRequest.GetAsync<OldVersionMetadata>(url, Header);
		}

		public async Task<VersionMetadata> Get(string appId, string versionId)
		{
			string url = string.Format(BaseURL, appId) + "/metadata/" + versionId;
			return await VoodooTuneRequest.GetAsync<VersionMetadata>(url, Header);
		}
		
		public async Task<VersionMetadata> GetWorkInProgress(string appId)
		{
			string url = string.Format(BaseURL, appId) + "/metadata/" + Status.wip;
			return await VoodooTuneRequest.GetAsync<VersionMetadata>(url, Header);
		}

		public async Task<VersionMetadata> GetLive(string appId)
		{
			string url = string.Format(BaseURL, appId) + "/metadata/" + Status.live;
			return await VoodooTuneRequest.GetAsync<VersionMetadata>(url, Header);
		}
		
		public async Task<IReadOnlyList<Version>> GetAll(string appId)
		{
			string url = string.Format(BaseURL, appId) + "?v3=true";
			var versionAPI = await VoodooTuneRequest.GetAsync<VersionAPI>(url, Header);
			
			return versionAPI?.Versions;
		}

		public async Task<VersionResponse> Publish(string appId, string versionName)
		{
			string url = string.Format(BaseURL, appId) + "/publish";
			string content = string.Concat("{\"name\" : \"", versionName, "\"}");
			
			return await VoodooTuneRequest.PostAsync<VersionResponse>(url, content, Header);
		}

		public async Task<string> Reset(string appId, string versionId)
		{
			string url = string.Format(BaseURL, appId) + "/reset";
			string content = string.Concat("{\"versionId\" : \"", versionId, "\"}");
			
			return await VoodooTuneRequest.PostAsync<string>(url, content, Header);
		}

		public async Task<Version> Rollback(string appId, string versionId)
		{
			string url = string.Format(BaseURL, appId) + "/rollback";
			string content = string.Concat("{\"versionId\" : \"", versionId, "\"}");
			
			return await VoodooTuneRequest.PostAsync<Version>(url, content, Header);
		}
	}
}