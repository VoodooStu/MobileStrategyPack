using System;
using System.Collections.Generic;
using Voodoo.Sauce.Debugger;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public static class DebugCustomUtility
    {
        private const string TAG = "DebugCustomUtility";

        private static List<CustomDebugger> _customTypes;

        public static List<CustomDebugger> GetAllCustomDebugger()
        {
            if (_customTypes != null) return _customTypes;
            _customTypes = new List<CustomDebugger>();
            
            Type customType = typeof(CustomDebugger);
            List<Type> allTypes = AssembliesUtils.GetTypes(customType);
            
            foreach (Type type in allTypes) {
                VoodooLog.LogDebug(Module.COMMON, TAG, $"CustomDebugger '{type}' found");
                CustomDebugger instance = (CustomDebugger)Activator.CreateInstance(type);
                _customTypes.Add(instance);
            }
            
            _customTypes.Sort(SortByIndex);

            return _customTypes;
        }

        private static int SortByIndex(CustomDebugger x, CustomDebugger y)
        {
            int orderX = x.GetOrderIndex();
            int orderY = y.GetOrderIndex();
            if (orderX == orderY) {
                return string.Compare(x.GetTitle(), y.GetTitle(), StringComparison.Ordinal);
            }
            return orderX.CompareTo(orderY);
        }
    }
}
