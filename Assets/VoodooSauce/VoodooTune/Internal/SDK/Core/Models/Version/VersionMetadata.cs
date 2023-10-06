using System;
using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class VersionMetadata
	{
		public string Id { get; protected set; }
		public List<SegmentMetadata> Segments { get; protected set; }
		public string AppId { get; protected set; }
		public int Version { get; protected set; }
		public string Name { get; protected set; }
		public DateTime? CreatedAt { get; protected set; }
		public DateTime? PublishedAt { get; protected set; }
		public List<ABTestMetadata> AbTests { get; protected set; }
		public List<LayerMetadata> Layers { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public VersionMetadata(string versionId, List<SegmentMetadata> segments, string appId, int version, string versionName, DateTime? createdAt, DateTime? publishedAt, List<ABTestMetadata> abTests, List<LayerMetadata> layers)
		{
			Id = versionId;
			Segments = segments;
			AppId = appId;
			Version = version;
			Name = versionName;
			CreatedAt = createdAt;
			PublishedAt = publishedAt;
			AbTests = abTests;
			Layers = layers;
		}
	}
}