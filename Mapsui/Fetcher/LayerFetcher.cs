using Mapsui.Layers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public class LayerFetcher(IEnumerable<ILayer> Layers)
{
    private readonly int _maxConcurrent = 16;
    private readonly Dictionary<long, FetchRequest> _activeFetches = new();
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
        foreach (var layer in Layers.OfType<ILayerDataFetcher>())
        {
            if (_activeFetches.Count >= _maxConcurrent)
                return;

            var activeFetches = _activeFetches.Count(kvp => kvp.Value.LayerId == layer.Id);

            var fetchRequests = layer.GetFetchRequests(activeFetches);

            foreach (var fetchRequest in fetchRequests)
            {
                _activeFetches[fetchRequest.RequestId] = fetchRequest;
                _ = Task.Run(async () =>
                {
                    await fetchRequest.FetchFunc();
                    _ = _activeFetches.Remove(fetchRequest.RequestId);
                    UpdateFetches(); // After one fetch completes we check if we can start more.
                }).ConfigureAwait(false);
            }

        }
    }
}
