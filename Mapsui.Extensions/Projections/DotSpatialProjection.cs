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
    private static readonly ConcurrentDictionary<int, ProjectionInfo> _projections = new();
    private static readonly ConcurrentDictionary<(int From, int To), GeometryTransform> _geometryTransformations = new();
    private static readonly ConcurrentDictionary<string, string> _crsFromEsriLookup = new();
    private static bool _initialized;

    public static void Init()
    {
        if (!_initialized)
        {
            _initialized = true;
            ProjectionDefaults.Projection = new DotSpatialProjection();
            InitProjections();
        }
    }

    private static int GetIdFromCrs(string crs)
    {
        var splits = crs.Split(':');
        if (splits.Length == 2)
            if (string.Compare(splits[0], "epsg", StringComparison.InvariantCultureIgnoreCase) == 0)
                if (int.TryParse(splits[1], out var number))
                    return number;

        throw new ArgumentException("Could not parse CRS");
    }

    public (double X, double Y) Project(string fromCRS, string toCRS, double x, double y)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
            return (x, y);

        var transform = GetTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        return Transform(x, y, transform);
    }

    private static (double X, double Y) Transform(double x, double y,
        [DisallowNull] (ProjectionInfo From, ProjectionInfo To)? transform)
    {
        var pointsXy = new[] { x, y };
        Reproject.ReprojectPoints(pointsXy, [], transform.Value.From, transform.Value.To, 0, 1);
        return (pointsXy[0], pointsXy[1]);
    }

    public void Project(string fromCRS, string toCRS, MPoint point)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
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
            return;

        var transform = GetTransformation(fromId, toId);
        if (transform == null) throw new ArgumentException();

        Transform(rect.Min, transform.Value);
        Transform(rect.Max, transform.Value);
    }

    public bool IsProjectionSupported(string? fromCRS, string? toCRS)
    {
        if (fromCRS == null || toCRS == null)
            return false;
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
            return true; // This is supported because we do not need to project

        var fromCoordinateSystem = GetCoordinateSystemById(fromId);
        if (fromCoordinateSystem == null) return false;
        var toCoordinateSystem = GetCoordinateSystemById(toId);
        if (toCoordinateSystem == null) return false;

        return true;
    }

    public void Project(string fromCRS, string toCRS, IEnumerable<IFeature> features)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
            return; // No transformation needed

        var geometryTransform = GetGeometryTransformation(fromId, toId) ?? throw new ArgumentException();
        var transform = GetTransformation(fromId, toId) ?? throw new ArgumentException();
        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var geometry = geometryFeature.Geometry;
                if (geometry != null) Transform(geometry, geometryTransform);
            }
            else
            {
                feature.CoordinateVisitor((x, y, setter) =>
                {
                    var (xOut, yOut) = Transform(x, y, transform);
                    setter(xOut, yOut);
                });
            }
    }

    public void Project(string fromCRS, string toCRS, IFeature feature)
    {
        var fromId = GetIdFromCrs(fromCRS);
        var toId = GetIdFromCrs(toCRS);
        if (fromId == toId)
            return;

        if (feature is GeometryFeature geometryFeature)
        {
            var geometryTransform = GetGeometryTransformation(fromId, toId) ?? throw new ArgumentException();
            var geometry = geometryFeature.Geometry;
            if (geometry != null) Transform(geometry, geometryTransform);
        }
        else
        {
            var transform = GetTransformation(fromId, toId) ?? throw new ArgumentException();
            feature.CoordinateVisitor((x, y, setter) =>
            {
                var (xOut, yOut) = Transform(x, y, transform);
                setter(xOut, yOut);
            });
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
        if (!_projections.TryGetValue(id, out var result))
        {
            result = ProjectionInfo.FromEpsgCode(id);
            _projections[id] = result;
        }

        return result;
    }

    private static GeometryTransform? GetGeometryTransformation(int? fromId, int? toId)
    {
        if (fromId is null) return null;
        if (toId is null) return null;

        var transformation = GetTransformation(fromId.Value, toId.Value);
        if (transformation == null) return null;

        var key = (fromId!.Value, toId!.Value);
        if (!_geometryTransformations.TryGetValue(key, out var result))
        {
            result = new GeometryTransform(transformation.Value);
            _geometryTransformations[key] = result;
        }

        return result;
    }

    public static (ProjectionInfo From, ProjectionInfo To)? GetTransformation(int fromId, int toId)
    {
        var fromCoordinateSystem = GetCoordinateSystemById(fromId);
        var toCoordinateSystem = GetCoordinateSystemById(toId);

        if (fromCoordinateSystem == null || toCoordinateSystem == null)
        {
            return null;
        }

        return (fromCoordinateSystem, toCoordinateSystem);
    }

    public string? CrsFromEsri(string esriString)
    {
        if (!_crsFromEsriLookup.TryGetValue(esriString, out var result))
        {
            var projection = ProjectionInfo.FromEsriString(esriString);
            if (!_crsFromEsriLookup.TryGetValue(projection.ToEsriString(), out result))
            {
                // Initialize Authority Code Handler
                var instance = AuthorityCodeHandler.Instance;
                var field = typeof(AuthorityCodeHandler).GetField("_authorityCodeToProjectionInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                var dictionary = (IDictionary<string, ProjectionInfo>?)field?.GetValue(instance);
                if (dictionary != null)
                    foreach (var it in dictionary)
                    {
                        _crsFromEsriLookup[it.Value.ToEsriString()] = it.Key;
                        if (projection.Equals(it.Value))
                        {
                            _crsFromEsriLookup[esriString] = it.Key;
                            return it.Key;
                        }
                    }
            }
        }

        return result;
    }

    public void Register(string crs, string esriString)
    {
        var id = GetIdFromCrs(crs);
        InitProjections();

        var projection = ProjectionInfo.FromEsriString(esriString);
        _projections[id] = projection;

        _crsFromEsriLookup[esriString] = crs;
    }

    private static void InitProjections()
    {
        if (!_projections.IsEmpty)
            return;

        // Initialize Authority Code Handler
        var instance = AuthorityCodeHandler.Instance;
        var field = typeof(AuthorityCodeHandler).GetField("_authorityCodeToProjectionInfo", BindingFlags.Instance | BindingFlags.NonPublic);
        var dictionary = (IDictionary<string, ProjectionInfo>?)field?.GetValue(instance);
        if (dictionary != null)
            foreach (var it in dictionary)
            {
                _crsFromEsriLookup[it.Value.ToEsriString()] = it.Key;
                var id = GetIdFromCrs(it.Key);
                _projections[id] = it.Value;
            }
    }
}
