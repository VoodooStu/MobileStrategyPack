using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface IHealthCheckClient 
	{
		Task<HealthCheck> Get();
	}
}