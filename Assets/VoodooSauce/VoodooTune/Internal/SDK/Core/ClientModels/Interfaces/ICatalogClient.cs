using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voodoo.Tune.Core
{
	public interface ICatalogClient 
	{
		Task<IReadOnlyList<ClassInfo>> GetAll(string bundleId = null);
		Task<ClassInfo> Get(Type classType);
		Task<string> Create(NewClassInfo classInfo);
		Task<string> Update(NewClassInfo classInfo);
	}
}