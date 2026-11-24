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
    /// <param name="viewport">The Viewport that is used for the conversions.</param>
    /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
    /// <param name="strokeWidth">StrokeWidth for inflating clipRect</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this Polygon polygon, Viewport viewport, SKRect clipRect, float strokeWidth)
    {
        using var builder = new SKPathBuilder();

        if (polygon.ExteriorRing is null)
            return builder.Detach();

        // Bring outer ring in CCW direction
        var outerRing = (polygon.ExteriorRing.IsRing && ((LinearRing)polygon.ExteriorRing).IsCCW) ? polygon.ExteriorRing : (LineString)((Geometry)polygon.ExteriorRing).Reverse();

        // Reduce exterior ring to parts, that are visible in clipping rectangle
        // Inflate clipRect, so that we could be sure, nothing of stroke is visible on screen
        var exterior = ClippingFunctions.ReducePointsToClipRect(outerRing?.Coordinates, viewport, SKRect.Inflate(clipRect, strokeWidth * 2, strokeWidth * 2));

        if (exterior.Count == 0)
            return builder.Detach();

        // Draw exterior path
        builder.MoveTo(exterior[0]);

        for (var i = 1; i < exterior.Count; i++)
            builder.LineTo(exterior[i]);

        // Close exterior path
        builder.Close();

        foreach (var interiorRing in polygon.InteriorRings)
        {
            // note: For Skia inner rings need to be clockwise and outer rings
            // need to be counter clockwise (if this is the other way around it also
            // seems to work)
            // this is not a requirement of the OGC polygon.

            if (interiorRing is null)
                continue;

            // Bring inner ring in CW direction
            var innerRing = (interiorRing.IsRing && ((LinearRing)interiorRing).IsCCW) ? (LineString)((Geometry)interiorRing).Reverse() : interiorRing;

            // Reduce interior ring to parts, that are visible in clipping rectangle
            var interior = ClippingFunctions.ReducePointsToClipRect(innerRing?.Coordinates, viewport, SKRect.Inflate(clipRect, strokeWidth, strokeWidth));

            if (interior.Count == 0)
                continue;

            // Draw interior paths
            builder.MoveTo(interior[0]);

            for (var i = 1; i < interior.Count; i++)
                builder.LineTo(interior[i]);
        }

        // Close interior paths
        builder.Close();

        return builder.Detach();
    }
}
