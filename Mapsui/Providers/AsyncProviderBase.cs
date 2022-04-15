using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Providers;

public abstract class AsyncProviderBase<T> : IAsyncProvider<T> where T : IFeature
{
    public virtual string? CRS { get; set; }
    
    public IEnumerable<T> GetFeatures(FetchInfo fetchInfo)
    {
        var enumerator = GetFeaturesAsync(fetchInfo).GetAsyncEnumerator();
#pragma warning disable VSTHRD002
        if (enumerator.MoveNextAsync().Result)
#pragma warning restore VSTHRD002
        {
            yield return enumerator.Current;
        }

        enumerator.DisposeAsync().GetAwaiter().GetResult();
    }

    public abstract MRect? GetExtent();

    public abstract IAsyncEnumerable<T> GetFeaturesAsync(FetchInfo fetchInfo);
}