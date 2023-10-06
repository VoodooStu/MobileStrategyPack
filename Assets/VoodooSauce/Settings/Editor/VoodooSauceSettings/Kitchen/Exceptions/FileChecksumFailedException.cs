using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This exception is thrown when the verification of a file's checksum fails.
     */

    [Serializable]
    public class FileChecksumFailedException : Exception
    {
        public FileChecksumFailedException() : base() { }
        public FileChecksumFailedException(string message) : base(message) { }
        public FileChecksumFailedException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected FileChecksumFailedException(System.Runtime.Serialization.SerializationInfo info,
                                             System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}