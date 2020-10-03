using System;

namespace AdLoginDemo.Services
{
    /// <summary>
    /// Exceptionklasse für spezifische Fehler aus dem Active Directory.
    /// </summary>
    public class AdException : Exception
    {
        public AdException() : base() { }
        public AdException(string message) : base(message) { }
        public AdException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected AdException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
