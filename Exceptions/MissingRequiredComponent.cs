using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
