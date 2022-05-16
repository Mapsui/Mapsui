using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
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

    public async IAsyncEnumerable<IFeature> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = IterateFeaturesAsync(fetchInfo, _provider.GetFeaturesAsync(fetchInfo));
        await foreach (var feature in features)
            yield return feature;
    }

    private async IAsyncEnumerable<IFeature> IterateFeaturesAsync(FetchInfo fetchInfo, IAsyncEnumerable<IFeature> features)
    {
        await foreach (var feature in features)
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

    public MRect? GetExtent()
    {
        return _provider.GetExtent();
    }
}