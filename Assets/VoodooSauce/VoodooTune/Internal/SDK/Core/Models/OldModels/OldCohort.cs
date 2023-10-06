namespace Voodoo.Tune.Core
{
	public class OldCohort
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public OldCohort(string id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}