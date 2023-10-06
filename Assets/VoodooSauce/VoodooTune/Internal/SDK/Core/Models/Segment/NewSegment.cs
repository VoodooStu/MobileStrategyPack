using System.Collections.Generic;
#pragma warning disable 414

namespace Voodoo.Tune.Core
{
	public class NewSegment
	{
		public string name;
		public string description;
		public List<Condition> conditions;
		
		private bool system = false;
		
		public class Condition
		{
			//if type == TEST then there are no more conditions
			//if type == AND or type == OR then there are no operator, featureId and testValue
			public ConditionType type;

#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonProperty("operator", NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
			public Operator curOperator;
		
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
			public string featureId;
			
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
			public object testValue;
			
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
			public List<Condition> conditions;
		}
	}
}