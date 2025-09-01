using System;
using System.Runtime.Serialization;

namespace MiniEngine;

[Serializable]
public class EngineNotInitializedException : Exception
{
    public EngineNotInitializedException() { }
    public EngineNotInitializedException(string message)
        : base(message) { }
    public EngineNotInitializedException(string message, Exception innerException)
        : base(message, innerException) { }
    protected EngineNotInitializedException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}