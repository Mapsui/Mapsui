using Mapsui.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class PointExtensions
    {
        public static SKPoint ToSkia(this Point point)
        {
            return new SKPoint((float)point.X, (float)point.Y);
        }
    }
}
