using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Providers;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace Mapsui.Nts.Providers;

public class GeometrySimplifyProvider : IProvider, IProviderExtended
{
    private readonly IProvider _provider;
    private readonly Func<Geometry, double, Geometry> _simplify;
    private readonly double? _distanceTolerance;
    private FeatureKeyCreator<(long, double)>? _featureKeyCreator;

    public GeometrySimplifyProvider(IProvider provider, Func<Geometry, double, Geometry>? simplify = null, double? distanceTolerance = null)
    {
        _provider = provider;
        _simplify = simplify ?? TopologyPreservingSimplifier.Simplify;
        _distanceTolerance = distanceTolerance;
    }

    public int Id { get; } = BaseLayer.NextId();

    public string? CRS
    {
        get => _provider.CRS;
        set => _provider.CRS = value;
    }

    public FeatureKeyCreator<(long, double)> FeatureKeyCreator
    {
        get => _featureKeyCreator ??= new FeatureKeyCreator<(long, double)>();
        set => _featureKeyCreator = value;
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        return IterateFeatures(fetchInfo, await _provider.GetFeaturesAsync(fetchInfo));
    }

    private IEnumerable<IFeature> IterateFeatures(FetchInfo fetchInfo, IEnumerable<IFeature> features)
    {
        var resolution = _distanceTolerance ?? fetchInfo.Resolution;
        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var copied = new GeometryFeature(geometryFeature, FeatureId.CreateId(Id, (feature.Id, resolution), FeatureKeyCreator.GetKey));
                if (geometryFeature.Geometry != null)
                {
                    copied.Geometry = _simplify(geometryFeature.Geometry, resolution);
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
