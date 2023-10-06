using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class NewCohort
	{
		public string name = "";
		public string description = "";
		public int allocationRate = 0;
		
		public List<OverrideValue> overrideValues = new List<OverrideValue>();
	
		public class OverrideValue
		{
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonProperty("class")]
#endif
			public string classTechnicalName;
			public Instance instances;

			public class Instance
			{
				public Dictionary<string, Value> instances;
			}

			public class Value
			{
				public Dictionary<string, object> values;
				public bool enabled;
			}
		}
	}
}