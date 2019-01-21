using Mapsui.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class BoundingBoxExtensions
    {
        public static SKRect ToSkia(this BoundingBox boundingBox)
        {
            return new SKRect((float)boundingBox.MinX, (float)boundingBox.MinY, (float)boundingBox.MaxX, (float)boundingBox.MaxY);
        }
    }
}
