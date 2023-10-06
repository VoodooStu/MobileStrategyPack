namespace Voodoo.Tune.Core
{
	public class NewLayer
	{
		public string name;
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonProperty("default")]
#endif
		public bool isDefaultLayer;
	}
}