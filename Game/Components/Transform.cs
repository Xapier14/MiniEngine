
using MiniEngine;

namespace MiniEngine.Components
{
    [HandledBy<TransformSystem>]
    public class Transform : Component
    {
        public int X { get; set; }
        public int Y { get; set; }
        private double _rotation;
        public double Rotation
        {
            get => _rotation;
            set => _rotation = value % 360.0;
        }
    }
}
