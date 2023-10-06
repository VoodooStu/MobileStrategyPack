using System;
using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class NewABTest
	{
		public string name = "";
		public string description = "";
		public int priority = 1;
		public bool immutable = true;
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
		public string layerId;
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
		public DateTime? beginsAt;
		
			
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
		public DateTime? endsAt;
		
		public List<string> segments;
		
		public PlatformVersion platform = new PlatformVersion();
		
		public class PlatformVersion
		{
				
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
			public VersionRange ios = new VersionRange();
			
				
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
			public VersionRange android = new VersionRange();
		}

		public class VersionRange
		{
			public string from = "";
			public string to = "";
		}
	}
}