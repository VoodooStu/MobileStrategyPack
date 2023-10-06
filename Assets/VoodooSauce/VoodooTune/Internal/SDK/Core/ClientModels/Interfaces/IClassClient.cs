using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface IClassClient 
	{
		Task<IReadOnlyList<ClassInfo>> GetAll(string appId, string versionReference);
		Task<ClassInfo> Get(string appId, string versionReference, Type classType);
		Task<string> Create(string appId, NewClassInfo classInfo);
		Task<string> Update(string appId, NewClassInfo classInfo);
	}
}