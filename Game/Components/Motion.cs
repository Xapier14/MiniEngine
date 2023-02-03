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
        private double _direction;
        public double Direction
        {
            get => _direction;
            set => _direction = value % 360.0;
        }

        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Velocity
        {
            get => Math.Sqrt(Math.Pow(VelocityX, 2) + Math.Pow(VelocityY, 2));
            set
            {
                VelocityX = Math.Cos(Direction) * value;
                VelocityY = Math.Sin(Direction) * value;
            }
        }

        public double AccelerationX { get; set; }
        public double AccelerationY { get; set; }
        public double Acceleration
        {
            get => Math.Sqrt(Math.Pow(AccelerationX, 2) + Math.Pow(AccelerationY, 2));
            set
            {
                AccelerationX = Math.Cos(Direction) * value;
                AccelerationY = Math.Sin(Direction) * value;
            }
        }
    }
}
