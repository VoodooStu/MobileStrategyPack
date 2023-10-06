using System;
using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	[Serializable]
	public class AppAPI
	{
		public bool Success { get; protected set; }
		public List<App> Apps { get; protected set; }

		#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public AppAPI(bool success, List<App> apps)
		{
			Success = success;
			Apps = apps;
		}
	}
	
	public class App
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }
		public string IOSBundleId { get; protected set; }
		public string AndroidBundleId { get; protected set; }
		public string IOSChinaBundleId { get; protected set; }
		public string IconUrl { get; protected set; }
		public bool NewSegmentation { get; protected set; }
		public bool FeatureStore { get; protected set; }
		public List<string> SubAppIds { get; protected set; }
		public DateTime? CreatedAt { get; protected set; }
		public DateTime? DeletedAt { get; protected set; }

		#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public App(string id, string name, string iosBundleId, string androidBundleId, string iosChinaBundleId, string iconUrl, bool newSegmentation, bool featureStore, List<string> subAppIds, DateTime? createdAt, DateTime? deletedAt)
		{
			Id = id;
			Name = name;
			IOSBundleId = iosBundleId;
			AndroidBundleId = androidBundleId;
			IOSChinaBundleId = iosChinaBundleId;
			IconUrl = iconUrl;
			NewSegmentation = newSegmentation;
			FeatureStore = featureStore;
			SubAppIds = subAppIds;
			CreatedAt = createdAt;
			DeletedAt = deletedAt;
		}
	}
}