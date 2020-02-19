using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class SKColorExtensions
    {
        public static Styles.Color ToMapsui(this SKColor color)
        {
            return new Styles.Color(color.Red, color.Green, color.Blue, color.Alpha);
        }
    }
}
