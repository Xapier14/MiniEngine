using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MiniEngine;

public struct SizeF : IEquatable<SizeF>, IEquatable<Size>, IParsable<SizeF>
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

    public static implicit operator SizeF(Vector2F vector)
    {
        return new SizeF(vector.X, vector.Y);
    }

    public static SizeF Parse(string s, IFormatProvider? provider = null)
    {
        var parsedSuccessfully = TryParse(s, provider, out var result);
        if (!parsedSuccessfully)
        {
            throw new InvalidOperationException("Input was not in a valid format.");
        }
        return result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out SizeF result)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (FromCross(s, out result))
            return true;
        if (FromTuple(s, out result))
            return true;
        return false;
    }

    private static bool FromCross(string crossString, out SizeF size)
    {
        size = new SizeF();
        var pattern = Regex.Match(crossString,
            @"^(\d+(?:\.\d+)?)x(\d+(?:\.\d+)?)$", RegexOptions.IgnoreCase);
        if (pattern.Success)
        {
            size.Width = float.Parse(pattern.Groups[1].Value);
            size.Height = float.Parse(pattern.Groups[2].Value);
            return true;
        }

        return false;
    }

    private static bool FromTuple(string tupleString, out SizeF size)
    {
        size = new SizeF();
        var pattern = Regex.Match(tupleString.Trim('(', ')'),
            @"^(\d+(?:\.\d+)?),\s?(\d+(?:\.\d+)?)$", RegexOptions.IgnoreCase);
        if (pattern.Success)
        {
            size.Width = float.Parse(pattern.Groups[1].Value);
            size.Height = float.Parse(pattern.Groups[2].Value);
            return true;
        }

        return false;
    }
}