using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandledByAttribute<T> : Attribute, IHandledByAttribute where T : System
    {
        public Type SystemType { get; init; }

        public HandledByAttribute()
        {   
            SystemType = typeof(T);
        }
    }
}
