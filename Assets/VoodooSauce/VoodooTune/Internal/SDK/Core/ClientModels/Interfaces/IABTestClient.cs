using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface IABTestClient
	{
		ICohortClient Cohort { get; }
		Task<IReadOnlyList<ABTest>> GetAll(string appId, string versionReference, string layerId);
		Task<IReadOnlyList<ABTest>> GetAll(string appId, string versionReference, List<ABTestState> states = null);
		Task<ABTest> Get(string appId, string versionReference, string abTestId);
		Task<ABTestResponse> Create(string appId, NewABTest abTest);
		Task<string> Duplicate(string appId, string abTestId);
		Task<ABTestResponse> Update(string appId, string abTestId, NewABTest abTest);
		Task<ABTestResponse> UpdateStatus(string appId, string abTestId, ABTestState state);
	}
}