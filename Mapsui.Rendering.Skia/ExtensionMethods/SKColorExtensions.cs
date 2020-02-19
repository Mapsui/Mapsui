using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class SKColorExtensions
    {
        public static Color ToMapsui(this SKColor color)
        {
            return new Color(color.Red, color.Green, color.Blue, color.Alpha);
        }
    }
}
