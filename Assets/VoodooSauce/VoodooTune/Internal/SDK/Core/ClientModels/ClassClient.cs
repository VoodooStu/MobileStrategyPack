using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class ClassClient : AbstractClient, IClassClient
	{
		public ClassClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "/{1}/classes";
			Header = header;
		}
		
		public async Task<IReadOnlyList<ClassInfo>> GetAll(string appId, string versionReference)
		{
			string url = string.Format(BaseURL, appId, versionReference);
			return await VoodooTuneRequest.GetAsync<List<ClassInfo>>(url, Header);
		}

		public async Task<ClassInfo> Get(string appId, string versionReference, Type classType)
		{
			string url = string.Format(BaseURL, appId, versionReference) + "/" + classType.FullName;
			return await VoodooTuneRequest.GetAsync<ClassInfo>(url, Header);
		}

		public async Task<string> Create(string appId, NewClassInfo classInfo)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip");
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(classInfo, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PostAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}

		public async Task<string> Update(string appId, NewClassInfo classInfo)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL, appId, "wip") + "/" + classInfo.technicalName;
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(classInfo, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PutAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}
	}
}