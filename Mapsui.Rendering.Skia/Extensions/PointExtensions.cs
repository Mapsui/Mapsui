using System;
using NetTopologySuite.Geometries;
using Mapsui.Rendering.Skia.Functions;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;
public static class PointExtensions
{
    /// <summary>
    /// Converts a LineString in world coordinates to a Skia path
    /// </summary>
    /// <param name="point">List of points in Mapsui world coordinates.</param>
    /// <param name="viewport">The Viewport that is used for the conversions.</param>
    /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
    /// <param name="strokeWidth">stroke Width</param>
    /// <returns></returns>
    public static SKPath ToSkiaPath(this Point point, Viewport viewport, SKRect clipRect, float strokeWidth) 
    {
        var width = (float)SymbolStyle.DefaultWidth;
        var halfWidth = width / 2;
        var points = ClippingFunctions.ReducePointsToClipRect(new[] { point.Coordinate }, viewport, SKRect.Inflate(clipRect, width + strokeWidth, width + strokeWidth));
        var transformed = points[0];

        var skPath = new SKPath();
        skPath.AddCircle(Convert.ToSingle(transformed.X), Convert.ToSingle(transformed.Y), halfWidth);

        return skPath;
    }
}
