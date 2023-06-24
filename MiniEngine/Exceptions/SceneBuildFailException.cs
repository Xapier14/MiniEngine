using System;
using System.Runtime.Serialization;

namespace MiniEngine
{
    [Serializable]
    public class SceneBuildFailException : Exception
    {
        public SceneBuildFailException() { }
        public SceneBuildFailException(string message)
            : base(message) { }
        public SceneBuildFailException(string message, Exception innerException)
            : base(message, innerException) { }
        protected SceneBuildFailException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
