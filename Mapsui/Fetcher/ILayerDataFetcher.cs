using Mapsui.Layers;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public interface ILayerDataFetcher
{
    public Task FetchAsync(FetchInfo fetchInfo, CancellationToken cancelationToken);
}
