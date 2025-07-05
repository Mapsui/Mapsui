using Mapsui.Layers;
using Mapsui.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public sealed class LayerFetcher
{
    private readonly int _maxConcurrent = 8;
    private int _fetchRequestCount = 0;
    private readonly ConcurrentDictionary<long, FetchRequest> _activeFetches = new();
    private readonly LatestMailbox<FetchInfo> _latestFetchInfo = new();
    private readonly IEnumerable<ILayer> _layers;

    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { AllowSynchronousContinuations = false, SingleReader = false });

    public LayerFetcher(IEnumerable<ILayer> Layers)
    {
        _layers = Layers;
        _ = Task.Run(() => AddConsumerAsync(_channel));
    }

    public void ViewportChanged(FetchInfo fetchInfo)
    {
        _latestFetchInfo.Overwrite(fetchInfo);
        UpdateLayerViewports(fetchInfo);
        _channel.Writer.TryWrite(true);
    }

    private async Task AddConsumerAsync(Channel<bool> channel)
    {
        await foreach (var _ in channel.Reader.ReadAllAsync())
        {
            UpdateFetches();
        }
    }

    private void UpdateLayerViewports(FetchInfo fetchInfo)
    {
        foreach (var layer in _layers.OfType<ILayerDataFetcher>())
        {
            layer.ViewportChanged(fetchInfo);
        }
    }

    private void UpdateFetches()
    {
        foreach (var layer in _layers.OfType<ILayerDataFetcher>())
        {
            try
            {
                var activeFetchCountForLayer = _activeFetches.Count(kvp => kvp.Value.LayerId == layer.Id);

                var availableFetchSlots = _maxConcurrent - _fetchRequestCount;
                if (availableFetchSlots == 0)
                    return;
                var fetchRequests = layer.GetFetchRequests(activeFetchCountForLayer, availableFetchSlots);

                foreach (var fetchRequest in fetchRequests)
                {
                    Interlocked.Increment(ref _fetchRequestCount);

                    _activeFetches[fetchRequest.RequestId] = fetchRequest;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await fetchRequest.FetchFunc();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, $"Error fetching data for layer {layer.Id}: {ex.Message}", ex);
                        }
                        finally
                        {
                            _activeFetches.Remove(fetchRequest.RequestId, out var value);
                            Interlocked.Decrement(ref _fetchRequestCount);
                            _channel.Writer.TryWrite(true);
                        }
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Error updating fetches for layer {layer.Id}: {ex.Message}", ex);
            }
        }
    }
}
