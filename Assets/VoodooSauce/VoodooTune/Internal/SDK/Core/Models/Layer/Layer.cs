using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class Layer
	{
		public string Id { get; }
		public string Name { get; protected set; }
		public bool Default { get; protected set; }
		public int ABTestsCount { get; protected set; }
		public List<string> LockedClasses { get; protected set; }
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public Layer(string id, string name, bool @default, int abTestsCount, List<string> lockedClasses)
		{
			Id = id;
			Name = name;
			Default = @default;
			ABTestsCount = abTestsCount;
			LockedClasses = lockedClasses;
		}

		public override bool Equals(object obj)
		{
			if (obj is Layer other)
			{
				return other.Id == Id;
			}

			return false;
		}

		public override int GetHashCode() => Id.GetHashCode();
	}
}