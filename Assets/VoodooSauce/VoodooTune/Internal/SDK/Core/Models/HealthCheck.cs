using System;

namespace Voodoo.Tune.Core
{
	[Serializable]
	public class HealthCheck
	{
		public bool IsAlive { get; protected set; }
		public string Version { get; protected set; }
		
		public static HealthCheck Default => new HealthCheck(false, "");
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public HealthCheck(bool success, string version)
		{
			IsAlive = success;
			Version = version;
		}
	}
}