using Silk.NET.Maths;
using System;

namespace MiniEngine;

public struct Size : IEquatable<Size>, IEquatable<SizeF>
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public bool Equals(Size other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public bool Equals(SizeF other)
    {
        return Math.Abs(Width - other.Width) <= 0.001 && Math.Abs(Height - other.Height) <= 0.001;
    }

    public override bool Equals(object? obj)
    {
        return obj is Size other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }

    public static implicit operator Size((int w, int h) size)
    {
        return new Size(size.w, size.h);
    }

    public static implicit operator Size(Vector2 vector)
    {
        return new Size(vector.X, vector.Y);
    }

    public static implicit operator Vector2D<int>(Size size)
    {
        return new Vector2D<int>(size.Width, size.Height);
    }
}