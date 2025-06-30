using Mapsui.Layers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public class LayerFetcher(IEnumerable<ILayer> Layers)
{
    private readonly int _maxConcurrent = 16;
    private readonly Dictionary<int, Task> _activeRequests = new();
    private readonly LatestMailbox<FetchInfo> _latestFetchInfo = new();

    public void ViewportChanged(FetchInfo fetchInfo)
    {
        _latestFetchInfo.Overwrite(fetchInfo);
        UpdateLayerViewports(fetchInfo);
        UpdateFetches();
    }

    private void UpdateLayerViewports(FetchInfo fetchInfo)
    {
        foreach (var layer in Layers.OfType<ILayerDataFetcher>())
        {
            layer.ViewportChanged(fetchInfo);
        }
    }

    public void UpdateFetches()
    {
        foreach (var layer in Layers.OfType<ILayerDataFetcher>().Where(l => l.NeedsFetch))
        {
            if (_activeRequests.Count >= _maxConcurrent)
                return;
            if (_activeRequests.ContainsKey(layer.Id))
                continue;

            _activeRequests[layer.Id] = Task.Run(async () =>
            {
                await layer.FetchAsync();
                _activeRequests.Remove(layer.Id);
                UpdateFetches(); // After one fetch completes we check if we can start more.
            });
        }
    }
}
