namespace Voodoo.Tune.Core
{
	public class LayerMetadata
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }
		public bool Default { get; protected set; }
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public LayerMetadata(string id, string name, bool @default)
		{
			Id = id;
			Name = name;
			Default = @default;
		}
	}
}