using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This exception is thrown when store settings must be loaded but this configuration (platform) is missing in Kitchen.
     * These settings don't exist in the cached settings.
     */

    [Serializable]
    public class StoreNotCreatedException : Exception
    {
        public StoreNotCreatedException() : base() { }
        public StoreNotCreatedException(string message) : base(message) { }
        public StoreNotCreatedException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected StoreNotCreatedException(System.Runtime.Serialization.SerializationInfo info,
                                           System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}