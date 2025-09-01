using System;

namespace MiniEngine.Components
{
    [OnlyOneOfType]
    public class Scriptable : Component
    {
        public double Delta { get; internal set; }
        public float DeltaF => (float)Delta;
        public Action<Scriptable>? Create { get; set; }
        public Action<Scriptable>? BeforeUpdate { get; set; }
        public Action<Scriptable>? Update { get; set; }
        public Action<Scriptable>? AfterUpdate { get; set; }
        public Action<Scriptable>? BeforeDraw { get; set; }
        public Action<Scriptable>? AfterDraw { get; set; }
        public Action<Scriptable>? Destroy { get; set; }
    }
}
