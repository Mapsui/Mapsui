using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using NetTopologySuite.Utilities;

namespace Mapsui.Nts.Providers;

public class GeometryIntersectionProvider : IProvider, IProviderExtended
{
    private readonly IProvider _provider;

    public GeometryIntersectionProvider(IProvider provider)
    {
        _provider = provider;
    }

    public int Id { get; } = BaseLayer.NextId();

    public string? CRS
    {
        get => _provider.CRS;
        set => _provider.CRS = value;
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        return IterateFeatures(fetchInfo, await _provider.GetFeaturesAsync(fetchInfo));
    }

    private IEnumerable<IFeature> IterateFeatures(FetchInfo fetchInfo, IEnumerable<IFeature> features)
    {
        var rectangle = fetchInfo.Extent.Grow(fetchInfo.Resolution).ToPolygon();

        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var copied = new GeometryFeature(geometryFeature, (Id, geometryFeature.Id, rectangle));
                if (geometryFeature.Geometry != null)
                {
                    copied.Geometry = rectangle.Intersection(geometryFeature.Geometry);
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
