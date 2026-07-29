using System;
using System.Runtime.Serialization;

namespace MiniEngine
{
    /// <summary>
    /// Thrown by <see cref="GameEngine.FatalExit(int)"/> after the engine has released
    /// itself. Catch this at your entry point if the process should exit.
    /// </summary>
    [Serializable]
    public class EngineFatalException : Exception
    {
        public int ExitCode { get; }

        public EngineFatalException() { }
        public EngineFatalException(string message)
            : base(message) { }
        public EngineFatalException(string message, int exitCode)
            : base(message) => ExitCode = exitCode;
        public EngineFatalException(string message, Exception innerException)
            : base(message, innerException) { }
        protected EngineFatalException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
