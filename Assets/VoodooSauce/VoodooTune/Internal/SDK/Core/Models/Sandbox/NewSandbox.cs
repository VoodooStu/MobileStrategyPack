using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class NewSandbox
	{
		public string name = "";
		public List<NewSandboxClass> classes;
		
		public class NewSandboxClass
		{
			public string classTechnicalName;
			public Dictionary<string, object> instances;

#if NEWTONSOFT_JSON
			[Newtonsoft.Json.JsonConstructor]
#endif
			public NewSandboxClass(string classTechnicalName, Dictionary<string, object> instances)
			{
				this.classTechnicalName = classTechnicalName;
				this.instances = instances;
			}
		}
	}
}