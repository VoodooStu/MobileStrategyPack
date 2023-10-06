using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class VoodooTuneClient
	{
		private const string BaseApiUrl = "https://voodootune.voodoo-{0}.io/v2/api/";
		
		//TODO be remove very soon when Auth0 is released (MVP is actually working)
		private const string tokenTech = "cba946f3-5a9d-fd05-4b66-67166a3716e1";
		private const string tokenStaging = "5dcbd1a3-98fd-d282-46bd-f89535db3713";
		private const string tokenDev = "1e2a2430-a516-284a-c05a-cacf2a020432";
		
		public IAppClient App { get; }

		public ICatalogClient Catalog { get; }

		public IHealthCheckClient ServerStatus { get; }

		private string URL { get; set; }
		private Header Header { get; set; }

		public readonly Server CurrentServer;

		public VoodooTuneClient() : this(VoodooTunePersistentData.SavedServer) { }

		public VoodooTuneClient(Server server)
		{
			CurrentServer = server;
			Initialize(CurrentServer);
			
			App = new AppClient(URL, Header);
			Catalog = new CatalogClient(URL, Header);
			ServerStatus = new HealthCheckClient(URL, Header);
		}

		private void Initialize(Server server)
		{
			SetUrls(server);
			SetHeader(server);
		}

		private void SetUrls(Server server)
		{
			URL = string.Format(BaseApiUrl, server);
		}

		private void SetHeader(Server server)
		{
			string token;
			switch (server)
			{
				case Server.tech:
					token = tokenTech;
					break;
				case Server.staging:
					token = tokenStaging;
					break;
				case Server.dev:
					token = tokenDev;
					break;
				default:
					token = "";
					break;
			}

			Header = new Header("X-API-KEY", token);
		}
	}
}