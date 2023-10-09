using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Providers;

public class FilteringProvider : IProvider
{
    private readonly IProvider _provider;
    private readonly Func<IFeature, bool> _filter;

    public FilteringProvider(IProvider provider, Func<IFeature, bool> filter)
    {
        _provider = provider;
        _filter = filter;
    }

    public string? CRS
    {
        get => _provider.CRS;
        set => _provider.CRS = value;
    }

    public MRect? GetExtent()
    {
        return _provider.GetExtent();
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = await _provider.GetFeaturesAsync(fetchInfo);
        return features.Where(f => _filter(f));
    }
}
