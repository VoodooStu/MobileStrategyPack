using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class Cohort
	{
		public string Id { get; }
		public bool IsControl { get; protected set; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public int AllocationRate { get; protected set; }
		public int? UserCount { get; protected set; }
		public List<OverrideValue> OverrideValues { get; protected set; }
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public Cohort(string id, bool isControl, string name, string description, int allocationRate, int? userCount, List<OverrideValue> overrideValues)
		{
			Id = id;
			IsControl = isControl;
			Name = name;
			Description = description;
			AllocationRate = allocationRate;
			UserCount = userCount;
			OverrideValues = overrideValues;
		}
		

		public override bool Equals(object obj)
		{
			if (obj is Cohort other)
			{
				return other.Id == Id;
			}

			return false;
		}

		public override int GetHashCode() => Id.GetHashCode();
		
		public class OverrideValue
		{
			public string ClassTechnicalName { get; protected set; }
			public Instance Instances { get; protected set; }

			public OverrideValue(string @class, Instance instances)
			{
				ClassTechnicalName = @class;
				Instances = instances;
			}

			public class Instance
			{
				public Dictionary<string, Value> Instances { get; protected set; }

				public Instance(Dictionary<string, Value> instances)
				{
					Instances = instances;
				}
			}

			public class Value
			{
				public Dictionary<string, object> Values { get; protected set; }
				public bool Enabled { get; protected set; }

				public Value(Dictionary<string, object> values, bool enabled)
				{
					Values = values;
					Enabled = enabled;
				}
			}
		}
	}
}