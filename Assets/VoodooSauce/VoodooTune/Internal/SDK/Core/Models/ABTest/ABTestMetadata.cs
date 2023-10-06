using System;
using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class ABTestMetadata
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public string CreatedOnVersion { get; protected set; }
		public int Priority { get; protected set; }
		public List<string> Segments { get; protected set; }
		public List<Cohort> Cohorts { get; protected set; }
		public DateTime? ArchivedAt { get; protected set; }

		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public ABTestMetadata(string id, string name, string description, string createdOnVersion, int priority, List<string> segments, List<Cohort> cohorts, DateTime? archivedAt)
		{
			Id = id;
			Name = name;
			Description = description;
			CreatedOnVersion = createdOnVersion;
			Priority = priority;
			Segments = segments;
			Cohorts = cohorts;
			ArchivedAt = archivedAt;
		}
	}
}