using System;
using System.Runtime.Serialization;

namespace MiniEngine
{
    [Serializable]
    public class EngineNotRunningException : Exception
    {
        public EngineNotRunningException() { }
        public EngineNotRunningException(string message)
            : base(message) { }
        public EngineNotRunningException(string message, Exception innerException)
            : base(message, innerException) { }
        protected EngineNotRunningException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
