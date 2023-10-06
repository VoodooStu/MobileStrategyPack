using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class Sandbox
	{
		public string Id { get; }
		public string Name { get; }
		public List<SandboxClass> Classes { get; }
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public Sandbox(string id, string name, List<SandboxClass> classes)
		{
			Id = id;
			Name = name;
			Classes = classes;
		}

		public override bool Equals(object obj)
		{
			if (obj is Sandbox other)
			{
				return other.Id == Id;
			}

			return false;
		}

		public override int GetHashCode() => Id.GetHashCode();
		
		public class SandboxClass
		{
			public string ClassTechnicalName { get; }
			public List<Dictionary<string, object>> Instances { get; }

#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonConstructor]
#endif
			public SandboxClass(string classTechnicalName, List<Dictionary<string, object>> instances)
			{
				ClassTechnicalName = classTechnicalName;
				Instances = instances;
			}
		}
	}
}