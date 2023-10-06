namespace Voodoo.Tune.Core
{
	public class VersionResponse
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public VersionResponse(string versionId, string versionName)
		{
			Id = versionId;
			Name = versionName;
		}
	}
}