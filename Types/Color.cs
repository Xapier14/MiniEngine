using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public struct Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

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
