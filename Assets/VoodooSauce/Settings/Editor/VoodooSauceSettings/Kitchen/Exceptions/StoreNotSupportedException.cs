using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This exception is thrown when a store does not exist in the cached settings.
     * This store is considered as unsupported.
     */

    [Serializable]
    public class StoreNotSupportedException : Exception
    {
        public StoreNotSupportedException() : base() { }
        public StoreNotSupportedException(string message) : base(message) { }
        public StoreNotSupportedException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected StoreNotSupportedException(System.Runtime.Serialization.SerializationInfo info,
                                             System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}