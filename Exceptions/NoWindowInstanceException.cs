using System;
using System.Runtime.Serialization;

namespace MiniEngine
{
    [Serializable]
    public class NoWindowInstanceException : Exception
    {
        public NoWindowInstanceException() { }
        public NoWindowInstanceException(string message)
            : base(message) { }
        public NoWindowInstanceException(string message, Exception innerException)
            : base(message, innerException) { }
        protected NoWindowInstanceException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
