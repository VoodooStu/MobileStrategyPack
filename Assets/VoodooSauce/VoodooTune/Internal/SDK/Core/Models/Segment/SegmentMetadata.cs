namespace Voodoo.Tune.Core
{
	public class SegmentMetadata
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public SegmentMetadata(string id, string name, string description)
		{
			Id = id;
			Name = name;
			Description = description;
		}
	}
}