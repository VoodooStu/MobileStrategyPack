using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This exception is thrown when store settings must be loaded
     * but the new Kitchen settings are not in cached (not downloaded yet).
     */

    [Serializable]
    public class SettingsNotCreatedException : Exception
    {
        public SettingsNotCreatedException() : base() { }
        public SettingsNotCreatedException(string message) : base(message) { }
        public SettingsNotCreatedException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected SettingsNotCreatedException(System.Runtime.Serialization.SerializationInfo info,
                                              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}