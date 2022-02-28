using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Mapsui.Nts;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using IProjection = Mapsui.Projections.IProjection;

namespace Mapsui.Extensions.Projections;

public class NtsProjection : IProjection
{
    private static readonly CoordinateSystemFactory CoordinateSystemFactory;
    private static readonly CoordinateTransformationFactory CoordinateTransformationFactory;
    private static readonly ConcurrentDictionary<int, CoordinateSystem> Projections;
    private static readonly ConcurrentDictionary<(int From, int To), ICoordinateTransformation> Transformations;
    private static readonly ConcurrentDictionary<(int From, int To),GeometryTransform> GeometryTransformations;

    static NtsProjection()
    {
        DefaultProjectionFactory.Create = () => new NtsProjection();
        CoordinateSystemFactory = new CoordinateSystemFactory();
        CoordinateTransformationFactory = new CoordinateTransformationFactory();
        Projections = new ConcurrentDictionary<int, CoordinateSystem>();
        Transformations = new ConcurrentDictionary<(int From, int To), ICoordinateTransformation>();
        GeometryTransformations = new ConcurrentDictionary<(int From, int To), GeometryTransform>();
    }

    private static StreamReader GetStreamReader()
    {
        var assembly = typeof(NtsProjection).Assembly;
        var fullName = assembly.GetFullName("Projections.SRID.csv.gz");
        var stream = assembly.GetManifestResourceStream(fullName);
        var gzipStream = new GZipStream(stream!, CompressionMode.Decompress);
        var reader = new StreamReader(gzipStream!, Encoding.UTF8);
        return reader;
    }

    public int? GetIdFromCrs(string? crs)
    {
        if (crs == null) return null;

        var splits = crs.Split(':');
        if (splits.Length == 2)
            if (string.Compare(splits[0], "epsg", StringComparison.InvariantCultureIgnoreCase) == 0)
                if (int.TryParse(splits[1], out var number))
                    return number;

        return null;
    }

    /// <summary>Enumerates all SRID's in the SRID.csv file.</summary>
    /// <returns>Enumerator</returns>
    public static IEnumerable<WKTstring> GetSRIDs()
    {
        using var sr = GetStreamReader();
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            var split = line?.IndexOf(';') ?? -1;
            if (split > -1)
            {
                var wkt = new WKTstring {
                    WKID = int.Parse(line!.Substring(0, split)),
                    WKT = line.Substring(split + 1)
                };

                yield return wkt;
            }
        }
        sr.Close();
    }

    private static string CleanUp(WKTstring wkt)
    {
        var result = wkt.WKT;
        // replace duplicate "" with "
        result = result.Replace("\"\"", "\"");
        if (result.StartsWith("\""))
        {
            // remove leading "
            result = result.Substring(1);
        }

        if (result.EndsWith("\""))
        {
            // remove trailing "
            result = result.Substring(0,result.Length -1);
        }

        return result;
    }

    /// <summary>Gets a coordinate system from the SRID.csv file</summary>
    /// <param name="id">EPSG ID</param>
    /// <returns>Coordinate system, or null if SRID was not found.</returns>
    public static CoordinateSystem? GetCoordinateSystemById(int id)
    {
        if (!Projections.TryGetValue(id, out var result))
            foreach (var wkt in GetSRIDs())
                if (wkt.WKID == id) //We found it!
                {
                    var cleanupWkt = CleanUp(wkt);
                    result = CoordinateSystemFactory.CreateFromWkt(cleanupWkt);
                    Projections[id] = result;
                    break;
                }

        return result;
    }


    public (double X, double Y) Project(string fromCRS, string toCRS, double x, double y)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
        // no transformation needed
            return (x, y);

        var transform = GetTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        return transform.MathTransform.Transform(x, y);
    }

    public void Project(string fromCRS, string toCRS, MPoint point)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
        // no transformation needed
            return;

        var transform = GetTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        Transform(point, transform);
    }

    private static void Transform(MPoint point, ICoordinateTransformation transform)
    {
        var transformed = transform.MathTransform.Transform(point.X, point.Y);
        point.X = transformed.x;
        point.Y = transformed.y;
    }

    private static void Transform(Geometry geometry, GeometryTransform transform)
    {
        geometry.Apply(transform);
    }

    private static GeometryTransform? GetGeometryTransformation(int? fromId, int? toId)
    {
        var transformation = GetTransformation(fromId, toId);
        if (transformation == null) return null;

        var key = (fromId!.Value, toId!.Value);
        if (!GeometryTransformations.TryGetValue(key, out var result))
        {
            result = new GeometryTransform(transformation.MathTransform);
            GeometryTransformations[key] = result;
        }

        return result;
    }

    public void Project(string fromCRS, string toCRS, MRect rect)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
        // no transformation needed
            return;

        var transform = GetTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        Transform(rect.Min, transform);
        Transform(rect.Max, transform);
    }

    public bool IsProjectionSupported(string? fromCRS, string? toCRS)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
        // no transformation needed
            return true;

        if (fromId == null) return false;

        var fromCoordinateSystem = GetCoordinateSystemById(fromId.Value);
        if (fromCoordinateSystem == null) return false;

        if (toId == null) return false;

        var toCoordinateSystem = GetCoordinateSystemById(toId.Value);
        if (toCoordinateSystem == null) return false;

        return true;
    }

    public void Project(string fromCRS, string toCRS, IEnumerable<IFeature> features)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
        // no transformation needed
            return;

        var transform = GetGeometryTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var geometry = geometryFeature.Geometry;
                if (geometry != null) Transform(geometry, transform);
            }
    }

    public void Project(string fromCRS, string toCRS, IFeature feature)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
        // no transformation needed
            return;

        var transform = GetGeometryTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        if (feature is GeometryFeature geometryFeature)
        {
            var geometry = geometryFeature.Geometry;
            if (geometry != null) Transform(geometry, transform);
        }
    }

    public static ICoordinateTransformation? GetTransformation(int? fromId, int? toId)
    {
        if (fromId == null || toId == null) return null;

        var key = (fromId.Value, toId.Value);
        if (!Transformations.TryGetValue(key, out var result))
        {
            var fromCoordinateSystem = GetCoordinateSystemById(fromId.Value);
            var toCoordinateSystem = GetCoordinateSystemById(toId.Value);
            result = CoordinateTransformationFactory.CreateFromCoordinateSystems(fromCoordinateSystem, toCoordinateSystem);
            Transformations[key] = result;
        }

        return result;
    }
}
