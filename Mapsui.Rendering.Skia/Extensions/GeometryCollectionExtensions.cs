using NetTopologySuite.Geometries;
using System;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;
public static class GeometryCollectionExtensions
{
    public static SKPath ToSkiaPath(this GeometryCollection geometryCollection, Viewport viewport, SKRect clipRect, float strokeWidth)
    {
        var path = new SKPath();
        foreach (var geometry in geometryCollection)
        {
            SKPath itPath;
            switch (geometry)
            {
                case GeometryCollection collection:
                    itPath = collection.ToSkiaPath(viewport, clipRect, strokeWidth);
                    break;
                case Point point:
                    itPath = point.ToSkiaPath(viewport, clipRect, strokeWidth);
                    break;
                case Polygon polygon:
                    itPath = polygon.ToSkiaPath(viewport, clipRect, strokeWidth);
                    break;
                case LineString lineString:
                    itPath = lineString.ToSkiaPath(viewport, clipRect, strokeWidth);
                    break;
                case null:
                    throw new ArgumentException($"Geometry is null");
                default:
                    throw new ArgumentException($"Unknown geometry type: {geometry?.GetType()}");
            }

            path.AddPath(itPath);
        }

        return path;
    }
}
