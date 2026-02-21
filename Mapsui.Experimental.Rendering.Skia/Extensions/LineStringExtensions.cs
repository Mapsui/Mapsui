using Mapsui.Extensions;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

internal static class LineStringExtensions
{
    /// <summary>
    /// Converts a LineString in world coordinates to a Skia path
    /// </summary>
    /// <param name="lineString">List of points in Mapsui world coordinates.</param>
    /// <param name="viewport">The Viewport that is used for the conversions.</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this LineString lineString, Viewport viewport)
    {
        var coordinates = lineString.Coordinates;
        var path = new SKPath();

        if (coordinates.Length == 0)
            return path;

        var (startX, startY) = viewport.WorldToScreenXY(coordinates[0].X, coordinates[0].Y);
        path.MoveTo((float)startX, (float)startY);

        for (var i = 1; i < coordinates.Length; i++)
        {
            var (screenX, screenY) = viewport.WorldToScreenXY(coordinates[i].X, coordinates[i].Y);
            path.LineTo((float)screenX, (float)screenY);
        }

        return path;
    }

    /// <summary>
    /// Converts a LineString into a SKPath using coordinates relative to a reference point.
    /// Using relative coordinates keeps float values small, avoiding precision loss for large world coordinates.
    /// The path is intended to be transformed to screen coordinates at draw time using a matrix,
    /// after translating to the reference point.
    /// </summary>
    /// <param name="lineString">LineString in Mapsui world coordinates.</param>
    /// <param name="referenceX">Reference X coordinate (typically centroid) to subtract from all points</param>
    /// <param name="referenceY">Reference Y coordinate (typically centroid) to subtract from all points</param>
    /// <returns>SKPath in relative world coordinates (centered around origin)</returns>
    public static SKPath ToWorldPath(this LineString lineString, double referenceX, double referenceY)
    {
        var coordinates = lineString.Coordinates;
        var path = new SKPath();

        if (coordinates.Length == 0)
            return path;

        // Subtract reference point to keep float values small and preserve precision
        path.MoveTo((float)(coordinates[0].X - referenceX), (float)(coordinates[0].Y - referenceY));
        for (var i = 1; i < coordinates.Length; i++)
            path.LineTo((float)(coordinates[i].X - referenceX), (float)(coordinates[i].Y - referenceY));

        return path;
    }
}
