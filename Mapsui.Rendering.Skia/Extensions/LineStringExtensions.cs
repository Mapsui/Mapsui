using Mapsui.Rendering.Skia.Functions;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

internal static class LineStringExtensions
{
    /// <summary>
    /// Converts a LineString in world coordinates to a Skia path
    /// </summary>
    /// <param name="lineString">List of points in Mapsui world coordinates.</param>
    /// <param name="viewport">The Viewport that is used for the conversions.</param>
    /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this LineString lineString, Viewport viewport, SKRect clipRect)
    {
        var coordinates = lineString.Coordinates;

        // First convert List<Points> to screen coordinates
        var vertices = ClippingFunctions.WorldToScreen(viewport, coordinates);

        var path = new SKPath();
        var lastPoint = SKPoint.Empty;

        for (var i = 1; i < vertices.Count; i++)
        {
            // Check each part of LineString, if it is inside or intersects the clipping rectangle
            var intersect = ClippingFunctions.LiangBarskyClip(vertices[i - 1], vertices[i], clipRect, out var intersectionPoint1, out var intersectionPoint2);

            if (intersect != ClippingFunctions.Intersection.CompleteOutside)
            {
                // If the last point isn't the same as actual starting point ...
                if (lastPoint.IsEmpty || !lastPoint.Equals(intersectionPoint1))
                    // ... than move to this point
                    path.MoveTo(intersectionPoint1);
                // Draw line
                path.LineTo(intersectionPoint2);

                // Save last end point for later use
                lastPoint = intersectionPoint2;
            }
        }
        return path;
    }

    /// <summary>
    /// Converts a LineString in world coordinates to a Skia path
    /// </summary>
    /// <param name="lineString">List of points in Mapsui world coordinates</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this LineString lineString)
    {
        var coordinates = lineString.Coordinates;

        // First convert List<Points> to screen coordinates
        var path = new SKPath();
        var lastPoint = new SKPoint((float)coordinates[0].X, (float)coordinates[0].Y);
        path.MoveTo(lastPoint);

        for (var i = 1; i < coordinates.Length; i++)
        {
            lastPoint = new SKPoint((float)coordinates[i].X, (float)coordinates[i].Y);
            path.LineTo(lastPoint);
        }

        return path;
    }
}
