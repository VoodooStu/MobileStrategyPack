using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface ISegmentClient 
	{
		Task<IReadOnlyList<Segment>> GetAll(string appId, string versionReference);
		Task<Segment> Get(string appId, string versionReference, string segmentId);
		Task<string> Create(string appId, NewSegment segment);
		Task<string> Update(string appId, string segmentId, NewSegment segment);
	}
}