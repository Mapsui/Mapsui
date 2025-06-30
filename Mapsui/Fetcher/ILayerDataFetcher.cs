using Mapsui.Layers;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public interface ILayerDataFetcher
{
    public int Id { get; }
    public bool NeedsFetch { get; }
    public Task FetchAsync();
    public void ViewportChanged(FetchInfo fetchInfo);
}
