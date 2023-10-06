using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class Segment
	{
		public string Id { get; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public bool System { get; protected set; }
		public List<Condition> Conditions { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public Segment(string id, string name, string description, bool system, List<Condition> conditions)
		{
			Id = id;
			Name = name;
			Description = description;
			System = system;
			Conditions = conditions;
		}

		public override bool Equals(object obj)
		{
			if (obj is Segment other)
			{
				return other.Id == Id;
			}

			return false;
		}

		public override int GetHashCode() => Id.GetHashCode();
	}
}