using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Utility
{
    public static class Conversion
    {
        public static double DegreesToRadians(double degree)
            => degree * Math.PI / 180.0;
        public static double RadiansToDegrees(double radian)
            => radian * 180.0 / Math.PI;
    }
}
