using System;
using UnityEngine;

namespace Voodoo.Sauce.Common.Utils
{
    public class EnumMask : PropertyAttribute
    {
        private readonly string[] _valueNames;
        private readonly int[] _valueValues;
        
        public EnumMask(Type type)
        {
            if (type.IsEnum) {
                Array values = Enum.GetValues(type);
                
                _valueNames = new string[values.Length];
                _valueValues = new int[values.Length];
                
                for (var i = 0; i < _valueNames.Length; i++) {
                    _valueNames[i] = values.GetValue(i).ToString();
                    _valueValues[i] = (int)values.GetValue(i);
                }
            } else {
                _valueNames = new[] { "n/a" };
                _valueValues = new[] {0};

            }
        }

        public string[] GetEnumValueNames() => _valueNames;
        public int[] GetEnumValues() => _valueValues;
    }
}
