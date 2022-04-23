using Mapsui.Providers;

namespace Mapsui.Layers;

public interface ILayerDataSource<out T> 
    where T : IProviderBase
{
    public T? DataSource { get; }
}
