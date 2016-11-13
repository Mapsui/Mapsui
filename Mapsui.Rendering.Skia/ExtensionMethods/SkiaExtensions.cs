using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    static class SkiaExtensions
    {
        public static SKColor ToSkia(this Color color)
        {
            if (color == null) return new SKColor(128, 128, 128, 0);
            return new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
        }

        public static SKRect ToSkia(this BoundingBox boundingBox)  
        {
            return new SKRect((float)boundingBox.MinX, (float)boundingBox.MinY, (float)boundingBox.MaxX, (float)boundingBox.MaxY);
        }
    }
}