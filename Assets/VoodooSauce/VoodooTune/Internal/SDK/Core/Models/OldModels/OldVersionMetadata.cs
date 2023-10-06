using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class OldVersionMetadata
	{
		public string Id { get; protected set; }
		public string AppId { get; protected set; }
		public List<OldSegment> Segments { get; protected set; }
		public List<OldABTest> AbTests { get; protected set; }
		public List<OldSandbox> Sandboxes { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public OldVersionMetadata(string versionId, string appId, List<OldSegment> segments, List<OldABTest> abTests, List<OldSandbox> sandboxes)
		{
			Id = versionId;
			AppId = appId;
			Segments = segments;
			AbTests = abTests;
			Sandboxes = sandboxes;
		}
	}
}