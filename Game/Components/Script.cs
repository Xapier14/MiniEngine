using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Components
{
    public class Script : Component
    {
        public double Delta { get; internal set; }
        public Action<Script>? Create { get; set; }
        public Action<Script>? BeforeUpdate { get; set; }
        public Action<Script>? Update { get; set; }
        public Action<Script>? AfterUpdate { get; set; }
        public Action<Script>? BeforeDraw { get; set; }
        public Action<Script>? AfterDraw { get; set; } 
        public Action<Script>? Destroy { get; set; }
    }
}
