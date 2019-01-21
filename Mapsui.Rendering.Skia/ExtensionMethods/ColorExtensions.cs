using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class SkiaExtensions
    {
        public static SKColor ToSkia(this Color color, float layerOpacity)
        {
            if (color == null) return new SKColor(128, 128, 128, 0);
            return new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)(color.A * layerOpacity));
        }
    }
}