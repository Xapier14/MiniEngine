using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MiniEngine;

public interface IReadOnlySize
{
    public int Width { get; }
    public int Height { get; }
}

public struct Size : IReadOnlySize, IEquatable<Size>, IEquatable<SizeF>, IParsable<Size>
{
    public int Width { get; set; }
    public int Height { get; set; }
    public static Size NoSize = new(-1, -1);

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

    public static Size Parse(string s, IFormatProvider? provider = null)
    {
        var parsedSuccessfully = TryParse(s, provider, out var result);
        if (!parsedSuccessfully)
        {
            throw new InvalidOperationException("Input was not in a valid format.");
        }
        return result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Size result)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (FromCross(s, out result))
            return true;
        if (FromTuple(s, out result))
            return true;
        return false;
    }

    public static implicit operator Size((int w, int h) size)
    {
        return new Size(size.w, size.h);
    }

    public static implicit operator Size(Vector2 vector)
    {
        return new Size(vector.X, vector.Y);
    }

    private static bool FromCross(string crossString, out Size size)
    {
        size = new Size();
        var pattern = Regex.Match(crossString,
            @"^(\d+)x(\d+)$", RegexOptions.IgnoreCase);
        if (pattern.Success)
        {
            size.Width = int.Parse(pattern.Groups[1].Value);
            size.Height = int.Parse(pattern.Groups[2].Value);
            return true;
        }

        return false;
    }

    private static bool FromTuple(string tupleString, out Size size)
    {
        size = new Size();
        var pattern = Regex.Match(tupleString.Trim('(', ')'),
            @"^(\d+),\s?(\d+)$", RegexOptions.IgnoreCase);
        if (pattern.Success)
        {
            size.Width = int.Parse(pattern.Groups[1].Value);
            size.Height = int.Parse(pattern.Groups[2].Value);
            return true;
        }

        return false;
    }
}