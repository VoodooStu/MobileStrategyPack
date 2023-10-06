using System.Collections.Generic;
using System.Linq;

namespace Voodoo.Tune.Core
{
	public class VoodooConfig
	{
		public readonly string ConfigurationId;
		public readonly string SegmentIds;
		public readonly string AbTestIds;
		public readonly string CohortIds;
		public readonly string CohortNames;
		public readonly string VersionNumber; //For the sandboxes it represent the sandbox id
		
		public List<string> SegmentIdsToList { get; private set; }
		public List<string> AbTestIdsToList { get; private set; }
		public List<string> CohortIdsToList { get; private set; }
		public List<string> CohortNamesToList { get; private set; }
		
		public string MainAbTestId => AbTestIdsToList[0];
		public string MainCohortId => CohortIdsToList[0];
		public string MainCohortName => CohortNamesToList[0];

		public bool IsValid => AbTestIdsToList.Count == CohortIdsToList.Count;
		
#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public VoodooConfig(string cid, string cli, string abi, string coi, string con, string vid)
		{
			ConfigurationId = cid;
			SegmentIds = cli;
			AbTestIds = abi;
			CohortIds = coi;
			CohortNames = con;
			VersionNumber = vid;
			
			SegmentIdsToList = cli.Split(',').ToList();
			AbTestIdsToList = abi.Split(',').ToList();
			CohortIdsToList = coi.Split(',').ToList();
			CohortNamesToList = con.Split(',').ToList();
		}

		public override string ToString()
		{
			string message = $"ConfigurationId: {ConfigurationId}, " +
			                 $"SegmentationUuid: {SegmentIds}, " +
			                 $"AbTestUuid: {AbTestIds}, " +
			                 $"AbTestCohortUuid: {CohortIds}, " +
			                 $"AbTestCohortName: {CohortNames}, " +
			                 $"AbTestVersionUuid: {VersionNumber}";
			return message;
		}
	}
}