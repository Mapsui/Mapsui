using System;
using Mapsui.Styles;

namespace Mapsui.VectorTiles
{
    public static class ColorParser
    {
        public static Color HslToColor(string color)
        {
            var start = color.IndexOf("(",StringComparison.Ordinal) + 1;
            var end = color.IndexOf(")", StringComparison.Ordinal);

            var values = color.Substring(start, end - start);
            var numbers = values.Split(',');
    
            var hue = float.Parse(numbers[0]) ;
            var saturation = float.Parse(numbers[1].Replace("%", "")) / 100f;
            var luminosity = float.Parse(numbers[2].Replace("%", "")) / 100f;

            return ColorConverter.HlsToRgb(hue, luminosity, saturation);
        }

        public static Color ToColorFromRgba(string color)
        {
            var start = color.IndexOf("(", StringComparison.Ordinal) + 1;
            var end = color.IndexOf(")", StringComparison.Ordinal);

            var values = color.Substring(start, end - start);
            var numbers = values.Split(',');

            var r = int.Parse(numbers[0]);
            var g = int.Parse(numbers[1]);
            var b = int.Parse(numbers[2]);

            return new Color(r, g, b);
        }
    }
}
