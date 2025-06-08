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
    private readonly object _lockRoot = new();
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
        lock (_lockRoot)
        {
            NumberTilesNeeded = _fetchTracker.Update(fetchInfo, tileSchema, _dataFetchStrategy, tileCache);

            StartFetching();
        }
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
