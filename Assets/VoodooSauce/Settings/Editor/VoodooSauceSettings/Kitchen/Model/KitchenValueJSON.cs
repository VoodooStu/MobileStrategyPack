using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    public enum KitchenValueType
    {
        Boolean,
        File,
        Text,
        Undefined
    }

    /*
     * This class represents the JSON structure 'key' returned by the Kitchen server.
     */
    
    [Serializable]
    public struct KitchenValueJSON
    {
        public string value;
        public int version;
        public string checksum;

        private static Dictionary<string, KitchenValueType> _valueTypeStrings = new Dictionary<string, KitchenValueType>() {
            {"boolean", KitchenValueType.Boolean},
            {"file", KitchenValueType.File},
            {"text", KitchenValueType.Text},
            {"", KitchenValueType.Undefined},
        };
        
        [SerializeField] private string type;
        public KitchenValueType Type {
            get => type != null ? _valueTypeStrings[type] : KitchenValueType.Undefined;
            set => type = _valueTypeStrings.FirstOrDefault(x => x.Value == value).Key;
        }
        
        [SerializeField] private string last_update;
        public DateTime LastUpdate {
            get => DateTimeHelper.StringToDateTime(last_update);
            set => last_update = DateTimeHelper.DateTimeToString(value);
        }

        public bool BoolValue()
        {
            // Do not throw an exception if type is not initialized,
            // that means that the value is set to "false" in kitchen web UI.
            if (Type != KitchenValueType.Boolean && Type != KitchenValueType.Undefined) {
                throw new InvalidTypeException($"This value is not a boolean (is {type})");
            }

            return value == "true";
        }

        public float FloatValue()
        {
            if (Type != KitchenValueType.Text && Type != KitchenValueType.Undefined) {
                throw new InvalidTypeException("This value is not a float");
            }

            return string.IsNullOrEmpty(value) ? 0.0f : float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
        }

        public int IntergerValue()
        {
            if (Type != KitchenValueType.Text && Type != KitchenValueType.Undefined)
            {
                throw new InvalidTypeException($"The value '{value}' is not a integer");
            }

            return string.IsNullOrEmpty(value) ? 0 : int.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
        }

        public bool IsFile() => Type == KitchenValueType.File;

        public bool IsUpToDate(KitchenValueJSON other) => last_update != "" && version >= other.version;
    }
}