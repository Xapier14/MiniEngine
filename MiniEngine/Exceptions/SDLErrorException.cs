using System;
using System.Runtime.Serialization;

namespace MiniEngine
{
    [Serializable]
    public class SDLErrorException : Exception
    {
        public SDLErrorException() { }
        public SDLErrorException(string message)
            : base(message) { }
        public SDLErrorException(string message, Exception innerException)
            : base(message, innerException) { }
        protected SDLErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}