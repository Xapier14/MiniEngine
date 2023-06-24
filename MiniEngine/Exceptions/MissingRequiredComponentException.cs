using System;
using System.Runtime.Serialization;

namespace MiniEngine
{
    [Serializable]
    public class MissingRequiredComponentException : Exception
    {
        public MissingRequiredComponentException() { }
        public MissingRequiredComponentException(string message)
            : base(message) { }
        public MissingRequiredComponentException(string message, Exception innerException)
            : base(message, innerException) { }
        protected MissingRequiredComponentException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
