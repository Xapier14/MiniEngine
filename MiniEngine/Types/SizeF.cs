using System;

namespace MiniEngine;

public struct SizeF : IEquatable<SizeF>, IEquatable<Size>
{
    public float Width { get; set; }
    public float Height { get; set; }

    public SizeF(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public bool Equals(SizeF other)
    {
        return Math.Abs(Width - other.Width) <= 0.001 && Math.Abs(Height - other.Height) <= 0.001;
    }

    public bool Equals(Size other)
    {
        return Math.Abs(Width - other.Width) <= 0.001 && Math.Abs(Height - other.Height) <= 0.001;
    }

    public override bool Equals(object? obj)
    {
        return obj is SizeF other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }

    public static implicit operator SizeF((int w, int h) size)
    {
        return new SizeF(size.w, size.h);
    }
}