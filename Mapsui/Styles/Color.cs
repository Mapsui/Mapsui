using System;
using System.Linq;

namespace Mapsui.Styles
{
    public class Color
    {
        public Color()
        {
            A = 255;
        }

        public Color(Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color { A = a, R = r, G = g, B = b };
        }

        public int A { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public static Color Black { get { return new Color { A = 255, R = 0, G = 0, B = 0 }; } }
        public static Color White { get { return new Color { A = 255, R = 255, G = 255, B = 255 }; } }
        public static Color Gray { get { return new Color { A = 255, R = 128, G = 128, B = 128 }; } }

        public static Color Red { get { return new Color { A = 255, R = 255, G = 0, B = 0 }; } }
        public static Color Yellow { get { return new Color { A = 255, R = 255, G = 255, B = 0 }; } }
        public static Color Green { get { return new Color { A = 255, R = 0, G = 128, B = 0 }; } }
        public static Color Cyan { get { return new Color { A = 255, R = 0, G = 255, B = 255 }; } }
        public static Color Blue { get { return new Color { A = 255, R = 0, G = 0, B = 255 }; } }

        public static Color Orange { get { return new Color { A = 255, R = 255, G = 165, B = 0 }; } }
        public static Color Indigo { get { return new Color { A = 255, R = 75, G = 0, B = 130 }; } }
        public static Color Violet { get { return new Color { A = 255, R = 238, G = 130, B = 238 }; } }

        
        public override bool Equals(object obj)
        {
            if (!(obj is Color))
            {
                return false;
            }
            return Equals((Color)obj);
        }

        public bool Equals(Color color)
        {
            if (A != color.A) return false;
            if (R != color.R) return false;
            if (G != color.G) return false;
            if (B != color.B) return false; 
            return true;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        public static bool operator ==(Color color1, Color color2)
        {
            return Equals(color1, color2);
        }

        public static bool operator !=(Color color1, Color color2)
        {
            return !Equals(color1, color2);
        }

            }
}
