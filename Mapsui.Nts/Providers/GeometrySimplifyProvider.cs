using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace Mapsui.Nts.Providers;

public class GeometrySimplifyProvider : AsyncProviderBase<IFeature>, IProvider<IFeature>
{
    private readonly IProviderBase _provider;
    private readonly Func<Geometry, double, Geometry> _simplify;
    private readonly double? _distanceTolerance;

    public GeometrySimplifyProvider(IProviderBase provider, Func<Geometry, double, Geometry>? simplify = null, double? distanceTolerance = null)
    {
        _provider = provider;
        _simplify = simplify ?? TopologyPreservingSimplifier.Simplify;
        _distanceTolerance = distanceTolerance;
    }

    public override string? CRS
    {
        get => _provider.CRS;
        set => _provider.CRS = value;
    }

    public override async IAsyncEnumerable<IFeature> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = await _provider.GetFeaturesAsync<IFeature>(fetchInfo);
        foreach (var p in IterateFeatures(fetchInfo, features))
        {
            yield return p;
        }
    }

    public IEnumerable<IFeature> GetFeatures(FetchInfo fetchInfo)
    {
        var features = _provider.GetFeatures<IFeature>(fetchInfo);
        foreach (var p in IterateFeatures(fetchInfo, features))
        {
            yield return p;
        }
    }

    private IEnumerable<IFeature> IterateFeatures(FetchInfo fetchInfo, IEnumerable<IFeature> features)
    {
        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var copied = new GeometryFeature(geometryFeature);
                if (geometryFeature.Geometry != null)
                {
                    copied.Geometry = _simplify(geometryFeature.Geometry, _distanceTolerance ?? fetchInfo.Resolution);
                }

                yield return copied;
            }
            else
                yield return feature;
    }

    public override MRect? GetExtent()
    {
        return _provider.GetExtent();
    }
}