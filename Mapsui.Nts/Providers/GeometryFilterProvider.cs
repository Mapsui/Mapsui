using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Nts.Providers;

public class GeometryFilterProvider(IProvider provider, Func<IFeature, bool> filter) : IProvider, IProviderExtended
{
    private DateTime _lastUpdate;
    private IEnumerable<IFeature> _current = [];
    public int Id { get; } = BaseLayer.NextId();

    public string? CRS
    {
        get => provider.CRS;
        set => provider.CRS = value;
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        if (DateTime.Now - _lastUpdate < TimeSpan.FromSeconds(20))
        {
            return _current;
        }

        _lastUpdate = DateTime.Now;
        _current = IterateFeatures(await provider.GetFeaturesAsync(fetchInfo));
        return _current;
    }

    private IEnumerable<IFeature> IterateFeatures(IEnumerable<IFeature> features)
    {
        foreach (var feature in features)
            if (filter(feature))
                yield return feature;
    }

    public MRect? GetExtent()
    {
        return provider.GetExtent();
    }
}
