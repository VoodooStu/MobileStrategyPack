using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public class OldABTest
	{
		public string Id { get; protected set; }
		public string Name { get; protected set; }
		public List<OldCohort> Cohorts { get; protected set; }

		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public OldABTest(string id, string name, List<OldCohort> cohorts)
		{
			Id = id;
			Name = name;
			Cohorts = cohorts;
		}
	}
}