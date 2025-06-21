using BruTile;
using BruTile.Cache;
using Mapsui.Layers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Mapsui.Tiling.Fetcher;

public class TileFetchDispatcher(
    ITileCache<IFeature?> tileCache,
    ITileSchema tileSchema,
    Func<TileInfo, Task<IFeature?>> fetchTileAsFeature,
    IDataFetchStrategy? dataFetchStrategy = null) : INotifyPropertyChanged
{
    public static int DefaultNumberOfSimultaneousThreads { get; set; } = 4;
    private bool _busy;
    private readonly IDataFetchStrategy _dataFetchStrategy = dataFetchStrategy ?? new MinimalDataFetchStrategy();
    private readonly MessageBox<FetchInfo> _latestFetchInfo = new();
    private readonly FetchTracker _fetchTracker = new();

    public int NumberTilesNeeded { get; private set; }

    public event EventHandler<Exception?>? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshData(FetchInfo fetchInfo, Action<Func<Task>> enqueueFetch)
    {
        // Set Busy to true immediately, so that the caller can immediately start waiting for it to go back to false.
        // Not sure if this is the best solution. It will often go to true and back to false without doing something.
        Busy = true;
        _latestFetchInfo.Put(fetchInfo);
        enqueueFetch?.Invoke(() => ProcessRefreshDataAsync(enqueueFetch)); // Calculations are done on the FetchMachine.
    }

    private Task ProcessRefreshDataAsync(Action<Func<Task>> enqueueFetch)
    {
        if (_latestFetchInfo.TryTake(out var fetchInfo))
        {
            NumberTilesNeeded = _fetchTracker.Update(fetchInfo, tileSchema, _dataFetchStrategy, tileCache);

            StartFetching(enqueueFetch);
        }
        return Task.CompletedTask; // To make it async because that allows for an easy way to enqueue.
    }

    private void StartFetching(Action<Func<Task>> enqueueFetch)
    {
        Busy = !_fetchTracker.IsDone();
        // We want to keep a limited number of tiles in progress because the extent could change again and we do not
        // want to fetch tiles that are not needed anymore.
        while (_fetchTracker.TryTake(out var tileToFetch, DefaultNumberOfSimultaneousThreads))
            enqueueFetch(() => FetchOnThreadAsync(tileToFetch, enqueueFetch));
    }

    private async Task FetchOnThreadAsync(TileInfo tileInfo, Action<Func<Task>> enqueueFetch)
    {
        try
        {
            var feature = await fetchTileAsFeature(tileInfo).ConfigureAwait(false);
            FetchCompleted(tileInfo, enqueueFetch, feature, null);
        }
        catch (Exception ex)
        {
            // The exception is returned to the caller and should be logged there.
            FetchCompleted(tileInfo, enqueueFetch, null, ex);
        }
    }

    private void FetchCompleted(TileInfo tileInfo, Action<Func<Task>> enqueueFetch, IFeature? feature, Exception? exception)
    {
        if (exception != null)
        {
            _fetchTracker.FetchFailed(tileInfo.Index);
        }
        else
        {
            _fetchTracker.FetchDone(tileInfo.Index);
            tileCache.Add(tileInfo.Index, feature);
        }

        Busy = !_fetchTracker.IsDone();
        DataChanged?.Invoke(this, exception);

        StartFetching(enqueueFetch);
    }

    public bool Busy
    {
        get => _busy;
        private set
        {
            if (_busy == value)
                return;
            _busy = value;
            OnPropertyChanged(nameof(Busy));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
