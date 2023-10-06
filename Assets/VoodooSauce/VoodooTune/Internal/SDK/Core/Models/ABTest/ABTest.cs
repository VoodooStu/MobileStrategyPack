using System;
using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class ABTest
	{
		public string Id { get; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public string CreatedOnVersion { get; protected set; }
		public int Priority { get; protected set; }
		public List<string> Segments { get; protected set; }
		public List<Cohort> Cohorts { get; protected set; }
		public string LayerId { get; protected set; }
		public string StoppedOnVersion { get; protected set; }
		public ABTestState State { get; protected set; }
		public DateTime? BeginsAt { get; protected set; }
		public DateTime? EndsAt { get; protected set; }
		public bool Immutable { get; protected set; }
		public Dictionary<string, string> SegmentNames { get; protected set; }
		public PlatformVersion Platform { get; protected set; }
		public int? UserCount { get; protected set; }
		
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public ABTest(string id, string name, string description, string createdOnVersion, int priority, List<string> segments, List<Cohort> cohorts, string layerId, string stoppedOnVersion, ABTestState state, DateTime? beginsAt, DateTime? endsAt, bool immutable, Dictionary<string, string> segmentNames, PlatformVersion platform, int? userCount)
		{
			Id = id;
			Name = name;
			Description = description;
			CreatedOnVersion = createdOnVersion;
			Priority = priority;
			Segments = segments;
			Cohorts = cohorts;
			LayerId = layerId;
			StoppedOnVersion = stoppedOnVersion;
			State = state;
			BeginsAt = beginsAt;
			EndsAt = endsAt;
			Immutable = immutable;
			SegmentNames = segmentNames;
			Platform = platform;
			UserCount = userCount;
		}

		public override bool Equals(object obj)
		{
			if (obj is ABTest other)
			{
				return other.Id == Id;
			}

			return false;
		}

		public override int GetHashCode() => Id.GetHashCode();
		
		public class PlatformVersion
		{
			public VersionRange IOS { get; protected set; }
			public VersionRange Android { get; protected set; }

			public bool AllPlatforms => (IOS == null && Android == null) || (IOS != null && Android != null);
		
			
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonConstructor]
#endif
			public PlatformVersion(VersionRange ios, VersionRange android)
			{
				IOS = ios;
				Android = android;
			}
		}

		public class VersionRange
		{
			public string From { get; protected set; }
			public string To { get; protected set; }
		
			
#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonConstructor]
#endif
			public VersionRange(string from, string to)
			{
				From = from;
				To = to;
			}
		}
	}
}