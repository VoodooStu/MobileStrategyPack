using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface ILayerClient 
	{
		Task<IReadOnlyList<Layer>> GetAll(string appId, string versionReference);
		Task<string> Create(string appId, NewLayer layer);
		Task<Layer> Update(string appId, string layerId, NewLayer layer);
	}
}