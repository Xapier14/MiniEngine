using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public interface IAutoInjectAttribute
    {
        public Type InjectType { get; }
    }
}
