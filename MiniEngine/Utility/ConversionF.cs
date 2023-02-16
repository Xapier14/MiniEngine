using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Utility
{
    public static class ConversionF
    {
        public static float TestForZero(float value, float halfWindow = 0.000001f)
        {
            if (value <= halfWindow && value >= -halfWindow)
                return 0f;
            return value;
        }
        public static float DegreesToRadians(float degree)
            => TestForZero(degree * MathF.PI / 180.0f);
        public static float RadiansToDegrees(float radian)
            => TestForZero(radian * 180.0f / MathF.PI);
    }
}
