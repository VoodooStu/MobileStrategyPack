using System;
using System.Collections.Generic;
using System.Linq;
using Voodoo.Tune.Utils;

namespace Voodoo.Tune.Core
{
	public static class VoodooSauceVariables
	{
		private static Dictionary<string, string> _parameters;
		private static List<Type> _allTypes;

		public static Dictionary<string, string> GetVSParams()
		{
			Dictionary<string, string> res = new Dictionary<string, string>();

#if VOODOO_SAUCE
			_allTypes = _allTypes ?? TypeUtility.GetTypes(t => t.GetInterface(nameof(IVoodooTuneFilledVariables)) != null);
			Type type = _allTypes.FirstOrDefault();

			if (type != null)
			{
				
				IVoodooTuneFilledVariables instance = (IVoodooTuneFilledVariables)type.CreateInstance();
				res = instance.GetSessionParameters();
			}
#endif

			return res;
		}
	}
}