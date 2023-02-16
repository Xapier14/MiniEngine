using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Components
{
    [RequiresComponent<Transform>]
    public class Motion : Component
    {
        public double AngularVelocity { get; set; }
        public Vector2F Velocity = Vector2F.Zero;
        public Vector2F Acceleration = Vector2F.Zero;
    }
}
