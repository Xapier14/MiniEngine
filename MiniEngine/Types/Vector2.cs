﻿using MiniEngine.Utility;
using System;
using Silk.NET.Maths;

namespace MiniEngine;

public struct Vector2 : IEquatable<Vector2>, IEquatable<Vector2F>, IEquatable<(int X, int Y)>, IEquatable<(float X, float Y)>
{
    public static Vector2 Zero => new(0, 0);

    public double Magnitude => Math.Sqrt(Math.Pow(X, 2.0) + Math.Pow(Y, 2.0));
    public double Angle => Conversion.RadiansToDegrees(Math.Atan((double)Y / X));

    public int X { get; set; }
    public int Y { get; set; }

    public Vector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Vector2 other)
    {
        return X == other.X && Y == other.Y;
    }

    public bool Equals((int X, int Y) other)
    {
        return X == other.X && Y == other.Y;
    }

    public bool Equals((float X, float Y) other)
    {
        return Math.Abs(X - other.X) <= 0.001 && Math.Abs(Y - other.Y) <= 0.001;
    }

    public bool Equals(Vector2F other)
    {
        return Math.Abs(X - other.X) <= 0.001 && Math.Abs(Y - other.Y) <= 0.001;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"{X}, {Y}";
    }

    public Vector2 Set(int x, int y)
    {
        X = x;
        Y = y;
        return this;
    }

    public Vector2 SetFrom(double degrees, double magnitude)
    {
        X = (int)Math.Round(Math.Cos(Conversion.DegreesToRadians(degrees)) * magnitude);
        Y = (int)Math.Round(Math.Sin(Conversion.DegreesToRadians(degrees)) * magnitude);
        return this;
    }

    public static Vector2 From(double degrees = 0.0, double magnitude = 1.0)
    {
        return new Vector2().SetFrom(degrees, magnitude);
    }

    public static implicit operator Vector2((int x, int y) vector2)
    {
        return new Vector2(vector2.x, vector2.y);
    }

    public static implicit operator Vector2(Size size)
    {
        return new Vector2(size.Width, size.Height);
    }

    public static implicit operator Vector2D<int>(Vector2 vector2)
    {
        return new Vector2D<int>(vector2.X, vector2.Y);
    }

    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2 left, Vector2 right)
    {
        return !(left == right);
    }

    public static Vector2 operator +(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2 operator -(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2 operator *(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X * right.X, left.Y * right.Y);
    }

    public static Vector2 operator /(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X / right.X, left.Y / right.Y);
    }
}