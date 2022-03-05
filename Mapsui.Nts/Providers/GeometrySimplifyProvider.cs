using System;
using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.Providers;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace Mapsui.Nts.Providers;

public class GeometrySimplifyProvider : IProvider<IFeature>
{
    private readonly IProvider<IFeature> _provider;
    private readonly Func<Geometry, double, Geometry> _simplify;
    private readonly double? _distanceTolerance;

    public GeometrySimplifyProvider(IProvider<IFeature> provider, Func<Geometry, double, Geometry>? simplify = null, double? distanceTolerance = null)
    {
        _provider = provider;
        _simplify = simplify ?? TopologyPreservingSimplifier.Simplify;
        _distanceTolerance = distanceTolerance;
    }

    public string? CRS
    {
        get => _provider.CRS;
        set => _provider.CRS = value;
    }

    public IEnumerable<IFeature> GetFeatures(FetchInfo fetchInfo)
    {
        var features = _provider.GetFeatures(fetchInfo);
        var result = new List<IFeature>();
        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var copied = new GeometryFeature(geometryFeature);
                copied.Geometry = _simplify(geometryFeature.Geometry, _distanceTolerance ?? fetchInfo.Resolution);
                result.Add(copied);
            }
            else
                result.Add(feature);

        return result;
    }

    public MRect? GetExtent()
    {
        return _provider.GetExtent();
    }
}