using System;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class OnlyOneOfTypeAttribute : Attribute
    {

    }
}
