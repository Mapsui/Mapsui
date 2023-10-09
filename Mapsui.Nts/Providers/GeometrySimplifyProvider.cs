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

public class GeometrySimplifyProvider : BaseProvider
{
    private readonly IProvider _provider;
    private readonly Func<Geometry, double, Geometry> _simplify;
    private readonly double? _distanceTolerance;

    public GeometrySimplifyProvider(IProvider provider, Func<Geometry, double, Geometry>? simplify = null, double? distanceTolerance = null)
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

    public override async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        return IterateFeatures(fetchInfo, await _provider.GetFeaturesAsync(fetchInfo));
    }

    private IEnumerable<IFeature> IterateFeatures(FetchInfo fetchInfo, IEnumerable<IFeature> features)
    {
        var tolerance = _distanceTolerance ?? fetchInfo.Resolution;
        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var copied = new GeometryFeature(geometryFeature, (geometryFeature.Id, Id, tolerance));
                if (geometryFeature.Geometry != null)
                {
                    copied.Geometry = _simplify(geometryFeature.Geometry, tolerance);
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
