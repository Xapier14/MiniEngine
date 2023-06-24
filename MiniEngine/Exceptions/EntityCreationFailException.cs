using System;
using System.Runtime.Serialization;

namespace MiniEngine
{
    [Serializable]
    public class EntityCreationFailException : Exception
    {
        public EntityCreationFailException() { }
        public EntityCreationFailException(string message)
            : base(message) { }
        public EntityCreationFailException(string message, Exception innerException)
            : base(message, innerException) { }
        protected EntityCreationFailException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
