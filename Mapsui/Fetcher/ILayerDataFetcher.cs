using Mapsui.Layers;

namespace Mapsui.Fetcher;

public interface ILayerDataFetcher
{
    public int Id { get; }
    public FetchRequest[] GetFetchRequests(int activeFetches);
    public void ViewportChanged(FetchInfo fetchInfo);
}
