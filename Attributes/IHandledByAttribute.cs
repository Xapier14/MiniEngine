using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public interface IHandledByAttribute
    {
        public Type SystemType { get; }
    }
}
