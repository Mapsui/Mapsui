using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

public static class CoordinateExtensions
{
    public static SKPoint ToSkiaPoint(this Coordinate coordinate)
    {
        return new SKPoint((float)coordinate.X, (float)coordinate.Y);
    }
}
