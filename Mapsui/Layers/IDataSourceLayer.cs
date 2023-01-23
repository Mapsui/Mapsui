using Mapsui.Providers;

namespace Mapsui.Layers;

public interface ILayerDataSource<out T>
    where T : IProvider
{
    public T? DataSource { get; }
}
