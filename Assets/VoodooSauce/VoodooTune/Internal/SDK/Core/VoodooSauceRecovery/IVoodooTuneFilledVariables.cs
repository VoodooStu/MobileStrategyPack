using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	public interface IVoodooTuneFilledVariables
	{
		Dictionary<string, string> GetSessionParameters();
	}
}