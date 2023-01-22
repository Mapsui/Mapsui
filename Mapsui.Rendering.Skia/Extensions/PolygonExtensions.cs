using Mapsui.Rendering.Skia.Functions;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

internal static class PolygonExtensions
{
    /// <summary>
    /// Converts a Polygon into a SKPath, that is clipped to clipRect, where exterior is bigger than interior
    /// </summary>
    /// <param name="polygon">Polygon to convert</param>
    /// <param name="viewport">Viewport implementation</param>
    /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
    /// <param name="strokeWidth">StrokeWidth for inflating clipRect</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this Polygon polygon, IReadOnlyViewport viewport, SKRect clipRect, float strokeWidth)
    {
        // Reduce exterior ring to parts, that are visible in clipping rectangle
        // Inflate clipRect, so that we could be sure, nothing of stroke is visible on screen
        var exterior = ClippingFunctions.ReducePointsToClipRect(polygon.ExteriorRing?.Coordinates, viewport, SKRect.Inflate(clipRect, strokeWidth * 2, strokeWidth * 2));

        // Create path for exterior and interior parts
        var path = new SKPath();

        if (exterior.Count == 0)
            return path;

        // Draw exterior path
        path.MoveTo(exterior[0]);

        for (var i = 1; i < exterior.Count; i++)
            path.LineTo(exterior[i]);

        // Close exterior path
        path.Close();

        foreach (var interiorRing in polygon.InteriorRings)
        {
            // note: For Skia inner rings need to be clockwise and outer rings
            // need to be counter clockwise (if this is the other way around it also
            // seems to work)
            // this is not a requirement of the OGC polygon.

            // Reduce interior ring to parts, that are visible in clipping rectangle
            var interior = ClippingFunctions.ReducePointsToClipRect(interiorRing.Coordinates, viewport, SKRect.Inflate(clipRect, strokeWidth, strokeWidth));

            if (interior.Count == 0)
                continue;

            // Draw interior paths
            path.MoveTo(interior[0]);

            for (var i = 1; i < interior.Count; i++)
                path.LineTo(interior[i]);
        }

        // Close interior paths
        path.Close();

        return path;
    }
}
