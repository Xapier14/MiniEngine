using System;

namespace MiniEngine
{
    public struct Color
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

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

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
        {
            throw new NotImplementedException();
        }
    }
}
