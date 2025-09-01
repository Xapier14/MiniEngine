using MiniEngine.Utility;
using System;
using System.Text.RegularExpressions;

namespace MiniEngine;

public interface IReadOnlyVector2F
{
    public float Magnitude => MathF.Sqrt(MathF.Pow(X, 2f) + MathF.Pow(Y, 2f));
    public float Angle => Magnitude > 0 ? ConversionF.RadiansToDegrees(MathF.Atan2(Y, X)) : 0;
    public float X { get; }
    public float Y { get; }
}

public struct Vector2F : IEquatable<Vector2F>, IEquatable<Vector2>, IEquatable<(int X, int Y)>, IEquatable<(float X, float Y)>, IParsable<Vector2F>
{
    public static Vector2F Zero => new(0, 0);
    public static Vector2F PositiveInf => new(float.MaxValue, float.MaxValue);
    public static Vector2F NegativeInf => new(float.MinValue, float.MinValue);

    public float Magnitude => MathF.Sqrt(MathF.Pow(X, 2f) + MathF.Pow(Y, 2f));
    public float Angle => Magnitude > 0 ? ConversionF.RadiansToDegrees(MathF.Atan2(Y, X)) : 0;

    public float X { get; set; }
    public float Y { get; set; }

    public Vector2F(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vector2F(double x, double y)
    {
        X = (float)x;
        Y = (float)y;
    }

    public bool Equals(Vector2 other)
    {
        // both are positive inf
        if (ApproximatelyEqual(X, float.MaxValue) && ApproximatelyEqual(Y, float.MaxValue)
            && other is { X: int.MaxValue, Y: int.MaxValue })
            return true;
        // both are negative inf
        if (ApproximatelyEqual(X, float.MinValue) && ApproximatelyEqual(Y, float.MinValue)
            && other is { X: int.MinValue, Y: int.MinValue })
            return true;
        return ApproximatelyEqual(X, other.X) && ApproximatelyEqual(Y, other.Y);
    }

    public bool Equals((int X, int Y) other)
    {
        // both are positive inf
        if (ApproximatelyEqual(X, float.MaxValue) && ApproximatelyEqual(Y, float.MaxValue)
            && other is { X: int.MaxValue, Y: int.MaxValue })
            return true;
        // both are negative inf
        if (ApproximatelyEqual(X, float.MinValue) && ApproximatelyEqual(Y, float.MinValue)
            && other is { X: int.MinValue, Y: int.MinValue })
            return true;
        return ApproximatelyEqual(X, other.X) && ApproximatelyEqual(Y, other.Y);
    }

    public bool Equals((float X, float Y) other)
    {
        // both are positive inf
        if (ApproximatelyEqual(X, float.MaxValue) && ApproximatelyEqual(Y, float.MaxValue)
            && ApproximatelyEqual(other.X, float.MaxValue) && ApproximatelyEqual(other.Y, float.MaxValue))
            return true;
        // both are negative inf
        if (ApproximatelyEqual(X, float.MinValue) && ApproximatelyEqual(Y, float.MinValue)
            && ApproximatelyEqual(other.X, float.MinValue) && ApproximatelyEqual(other.Y, float.MinValue))
            return true;
        return ApproximatelyEqual(X, other.X) && ApproximatelyEqual(Y, other.Y);
    }

    public bool Equals(Vector2F other)
    {
        // both are positive inf
        if (ApproximatelyEqual(X, float.MaxValue) && ApproximatelyEqual(Y, float.MaxValue)
            && ApproximatelyEqual(other.X, float.MaxValue) && ApproximatelyEqual(other.Y, float.MaxValue))
            return true;
        // both are negative inf
        if (ApproximatelyEqual(X, float.MinValue) && ApproximatelyEqual(Y, float.MinValue)
            && ApproximatelyEqual(other.X, float.MinValue) && ApproximatelyEqual(other.Y, float.MinValue))
            return true;
        return ApproximatelyEqual(X, other.X) && ApproximatelyEqual(Y, other.Y);
    }

    public Vector2 Denormalize(Vector2 range)
    {
        var halfX = range.X / 2;
        var halfY = range.Y / 2;

        var x = (int)MathF.Round(ConversionF.TestForZero(halfX * X)) + halfX;
        var y = (int)MathF.Round(ConversionF.TestForZero(halfY * Y)) + halfY;
        return new Vector2(x, y);
    }

    public Vector2F Set(float x, float y)
    {
        X = x;
        Y = y;
        return this;
    }

    public Vector2F SetFrom(float degrees, float magnitude)
    {
        X = ConversionF.TestForZero(MathF.Cos(ConversionF.DegreesToRadians(degrees)) * magnitude);
        Y = ConversionF.TestForZero(MathF.Sin(ConversionF.DegreesToRadians(degrees)) * magnitude);
        return this;
    }

    public Vector2F SetFrom(double degrees, double magnitude)
    {
        X = ConversionF.TestForZero(MathF.Cos(ConversionF.DegreesToRadians(degrees)) * magnitude);
        Y = ConversionF.TestForZero(MathF.Sin(ConversionF.DegreesToRadians(degrees)) * magnitude);
        return this;
    }

    public static implicit operator Vector2F(Vector2 vector2)
    {
        return new Vector2F(vector2.X, vector2.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2F other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"{X}, {Y}";
    }

    public static Vector2F Clamp(Vector2F value, float min, float max)
    {
        return new Vector2F(Math.Clamp(value.X, min, max), Math.Clamp(value.Y, min, max));
    }

    public static Vector2F Clamp(Vector2F value, float max)
        => Clamp(value, float.MinValue, max);

    public static Vector2F Clamp(Vector2F value, Vector2F min, Vector2F max)
    {
        return new Vector2F(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
    }

    public static Vector2F Clamp(Vector2F value, Vector2F max)
        => Clamp(value, NegativeInf, max);

    public static Vector2F From(float degrees = 0.0f, float magnitude = 1.0f)
    {
        return new Vector2F().SetFrom(degrees, magnitude);
    }

    public static Vector2F From(double degrees = 0.0, double magnitude = 1.0)
    {
        return new Vector2F().SetFrom(degrees, magnitude);
    }

    public static implicit operator Vector2F((float X, float Y) vector2F)
    {
        return new Vector2F(vector2F.X, vector2F.Y);
    }

    public static implicit operator Vector2F((double X, double Y) vector2F)
    {
        return new Vector2F(vector2F.X, vector2F.Y);
    }

    public static implicit operator Vector2F(SizeF size)
    {
        return new Vector2F(size.Width, size.Height);
    }

    public static bool operator ==(Vector2F left, Vector2F right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2F left, Vector2F right)
    {
        return !(left == right);
    }

    public static Vector2F operator +(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2F operator -(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2F operator *(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X * right.X, left.Y * right.Y);
    }

    public static Vector2F operator /(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X / right.X, left.Y / right.Y);
    }

    public static Vector2F operator /(Vector2F left, float right)
    {
        return new Vector2F(left.X / right, left.Y / right);
    }

    public static Vector2F operator *(Vector2F left, float right)
    {
        return new Vector2F(left.X * right, left.Y * right);
    }

    public static Vector2F Parse(string s, IFormatProvider? provider = null)
    {
        var parsedSuccessfully = TryParse(s, provider, out var result);
        if (!parsedSuccessfully)
        {
            throw new InvalidOperationException("Input was not in a valid format.");
        }
        return result;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Vector2F result)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (FromTuple(s, out result))
            return true;
        return false;
    }

    private static bool FromTuple(string tupleString, out Vector2F size)
    {
        size = new Vector2F();
        var pattern = Regex.Match(tupleString.Trim('(', ')'),
            @"^(\-?\d+(?:\.\d+)?),\s?(\-?\d+(?:\.\d+)?)$", RegexOptions.IgnoreCase);
        if (pattern.Success)
        {
            size.X = float.Parse(pattern.Groups[1].Value);
            size.Y = float.Parse(pattern.Groups[2].Value);
            return true;
        }

        return false;
    }

    private static bool ApproximatelyEqual(float x, float y, float threshold = 0.001f)
        => Math.Abs(x - y) <= threshold;
}