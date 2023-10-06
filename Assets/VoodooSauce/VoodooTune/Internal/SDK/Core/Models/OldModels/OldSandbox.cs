namespace Voodoo.Tune.Core
{
	public class OldSandbox
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }

		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public OldSandbox(string id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}