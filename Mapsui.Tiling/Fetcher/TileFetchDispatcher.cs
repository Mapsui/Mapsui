using BruTile;
using BruTile.Cache;
using Mapsui.Fetcher;
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
    private bool _busy;
    private readonly IDataFetchStrategy _dataFetchStrategy = dataFetchStrategy ?? new MinimalDataFetchStrategy();
    private readonly FetchMachine _fetchMachine = new(4);
    private readonly MessageBox<FetchInfo> _latestFetchInfo = new();
    private readonly FetchTracker _fetchTracker = new();

    public int NumberTilesNeeded { get; private set; }

    public event EventHandler<Exception?>? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshData(FetchInfo fetchInfo)
    {
        // Set Busy to true immediately, so that the caller can immediately start waiting for it to go back to false.
        // Not sure if this is the best solution. It will often go to true and back to false without doing something.
        Busy = true;
        _latestFetchInfo.Put(fetchInfo);
        _fetchMachine.Enqueue(ProcessRefreshDataAsync); // Calculations are done on the FetchMachine.
    }

    private Task ProcessRefreshDataAsync()
    {
        if (_latestFetchInfo.TryTake(out var fetchInfo))
        {
            NumberTilesNeeded = _fetchTracker.Update(fetchInfo, tileSchema, _dataFetchStrategy, tileCache);

            StartFetching();
        }
        return Task.CompletedTask; // To make it async because that allows for an easy way to enqueue.
    }

    public void StartFetching()
    {
        Busy = !_fetchTracker.IsDone();
        // We want to keep a limited number of tiles in progress because the extent could change again and we do not
        // want to fetch tiles that are not needed anymore.
        while (_fetchTracker.TryTake(out var tileToFetch, _fetchMachine.NumberOfWorkers))
            _fetchMachine.Enqueue(() => FetchOnThreadAsync(tileToFetch));
    }

    private async Task FetchOnThreadAsync(TileInfo tileInfo)
    {
        try
        {
            var feature = await fetchTileAsFeature(tileInfo).ConfigureAwait(false);
            FetchCompleted(tileInfo, feature, null);
        }
        catch (Exception ex)
        {
            // The exception is returned to the caller and should be logged there.
            FetchCompleted(tileInfo, null, ex);
        }
    }

    private void FetchCompleted(TileInfo tileInfo, IFeature? feature, Exception? exception)
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

        StartFetching();
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

    public void StopFetching()
    {
        _fetchMachine.Stop();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
