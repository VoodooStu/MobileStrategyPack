using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface IAppClient
	{
		IVersionClient Version { get; }
		ISandboxClient Sandbox { get; }
		ICatalogClient Catalog { get; }
		
		Task<string> GetCurrentId();
		Task<string> GetId(string bundleId, Platform platform);
		Task<IReadOnlyList<App>> GetAll();
	}
}