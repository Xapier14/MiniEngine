using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class InjectAttribute<T> : Attribute, IInjectAttribute
    {
        public Type InjectType { get; }

        public InjectAttribute()
        {
            InjectType = typeof(T);
        }
    }
}
