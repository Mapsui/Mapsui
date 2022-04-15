using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Providers;

public abstract class AsyncProviderBase<T> : IProviderBase where T : IFeature
{
    public virtual string? CRS { get; set; }

    public abstract MRect? GetExtent();

    public abstract IAsyncEnumerable<T> GetFeaturesAsync(FetchInfo fetchInfo);
}