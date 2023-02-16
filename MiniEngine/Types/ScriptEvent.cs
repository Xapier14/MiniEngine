using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    internal enum ScriptEvent
    {
        Create,
        BeforeUpdate,
        Update,
        AfterUpdate,
        BeforeDraw,
        AfterDraw,
        Destroy
    }
}
