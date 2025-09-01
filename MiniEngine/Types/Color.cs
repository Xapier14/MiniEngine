using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniEngine
{
    public struct Color : IParsable<Color>
    {
        public static Color Black { get; } = new(0);
        public static Color White { get; } = new(255);
        public static Color LightGray { get; } = new(224);
        public static Color Gray { get; } = new(128);
        public static Color DarkGray { get; } = new(64);
        public static Color Red { get; } = new(255, 0, 0);
        public static Color Pink { get; } = new(255, 98, 208);
        public static Color Purple { get; } = new(160, 32, 255);
        public static Color Cyan { get; } = new(80, 208, 255);
        public static Color Blue { get; } = new(0, 32, 255);
        public static Color LightGreen { get; } = new(96, 255, 128);
        public static Color Green { get; } = new(0, 192, 0);
        public static Color Lime { get; } = new(0, 255, 0);
        public static Color Yellow { get; } = new(255, 224, 32);
        public static Color Orange { get; } = new(255, 160, 16);
        public static Color Brown { get; } = new(160, 128, 96);
        public static Color Tan { get; } = new(255, 208, 160);
        public static Color Transparent { get; } = new(255, 255, 255, 0);

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public Color()
        {
            R = 255;
            G = 255;
            B = 255;
            A = 255;
        }

        public Color(byte gs)
        {
            R = gs;
            G = gs;
            B = gs;
            A = 255;
        }

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = 255;
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static implicit operator Color((byte r, byte g, byte b) color)
        {
            return new Color(color.r, color.g, color.b);
        }

        public static implicit operator Color((byte r, byte g, byte b, byte a) color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }

        public static implicit operator Color(string hex)
            => Parse(hex);

        public static Color Parse(string s, IFormatProvider? provider = null)
        {
            TryParse(s, provider, out var color);
            return color;
        }

        public static bool TryParse(string? s, IFormatProvider? provider, out Color result)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s), "String to parse must not be null.");
            if (ParseHex(s, out result))
                return true;
            return false;
        }

        private static byte FromHex(string str)
            => (byte)int.Parse(str, NumberStyles.HexNumber);

        private static string Repeat(string str, int repetition)
        {
            StringBuilder builder = new();
            for (var i = 0; i < repetition; ++i)
                builder.Append(str);
            return builder.ToString();
        }

        private static bool ParseHex(string hexColor, out Color color)
        {
            color = new Color();
            // #RGB
            var pattern1 = Regex.Match(hexColor,
                @"^#([\da-f])([\da-f])([\da-f])$", RegexOptions.IgnoreCase);
            if (pattern1.Success)
            {
                color.R = FromHex(Repeat(pattern1.Groups[1].Value, 2));
                color.G = FromHex(Repeat(pattern1.Groups[2].Value, 2));
                color.B = FromHex(Repeat(pattern1.Groups[3].Value, 2));
                return true;
            }

            // #RGBA
            var pattern2 = Regex.Match(hexColor,
                @"^#([\da-f])([\da-f])([\da-f])([\da-f])$", RegexOptions.IgnoreCase);
            if (pattern2.Success)
            {
                color.R = FromHex(Repeat(pattern2.Groups[1].Value, 2));
                color.G = FromHex(Repeat(pattern2.Groups[2].Value, 2));
                color.B = FromHex(Repeat(pattern2.Groups[3].Value, 2));
                color.A = FromHex(Repeat(pattern2.Groups[4].Value, 2));
                return true;
            }

            // #RRGGBB
            var pattern3 = Regex.Match(hexColor,
                @"^#([\da-f]{2})([\da-f]{2})([\da-f]{2})$", RegexOptions.IgnoreCase);
            if (pattern3.Success)
            {
                color.R = FromHex(pattern3.Groups[1].Value[..2]);
                color.G = FromHex(pattern3.Groups[2].Value[..2]);
                color.B = FromHex(pattern3.Groups[3].Value[..2]);
                return true;
            }

            // #RRGGBBAA
            var pattern4 = Regex.Match(hexColor,
                @"^#([\da-f]{2})([\da-f]{2})([\da-f]{2})([\da-f]{2})$", RegexOptions.IgnoreCase);
            if (pattern4.Success)
            {
                color.R = FromHex(pattern4.Groups[1].Value[..2]);
                color.G = FromHex(pattern4.Groups[2].Value[..2]);
                color.B = FromHex(pattern4.Groups[3].Value[..2]);
                color.A = FromHex(pattern4.Groups[4].Value[..2]);
                return true;
            }

            return false;
        }

    }
}
