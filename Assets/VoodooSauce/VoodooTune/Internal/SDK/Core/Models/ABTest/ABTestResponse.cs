namespace Voodoo.Tune.Core
{
	public class ABTestResponse
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }

		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public ABTestResponse(string id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}