namespace Voodoo.Tune.Core
{
	public class CohortResponse
	{
		public string Id { get; protected set; }
		public string ABTestName { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public CohortResponse(string id, string abTestName)
		{
			Id = id;
			ABTestName = abTestName;
		}
	}
}