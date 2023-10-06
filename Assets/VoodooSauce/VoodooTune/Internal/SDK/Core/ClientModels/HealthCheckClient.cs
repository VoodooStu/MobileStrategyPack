using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class HealthCheckClient : AbstractClient, IHealthCheckClient
	{
		public HealthCheckClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "health-check";
			Header = header;
		}

		public async Task<HealthCheck> Get()
		{
			HealthCheck healthCheck = await VoodooTuneRequest.GetAsync<HealthCheck>(BaseURL, Header);
			return healthCheck ?? HealthCheck.Default;
		}
	}
}