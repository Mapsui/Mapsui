using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;

namespace Mapsui.Nts.Providers;

public class GeometryIntersectionProvider : BaseProvider
{
    private readonly IProvider _provider;

    public GeometryIntersectionProvider(IProvider provider)
    {
        _provider = provider;
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
        var rectangle = fetchInfo.Extent.Grow(fetchInfo.Resolution).ToPolygon();

        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                var copied = new GeometryFeature(geometryFeature, (feature.Id, Id, fetchInfo.Resolution));
                if (geometryFeature.Geometry != null)
                {
                    copied.Geometry = rectangle.Intersection(geometryFeature.Geometry);
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
