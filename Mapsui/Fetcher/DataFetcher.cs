using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public sealed class DataFetcher
{
    private readonly int _maxConcurrentFetches = 8;
    private readonly ConcurrentDictionary<long, FetchJob> _activeFetches = new();
    private readonly LatestMailbox<FetchInfo> _latestFetchInfo = new();
    private readonly Func<IEnumerable<IFetchableSource>> _getFetchableSources;

    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { AllowSynchronousContinuations = false, SingleReader = false });

    public DataFetcher(Func<IEnumerable<IFetchableSource>> getFetchableSources) // The constructor accepts a function so that it works for changes to the layer list.
    {
        _getFetchableSources = getFetchableSources;
        _ = Task.Run(() => AddConsumerAsync(_channel).ConfigureAwait(false));
    }

    public void ViewportChanged(FetchInfo fetchInfo)
    {
        _latestFetchInfo.Overwrite(fetchInfo);
        UpdateViewports(fetchInfo);
        _ = _channel.Writer.TryWrite(true);
    }

    /// <summary>
    /// Awaitable version of <see cref="ViewportChanged"/>. Returns when all fetches triggered
    /// by this viewport change (including cascading fetches) have completed.
    /// </summary>
    public async Task ViewportChangedAsync(FetchInfo fetchInfo)
    {
        _latestFetchInfo.Overwrite(fetchInfo);
        UpdateViewports(fetchInfo);

        // Drive the fetch loop ourselves instead of writing to _channel,
        // so we can await completion of all cascading fetches.
        while (true)
        {
            var tasks = new List<Task>();

            foreach (var fetchableSource in _getFetchableSources())
            {
                try
                {
                    var activeFetchCountForSource = _activeFetches.Count(kvp => kvp.Value.FetchableSourceId == fetchableSource.Id);
                    var availableFetchSlots = _maxConcurrentFetches - _activeFetches.Count;
                    if (availableFetchSlots <= 0)
                        break;

                    var fetchJobs = fetchableSource.GetFetchJobs(activeFetchCountForSource, availableFetchSlots);

                    foreach (var fetchJob in fetchJobs)
                    {
                        _activeFetches[fetchJob.JobId] = fetchJob;
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await fetchJob.FetchFunc().ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Error, $"Error fetching data for fetchable source {fetchableSource.Id}: {ex.Message}", ex);
                            }
                            finally
                            {
                                _ = _activeFetches.Remove(fetchJob.JobId, out _);
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Error updating fetches for fetchable source {fetchableSource.Id}: {ex.Message}", ex);
                }
            }

            if (tasks.Count == 0)
                break;

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    private async Task AddConsumerAsync(Channel<bool> channel)
    {
        await foreach (var _ in channel.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            UpdateFetches();
        }
    }

    private void UpdateViewports(FetchInfo fetchInfo)
    {
        if (fetchInfo.Section.CheckIfAreaIsTooBig())
        {
            Logger.Log(LogLevel.Error, $"The area of the section is too big in the DataFetcher.UpdateViewports method with parameters: Extent: {fetchInfo.Extent}, Resolution: {fetchInfo.Resolution}");
            return; // Check added for this issue: https://github.com/Mapsui/Mapsui/issues/3105
        }

        foreach (var fetchableSource in _getFetchableSources())
        {
            fetchableSource.ViewportChanged(fetchInfo);
        }
    }

    private void UpdateFetches()
    {
        foreach (var fetchableSource in _getFetchableSources())
        {
            try
            {
                var activeFetchCountForFetchableSource = _activeFetches.Count(kvp => kvp.Value.FetchableSourceId == fetchableSource.Id);

                var availableFetchSlots = _maxConcurrentFetches - _activeFetches.Count;
                if (availableFetchSlots == 0)
                    return;
                var fetchJobs = fetchableSource.GetFetchJobs(activeFetchCountForFetchableSource, availableFetchSlots);

                foreach (var fetchJob in fetchJobs)
                {
                    _activeFetches[fetchJob.JobId] = fetchJob;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await fetchJob.FetchFunc().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, $"Error fetching data for fetchable source {fetchableSource.Id}: {ex.Message}", ex);
                        }
                        finally
                        {
                            _ = _activeFetches.Remove(fetchJob.JobId, out var value);
                            _ = _channel.Writer.TryWrite(true);
                        }
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Error updating fetches for fetchable source {fetchableSource.Id}: {ex.Message}", ex);
            }
        }
    }
}
