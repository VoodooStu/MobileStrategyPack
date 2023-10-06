using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class ClassInfo
	{
		public string DisplayName { get; protected set; }
		public string TechnicalName { get; protected set; }
		public string Description { get; protected set; }
		public string BundleId { get; protected set; }
		public List<Attribute> Attributes { get; protected set; }
		public List<Instance> Instances { get; protected set; }
		public bool MultiInstance { get; protected set; }
		public string LockedByLayer { get; protected set; }

		#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public ClassInfo(string displayName, string technicalName, string description, List<Attribute> attributes, List<Instance> instances, bool multiInstance, string bundleId, string lockedByLayer)
		{
			DisplayName = displayName;
			TechnicalName = technicalName;
			Description = description;
			Attributes = attributes;
			Instances = instances;
			MultiInstance = multiInstance;
			BundleId = bundleId;
			LockedByLayer = lockedByLayer;
		}

		public class Instance
		{
			public string Id { get; protected set; }
			public Dictionary<string, object> Values { get; protected set; }
			public bool Enabled { get; protected set; }

			#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
			public Instance(string id, Dictionary<string, object> values, bool enabled)
			{
				Id = id;
				Values = values;
				Enabled = enabled;
			}
		}

		public class Attribute
		{
			public string DisplayName { get; protected set; }
			public string TechnicalName { get; protected set; }
			public string Type { get; protected set; }
			public List<string> AvailableValues { get; protected set; }

			#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
			public Attribute(string displayName, string technicalName, string type, List<string> availableValues)
			{
				DisplayName = displayName;
				TechnicalName = technicalName;
				Type = type;
				AvailableValues = availableValues;
			}

			private static readonly Dictionary<string, string> CSharpTypeEquivalent = new Dictionary<string, string>()
			{
				{"string","string"},
				{"float","double"},//Done for security purposes
				{"integer","int"},
				{"boolean","bool"},
				{"string[]","string[]"},
				{"float[]","double[]"},//Done for security purposes
				{"integer[]","int[]"},
			};
			
			public string CSharpType => CSharpTypeEquivalent.ContainsKey(Type) ? CSharpTypeEquivalent[Type] : string.Empty;
		}
	}
}