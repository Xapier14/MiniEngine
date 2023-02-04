using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlesComponentAttribute<T> : Attribute, IHandlesComponentAttribute where T : Component
    {
        public Type ComponentType { get; init; }

        public HandlesComponentAttribute()
        {   
            ComponentType = typeof(T);
        }
    }
}
