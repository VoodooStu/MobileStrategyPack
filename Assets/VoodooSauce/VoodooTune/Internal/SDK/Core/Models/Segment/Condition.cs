using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class Condition
	{
		public ConditionType Type { get; protected set; }
		
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
		public Operator Operator { get; protected set; }
		
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
		public string FeatureId { get; protected set; }
		
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
		public object TestValue { get; protected set; }
		
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
#endif
		public List<Condition> Conditions { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public Condition(ConditionType type, Operator @operator, string featureId, object testValue, List<Condition> conditions)
		{
			Type = type;
			Operator = @operator;
			FeatureId = featureId;
			TestValue = testValue;
			Conditions = conditions;
		}
	}
}