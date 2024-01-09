using MiniEngine.Utility;
using System;
using System.Text.RegularExpressions;

namespace MiniEngine;

public struct Vector2F : IEquatable<Vector2F>, IEquatable<Vector2>, IEquatable<(int X, int Y)>, IEquatable<(float X, float Y)>, IParsable<Vector2F>
{
    public static Vector2F Zero => new(0, 0);

    public float Magnitude => MathF.Sqrt(MathF.Pow(X, 2f) + MathF.Pow(Y, 2f));
    public float Angle => ConversionF.RadiansToDegrees(MathF.Atan(Y / X));

    public float X { get; set; }
    public float Y { get; set; }

    public Vector2F(float x, float y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Vector2 other)
    {
        return Math.Abs(X - other.X) <= 0.001 && Math.Abs(Y - other.Y) <= 0.001;
    }

    public bool Equals((int X, int Y) other)
    {
        return Math.Abs(X - other.X) <= 0.001 && Math.Abs(Y - other.Y) <= 0.001;
    }

    public bool Equals((float X, float Y) other)
    {
        return Math.Abs(X - other.X) <= 0.001 && Math.Abs(Y - other.Y) <= 0.001;
    }

    public bool Equals(Vector2F other)
    {
        return Math.Abs(X - other.X) <= 0.001 && Math.Abs(Y - other.Y) <= 0.001;
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

    public static Vector2F Clamp(Vector2F value, Vector2F min, Vector2F max)
    {
        return new Vector2F(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
    }

    public static Vector2F From(float degrees = 0.0f, float magnitude = 1.0f)
    {
        return new Vector2F().SetFrom(degrees, magnitude);
    }

    public static implicit operator Vector2F((float X, float Y) vector2F)
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

    public static Vector2F operator -(Vector2F left, Vector2 right)
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
        if (FromTuple(s, out result))
            return true;
        return false;
    }

    private static bool FromTuple(string tupleString, out Vector2F size)
    {
        size = new Vector2F();
        var pattern = Regex.Match(tupleString.Trim('(', ')'),
            @"^(\d+(?:\.\d+)?),\s?(\d+(?:\.\d+)?)$", RegexOptions.IgnoreCase);
        if (pattern.Success)
        {
            size.X = float.Parse(pattern.Groups[1].Value);
            size.Y = float.Parse(pattern.Groups[2].Value);
            return true;
        }

        return false;
    }
}