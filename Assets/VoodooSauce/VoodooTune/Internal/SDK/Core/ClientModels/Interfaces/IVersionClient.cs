using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface IVersionClient 
	{
		IClassClient Class { get; }
		ISegmentClient Segment { get; }
		IABTestClient AbTest { get; }
		ILayerClient Layer { get; }
		
		Task<OldVersionMetadata> GetOldMetadata(string appId, string versionId);
		Task<VersionMetadata> Get(string appId, string versionId);
		Task<VersionMetadata> GetWorkInProgress(string appId);
		Task<VersionMetadata> GetLive(string appId);
		Task<IReadOnlyList<Version>> GetAll(string appId);
		Task<VersionResponse> Publish(string appId, string versionName);
		Task<string> Reset(string appId, string versionId);
		Task<Version> Rollback(string appId, string versionId);
	}
}