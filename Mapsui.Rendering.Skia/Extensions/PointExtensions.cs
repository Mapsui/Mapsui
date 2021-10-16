using Mapsui.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions
{
    public static class PointExtensions
    {
        public static SKPoint ToSkia(this Point point)
        {
            return new SKPoint((float)point.X, (float)point.Y);
        }
    }
}
