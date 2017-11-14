using Mapsui.Styles;

namespace Mapsui.VectorTiles
{
    public static class ColorConverter
    {
        public static Color HlsToRgb(double h, double l, double s)
        {
            // This is relevant:
            // http://www.easyrgb.com/en/math.php
            int r, g, b;
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double doubleR, doubleG, doubleB;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (s == 0)
            {
                doubleR = l;
                doubleG = l;
                doubleB = l;
            }
            else
            {
                doubleR = QqhToRgb(p1, p2, h + 120);
                doubleG = QqhToRgb(p1, p2, h);
                doubleB = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 256 range.
            r = (int)(doubleR * 256.0);
            g = (int)(doubleG * 256.0);
            b = (int)(doubleB * 256.0);

            return new Color(r, g, b);
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }
    }
}
