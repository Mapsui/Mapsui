// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DotSpatial.Projections;
using DotSpatial.Projections.AuthorityCodes;
using Mapsui.Nts;
using Mapsui.Nts.Projections;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace Mapsui.Extensions.Projections;

public class DotSpatialProjection : IProjection, IProjectionCrs
{
    private static readonly ConcurrentDictionary<int, ProjectionInfo> Projections = new();
    private static readonly ConcurrentDictionary<(int From, int To), GeometryTransform> GeometryTransformations = new();
    private static readonly ConcurrentDictionary<string, string> CrsFromEsriLookup = new();

    public static void Init()
    {
        ProjectionDefaults.Projection = new DotSpatialProjection();
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

    public (double X, double Y) Project(string fromCRS, string toCRS, double x, double y)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
            // no transformation needed
            return (x, y);

        var transform = GetTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        return Transform(x, y, transform);
    }

    private static (double X, double Y) Transform(double x, double y,
        [DisallowNull] (ProjectionInfo From, ProjectionInfo To)? transform)
    {
        var pointsXy = new[] { x, y };
        Reproject.ReprojectPoints(pointsXy, Array.Empty<double>(), transform.Value.From, transform.Value.To, 0, 1);
        return (pointsXy[0], pointsXy[1]);
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

        Transform(point, transform.Value);
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

        Transform(rect.Min, transform.Value);
        Transform(rect.Max, transform.Value);
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

    private static void Transform(MPoint point, (ProjectionInfo From, ProjectionInfo To) transform)
    {
        var transformed = Transform(point.X, point.Y, transform);
        point.X = transformed.X;
        point.Y = transformed.Y;
    }

    private static void Transform(Geometry geometry, GeometryTransform transform)
    {
        geometry.Apply(transform);
    }

    /// <summary>Gets a coordinate system from the SRID.csv file</summary>
    /// <param name="id">EPSG ID</param>
    /// <returns>Coordinate system, or null if SRID was not found.</returns>
    private static ProjectionInfo? GetCoordinateSystemById(int id)
    {
        if (!Projections.TryGetValue(id, out var result))
        {
            result = ProjectionInfo.FromEpsgCode(id);
            Projections[id] = result;
        }

        return result;
    }

    private static GeometryTransform? GetGeometryTransformation(int? fromId, int? toId)
    {
        var transformation = GetTransformation(fromId, toId);
        if (transformation == null) return null;

        var key = (fromId!.Value, toId!.Value);
        if (!GeometryTransformations.TryGetValue(key, out var result))
        {
            result = new GeometryTransform(transformation.Value);
            GeometryTransformations[key] = result;
        }

        return result;
    }

    public static (ProjectionInfo From, ProjectionInfo To)? GetTransformation(int? fromId, int? toId)
    {
        if (fromId == null || toId == null) return null;

        var fromCoordinateSystem = GetCoordinateSystemById(fromId.Value);
        var toCoordinateSystem = GetCoordinateSystemById(toId.Value);

        if (fromCoordinateSystem == null || toCoordinateSystem == null)
        {
            return null;
        }

        return (fromCoordinateSystem, toCoordinateSystem);
    }

    public string? CrsFromEsri(string esriString)
    {
        if (!CrsFromEsriLookup.TryGetValue(esriString, out var result))
        {
            var projection = ProjectionInfo.FromEsriString(esriString);
            if (!CrsFromEsriLookup.TryGetValue(projection.ToEsriString(), out result))
            {
                // Initialize Authority Code Handler
                var instance = AuthorityCodeHandler.Instance;
                var field = typeof(AuthorityCodeHandler).GetField("_authorityCodeToProjectionInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                var dictionary = (IDictionary<string, ProjectionInfo>?)field?.GetValue(instance);
                if (dictionary != null)
                    foreach (var it in dictionary)
                    {
                        CrsFromEsriLookup[it.Value.ToEsriString()] = it.Key;
                        if (projection.Equals(it.Value))
                        {
                            CrsFromEsriLookup[esriString] = it.Key;
                            return it.Key;
                        }
                    }
            }
        }

        return result;
    }
}
