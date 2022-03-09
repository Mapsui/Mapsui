using Mapsui.Providers;

namespace Mapsui.Layers;

public interface ILayerDataSource
{
    public IProvider<IFeature>? DataSource { get; }
}
