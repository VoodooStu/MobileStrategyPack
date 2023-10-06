using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface ISandboxClient 
	{
		Task<IReadOnlyList<Sandbox>> GetAll(string appId);
		Task<Sandbox> Get(string appId, string sandboxId);
		Task<string> Create(string appId, NewSandbox sandbox);
		Task<string> Update(string appId, string sandboxId, NewSandbox sandbox);
	}
}