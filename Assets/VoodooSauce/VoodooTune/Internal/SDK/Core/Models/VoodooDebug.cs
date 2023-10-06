using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class VoodooDebug
	{
		public readonly bool Authorized;
		public readonly string ClusterId;
		public readonly string UserId;
		public readonly Dictionary<string, Explanation> Explain;

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public VoodooDebug(bool authorized, string clusterId, string userId, Dictionary<string, Explanation> explain)
		{
			Authorized = authorized;
			ClusterId = clusterId;
			UserId = userId;
			Explain = explain;
		}

		public override string ToString()
		{
			return $"authorized: {Authorized}, " + $"clusterId: {ClusterId}, " + $"UserId: {UserId}";
		}
	}

	public class Explanation
	{
		public readonly Source Source;
		public readonly string AbTestId;
		public readonly string CohortId;
		public readonly string ConfigurationRuleId;
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public Explanation(Source source, string abTestId, string cohortId, string configurationRuleId)
		{
			Source = source;
			AbTestId = abTestId;
			CohortId = cohortId;
			ConfigurationRuleId = configurationRuleId;
		}
	}
	
	public enum Source
	{
		AbTest,
		ConfigurationRule,
		DefaultValue,
	}
}