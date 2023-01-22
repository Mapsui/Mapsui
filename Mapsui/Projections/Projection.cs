using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Projections;

/// <summary>
/// A very minimal implementation that is only capable of projecting between
/// SphericalMercator and WGS84.
/// </summary>
public class Projection : IProjection
{
    private readonly IDictionary<string, Func<double, double, (double, double)>> _toLonLat =
        new Dictionary<string, Func<double, double, (double, double)>>();

    private readonly IDictionary<string, Func<double, double, (double, double)>> _fromLonLat =
        new Dictionary<string, Func<double, double, (double, double)>>();

    public Projection()
    {
        _toLonLat["EPSG:4326"] = (x, y) => (x, y);
        _fromLonLat["EPSG:4326"] = (x, y) => (x, y);
        _toLonLat["EPSG:3857"] = SphericalMercator.ToLonLat;
        _fromLonLat["EPSG:3857"] = SphericalMercator.FromLonLat;
    }

    public (double X, double Y) Project(string fromCRS, string toCRS, double x, double y)
    {
        var (lon, lat) = Project(x, y, _toLonLat[fromCRS]);
        return Project(lon, lat, _fromLonLat[toCRS]);
    }

    private static (double X, double Y) Project(double x, double y, Func<double, double, (double, double)> projectFunc)
    {
        return projectFunc(x, y);
    }

    public bool IsProjectionSupported([NotNullWhen(true)] string? fromCRS, [NotNullWhen(true)] string? toCRS)
    {
        if (fromCRS == null || toCRS == null)
            return false;
        return _toLonLat.ContainsKey(fromCRS) && _fromLonLat.ContainsKey(toCRS);
    }

    public void Project(string fromCRS, string toCRS, MPoint point)
    {
        if (!IsProjectionSupported(fromCRS, toCRS))
            throw new NotSupportedException($"Projection is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

        Project(point, _toLonLat[fromCRS]);
        Project(point, _fromLonLat[toCRS]);
    }

    private static void Project(MPoint point, Func<double, double, (double, double)> projectFunc)
    {
        (point.X, point.Y) = projectFunc(point.X, point.Y);
    }

    public void Project(string fromCRS, string toCRS, MRect rect)
    {
        if (!IsProjectionSupported(fromCRS, toCRS))
            throw new NotSupportedException($"Projection is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

        Project(rect.Min, _toLonLat[fromCRS]);
        Project(rect.Min, _fromLonLat[toCRS]);

        Project(rect.Max, _toLonLat[fromCRS]);
        Project(rect.Max, _fromLonLat[toCRS]);
    }

    public void Project(string fromCRS, string toCRS, IFeature feature)
    {
        if (!IsProjectionSupported(fromCRS, toCRS))
            throw new NotSupportedException($"Projection is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

        Project(feature, _toLonLat[fromCRS]);
        Project(feature, _fromLonLat[toCRS]);
    }

    public void Project(string fromCRS, string toCRS, IEnumerable<IFeature> features)
    {
        Project(features, _toLonLat[fromCRS]);
        Project(features, _fromLonLat[toCRS]);
    }

    private static void Project(IFeature feature, Func<double, double, (double, double)> transformFunc)
    {
        feature.CoordinateVisitor((x, y, setter) =>
        {
            var (xOut, yOut) = transformFunc(x, y);
            setter(xOut, yOut);
        });
    }

    private static void Project(IEnumerable<IFeature> features, Func<double, double, (double, double)> transformFunc)
    {
        foreach (var feature in features)
            feature.CoordinateVisitor((x, y, setter) =>
            {
                var (xOut, yOut) = transformFunc(x, y);
                setter(xOut, yOut);
            });
    }
}
