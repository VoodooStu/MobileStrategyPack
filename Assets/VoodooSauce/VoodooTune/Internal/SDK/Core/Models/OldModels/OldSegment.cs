namespace Voodoo.Tune.Core
{
	public class OldSegment
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }

		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public OldSegment(string id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}