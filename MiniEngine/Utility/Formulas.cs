using System;

namespace MiniEngine.Utility
{
    public static class Formulas
    {
        public static int DistanceFrom(Vector2 point1, Vector2 point2)
        {
            var xSqr = point2.X - point1.X;
            xSqr *= xSqr;
            var ySqr = point2.Y - point1.Y;
            ySqr *= ySqr;

            return (int)Math.Min(Math.Max(Math.Round(Math.Sqrt(xSqr + ySqr)), int.MinValue), int.MaxValue);
        }

        public static float DistanceFrom(Vector2F point1, Vector2F point2)
        {
            var xSqr = point2.X - point1.X;
            xSqr *= xSqr;
            var ySqr = point2.Y - point1.Y;
            ySqr *= ySqr;

            return MathF.Sqrt(xSqr + ySqr);
        }

        public static float AngleBetween(Vector2 point1, Vector2 point2)
        {
            return ConversionF.RadiansToDegrees(MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X));
        }

        public static float AngleBetween(Vector2F point1, Vector2F point2)
        {
            return ConversionF.RadiansToDegrees(MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X));
        }

        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
