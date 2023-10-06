using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This exception is thrown when the asked value of a KitchenValueJSON is not of its original type.
     * For example, if you ask the FloatValue of a KitchenValueJSON with the type "file" this exception is thrown.
     */
    
    [Serializable]
    public class InvalidTypeException : Exception
    {
        public InvalidTypeException() : base() { }
        public InvalidTypeException(string message) : base(message) { }
        public InvalidTypeException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected InvalidTypeException(System.Runtime.Serialization.SerializationInfo info,
                                       System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}