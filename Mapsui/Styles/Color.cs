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
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public Color(int red, int green, int blue, int alpha = 255)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }
        
        public static Color Transparent => new Color {A = 0, R = 255, G = 255, B = 255};
        public static Color Black => new Color {A = 255, R = 0, G = 0, B = 0};
        public static Color White => new Color {A = 255, R = 255, G = 255, B = 255};
        public static Color Gray => new Color {A = 255, R = 128, G = 128, B = 128};
        public static Color Red => new Color {A = 255, R = 255, G = 0, B = 0};
        public static Color Yellow => new Color {A = 255, R = 255, G = 255, B = 0};
        public static Color Green => new Color {A = 255, R = 0, G = 128, B = 0};
        public static Color Cyan => new Color {A = 255, R = 0, G = 255, B = 255};
        public static Color Blue => new Color {A = 255, R = 0, G = 0, B = 255};
        public static Color Orange => new Color {A = 255, R = 255, G = 165, B = 0};
        public static Color Indigo => new Color {A = 255, R = 75, G = 0, B = 130};
        public static Color Violet => new Color {A = 255, R = 238, G = 130, B = 238};

        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color {A = a, R = r, G = g, B = b};
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is Color))
                return false;
            return Equals((Color) obj);
        }

        public bool Equals(Color color)
        {
            if (R != color.R) return false;
            if (G != color.G) return false;
            if (B != color.B) return false;
            if (A != color.A) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
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