using Mapsui.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions
{
    internal static class SKRectExtensions
    {
        public static BoundingBox ToMapsui(this SKRect rect)
        {
            return new BoundingBox(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static MRect ToMRect(this SKRect rect)
        {
            return new MRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
}
