using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Tiling.Extensions;
using Mapsui.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
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
    private ConcurrentHashSet<TileInfo> _tilesToFetch = [];
    private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = [];
    private readonly ConcurrentHashSet<TileIndex> _tilesThatFailed = [];
    private readonly FetchMachine _fetchMachine = new(4);

    public int NumberTilesNeeded { get; private set; }

    public static int MaxTilesInOneRequest { get; set; } = 128;

    public event EventHandler<Exception?>? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshData(FetchInfo fetchInfo)
    {
        lock (_lockRoot)
        {
            _tilesThatFailed.Clear(); // Try them again on new refresh data event.
            // GetTilesToFetch can be fairly expensive. It is assumed that the call to RefreshData is throttled in case of dragging and animations.
            _tilesToFetch = GetTilesToFetch(fetchInfo, tileSchema);
            StartFetching();
        }
    }

    public void StartFetching()
    {
        Busy = GetBusy();
        // We want to keep a limited number of tiles in progress because the extent could change again and we do not
        // want to fetch tiles that are not needed anymore.
        while (_tilesInProgress.Count < _fetchMachine.NumberOfWorkers)
        {
            lock (_lockRoot)
            {
                var tileToFetch = _tilesToFetch.FirstOrDefault();
                if (tileToFetch is null)
                    break;
                if (!_tilesToFetch.TryRemove(tileToFetch))
                    return; // Return if it was already removed. Should not happen with a lock.
                if (!_tilesInProgress.Add(tileToFetch.Index))
                    Logger.Log(LogLevel.Warning, "Could not add the tile index to the tiles in progress list. This was not expected");
                _fetchMachine.Enqueue(() => FetchOnThreadAsync(tileToFetch));
            }
        }
    }

    private bool GetBusy()
    {
        return _tilesToFetch.Count > 0 || _tilesInProgress.Count > 0;
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
            if (!_tilesThatFailed.Add(tileInfo.Index))
                Logger.Log(LogLevel.Warning, "Could not add the tile index to the failed tiles list. This was not expected");
        }
        else
            tileCache.Add(tileInfo.Index, feature);

        if (!_tilesInProgress.TryRemove(tileInfo.Index))
            Logger.Log(LogLevel.Warning, "Could not remove the tile index to the in-progress tiles list. This was not expected");

        Busy = GetBusy();
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

    private ConcurrentHashSet<TileInfo> GetTilesToFetch(FetchInfo fetchInfo, ITileSchema tileSchema)
    {
        if (fetchInfo is null || tileSchema is null)
            return [];

        var levelId = BruTile.Utilities.GetNearestLevel(tileSchema.Resolutions, fetchInfo.Resolution);
        var tilesToCoverViewport = _dataFetchStrategy.Get(tileSchema, fetchInfo.Extent.ToExtent(), levelId);
        NumberTilesNeeded = tilesToCoverViewport.Count;

        var tilesToFetch = tilesToCoverViewport.Where(t =>
            tileCache.Find(t.Index) == null
            && !_tilesInProgress.Contains(t.Index)
            && !_tilesThatFailed.Contains(t.Index));

        if (tilesToFetch.Count() > MaxTilesInOneRequest)
        {
            Logger.Log(LogLevel.Warning,
                $"The number tiles requested is '{tilesToFetch.Count()}' which exceeds the maximum " +
                $"of '{MaxTilesInOneRequest}'. The number of tiles will be limited to the maximum. Note, " +
                $"that this may indicate a bug or configuration error");

            tilesToFetch = tilesToFetch.Take(MaxTilesInOneRequest).ToList();
        }

        var hashSet = new ConcurrentHashSet<TileInfo>();

        foreach (var tile in tilesToFetch)
            _ = hashSet.Add(tile);

        return hashSet;
    }
}
