using System.Collections.Generic;
using System.Threading.Tasks;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public class AppClient : AbstractClient, IAppClient
	{
		public IVersionClient Version { get; }
		public ISandboxClient Sandbox { get; }
		public ICatalogClient Catalog { get; }

		public AppClient(string baseURL, Header header)
		{
			BaseURL = baseURL + "app";
			Header = header;
			Version = new VersionClient(BaseURL, Header);
			Sandbox = new SandboxClient(BaseURL, Header);
			Catalog = new CatalogClient(BaseURL, Header);
		}

		private Platform CurrentPlatform
		{
			get
			{
#if UNITY_EDITOR
				return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ? Platform.android : Platform.ios;
#elif UNITY_ANDROID
                return Platform.android;
#else
                return Platform.ios;
#endif
			}
		}
		
		private string CurrentBundleId
		{
			get
			{
#if UNITY_EDITOR
				return UnityEditor.PlayerSettings.applicationIdentifier;
#else
                return UnityEngine.Application.identifier;
#endif
			}
		}

		/// <summary>
		/// Return all the existing apps.
		/// </summary>
		public async Task<IReadOnlyList<App>> GetAll()
		{
			var appAPI = await VoodooTuneRequest.GetAsync<AppAPI>(BaseURL, Header);

			return appAPI?.Apps;
		}

		/// <summary>
		/// Return the current client app.
		/// The bundleId is retrieved from the project settings
		/// The platform is retrieved from the build settings active build target. iOS by default 
		/// </summary>
		public async Task<string> GetCurrentId()
		{
			return await GetId(CurrentBundleId, CurrentPlatform);
		}
		
		public async Task<string> GetId(string bundleId, Platform platform)
		{
			string url = BaseURL + "/" + bundleId;
			if (platform != Platform.android)
			{
				url += "?force_ios=true";
			}
			
			string appId = await VoodooTuneRequest.GetAsync<string>(url, Header);
			appId = appId?.Replace("\"", "");

			return appId;
		}
	}
}