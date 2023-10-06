using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This exception is thrown when a voodoo settings field has an incorrect value
     * but the new Kitchen settings are not in cached (not downloaded yet).
     */

    [Serializable]
    public class BadFormattedSettingValueException : Exception
    {
        public BadFormattedSettingValueException() : base() { }
        public BadFormattedSettingValueException(string message) : base(message) { }
        public BadFormattedSettingValueException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected BadFormattedSettingValueException(System.Runtime.Serialization.SerializationInfo info,
                                                 System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}