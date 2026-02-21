using Mapsui.Extensions;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System.Collections.Generic;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

internal static class PolygonExtensions
{
    /// <summary>
    /// Converts a Polygon into a SKPath using world-to-screen coordinate conversion.
    /// </summary>
    /// <param name="polygon">Polygon to convert</param>
    /// <param name="viewport">The Viewport that is used for the conversions.</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this Polygon polygon, Viewport viewport)
    {
        var path = new SKPath();

        if (polygon.ExteriorRing is null)
            return path;

        // Bring outer ring in CCW direction
        var outerRing = (polygon.ExteriorRing.IsRing && ((LinearRing)polygon.ExteriorRing).IsCCW) ? polygon.ExteriorRing : (LineString)((Geometry)polygon.ExteriorRing).Reverse();

        var exterior = WorldToScreen(viewport, outerRing?.Coordinates);

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

            if (interiorRing is null)
                continue;

            // Bring inner ring in CW direction
            var innerRing = (interiorRing.IsRing && ((LinearRing)interiorRing).IsCCW) ? (LineString)((Geometry)interiorRing).Reverse() : interiorRing;

            var interior = WorldToScreen(viewport, innerRing?.Coordinates);

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

    private static List<SKPoint> WorldToScreen(Viewport viewport, IEnumerable<Coordinate>? coordinates)
    {
        var result = new List<SKPoint>();
        if (coordinates == null)
            return result;

        foreach (var coordinate in coordinates)
        {
            var (screenX, screenY) = viewport.WorldToScreenXY(coordinate.X, coordinate.Y);
            result.Add(new SKPoint((float)screenX, (float)screenY));
        }

        return result;
    }

    /// <summary>
    /// Converts a Polygon into a SKPath using coordinates relative to a reference point.
    /// Using relative coordinates keeps float values small, avoiding precision loss for large world coordinates.
    /// The path is intended to be transformed to screen coordinates at draw time using a matrix,
    /// after translating to the reference point.
    /// </summary>
    /// <param name="polygon">Polygon to convert</param>
    /// <param name="referenceX">Reference X coordinate (typically centroid) to subtract from all points</param>
    /// <param name="referenceY">Reference Y coordinate (typically centroid) to subtract from all points</param>
    /// <returns>SKPath in relative world coordinates (centered around origin)</returns>
    public static SKPath ToWorldPath(this Polygon polygon, double referenceX, double referenceY)
    {
        var path = new SKPath();

        if (polygon.ExteriorRing is null)
            return path;

        // Bring outer ring in CCW direction
        var outerRing = (polygon.ExteriorRing.IsRing && ((LinearRing)polygon.ExteriorRing).IsCCW) ? polygon.ExteriorRing : (LineString)((Geometry)polygon.ExteriorRing).Reverse();

        var coords = outerRing?.Coordinates;
        if (coords == null || coords.Length == 0)
            return path;

        // Subtract reference point to keep float values small and preserve precision
        path.MoveTo((float)(coords[0].X - referenceX), (float)(coords[0].Y - referenceY));
        for (var i = 1; i < coords.Length; i++)
            path.LineTo((float)(coords[i].X - referenceX), (float)(coords[i].Y - referenceY));
        path.Close();

        foreach (var interiorRing in polygon.InteriorRings)
        {
            // note: For Skia inner rings need to be clockwise and outer rings
            // need to be counter clockwise (if this is the other way around it also
            // seems to work). The world-to-screen matrix includes a Y-flip which
            // reverses both windings equally, so they remain opposite.

            if (interiorRing is null)
                continue;

            // Bring inner ring in CW direction
            var innerRing = (interiorRing.IsRing && ((LinearRing)interiorRing).IsCCW) ? (LineString)((Geometry)interiorRing).Reverse() : interiorRing;

            var innerCoords = innerRing?.Coordinates;
            if (innerCoords == null || innerCoords.Length == 0)
                continue;

            path.MoveTo((float)(innerCoords[0].X - referenceX), (float)(innerCoords[0].Y - referenceY));
            for (var i = 1; i < innerCoords.Length; i++)
                path.LineTo((float)(innerCoords[i].X - referenceX), (float)(innerCoords[i].Y - referenceY));
        }

        path.Close();

        return path;
    }
}
