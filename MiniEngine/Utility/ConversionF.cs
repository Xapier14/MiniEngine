using System;

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
        public static float TestForZero(double value, double halfWindow = 0.000001)
        {
            if (value <= halfWindow && value >= -halfWindow)
                return 0f;
            return (float)value;
        }
        public static float DegreesToRadians(float degree)
            => TestForZero(degree * MathF.PI / 180.0f);
        public static float RadiansToDegrees(float radian)
            => TestForZero(radian * 180.0f / MathF.PI);
        public static float DegreesToRadians(double degree)
            => TestForZero(degree * MathF.PI / 180.0f);
        public static float RadiansToDegrees(double radian)
            => TestForZero(radian * 180.0f / MathF.PI);
    }
}
