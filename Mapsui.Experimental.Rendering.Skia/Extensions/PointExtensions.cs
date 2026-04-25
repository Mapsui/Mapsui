using Mapsui.Extensions;
using NetTopologySuite.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

public static class PointExtensions
{
    /// <summary>
    /// Converts a Point in world coordinates to a Skia path
    /// </summary>
    /// <param name="point">Point in Mapsui world coordinates.</param>
    /// <param name="viewport">The Viewport that is used for the conversions.</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this Point point, Viewport viewport)
    {
        var width = (float)SymbolStyle.DefaultWidth;
        var halfWidth = width / 2;
        var (screenX, screenY) = viewport.WorldToScreenXY(point.X, point.Y);

        var skPath = new SKPath();
        skPath.AddCircle((float)screenX, (float)screenY, halfWidth);

        return skPath;
    }
}
