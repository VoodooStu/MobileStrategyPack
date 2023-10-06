using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class CatalogClient : AbstractClient, ICatalogClient
	{
		public CatalogClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "classes";
			Header = header;
		}
		
		public async Task<IReadOnlyList<ClassInfo>> GetAll(string bundleId = null)
		{
			string url = BaseURL;
			if (bundleId != null)
			{
				url += $"?bundleId={bundleId}";
			}
			
			return await VoodooTuneRequest.GetAsync<List<ClassInfo>>(url, Header);
		}

		public async Task<ClassInfo> Get(Type classType)
		{
			string url = BaseURL + "/" + classType.FullName;
			return await VoodooTuneRequest.GetAsync<ClassInfo>(url, Header);
		}

		public async Task<string> Create(NewClassInfo classInfo)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL);
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(classInfo, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PostAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}

		public async Task<string> Update(NewClassInfo classInfo)
		{
#if NEWTONSOFT_JSON
			string url = string.Format(BaseURL) + "/" + classInfo.technicalName;
			string content = Newtonsoft.Json.JsonConvert.SerializeObject(classInfo, VoodooTuneRequest.SerializerSettings);
			
			return await VoodooTuneRequest.PutAsync<string>(url, content, Header);
#else
			return await Task.FromResult<string>(null);
#endif
		}
	}
}