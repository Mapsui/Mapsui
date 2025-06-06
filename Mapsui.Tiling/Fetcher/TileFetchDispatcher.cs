using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Tiling.Extensions;
using Mapsui.Utilities;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Tiling.Fetcher;

public class TileFetchDispatcher
{
    private bool _busy;
    private readonly IDataFetchStrategy _dataFetchStrategy;
    private ConcurrentQueue<TileInfo> _tilesToFetch = [];
    private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = [];
    private readonly ConcurrentHashSet<TileIndex> _tilesThatFailed = [];
    private readonly FetchMachine _fetchMachine = new(4);
    private readonly ITileCache<IFeature?> _tileCache;
    private readonly ITileSchema _tileSchema;
    private readonly Func<TileInfo, Task<IFeature?>> _fetchTileAsFeature;
    private readonly Channel<FetchInfo> _refreshQueue = Channel.CreateBounded<FetchInfo>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest, AllowSynchronousContinuations = false, SingleReader = true });

    public int NumberTilesNeeded { get; private set; }
    public static int MaxTilesInOneRequest { get; set; } = 128;

    public event EventHandler<Exception?>? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public TileFetchDispatcher(
        ITileCache<IFeature?> tileCache,
        ITileSchema tileSchema,
        Func<TileInfo, Task<IFeature?>> fetchTileAsFeature,
        IDataFetchStrategy? dataFetchStrategy = null)
    {
        _tileCache = tileCache;
        _tileSchema = tileSchema;
        _fetchTileAsFeature = fetchTileAsFeature;
        _dataFetchStrategy = dataFetchStrategy ?? new MinimalDataFetchStrategy();
        _ = Task.Run(ProcessRefreshDataAsync);
    }

    public void RefreshData(FetchInfo fetchInfo)
    {
        // Set Busy to true immediately, so that the caller can immediately start waiting for it to go back to false.
        // Not sure if this is the best solution. It will often go to true and back to false without doing something.
        Busy = true;
        _ = _refreshQueue.Writer.WriteAsync(fetchInfo);
    }

    private async Task ProcessRefreshDataAsync()
    {
        await foreach (var fetchInfo in _refreshQueue.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            _tilesThatFailed.Clear(); // Try them again on new refresh data event.
            _tilesToFetch = GetTilesToFetch(fetchInfo, _tileSchema);
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
            if (!_tilesToFetch.TryDequeue(out var tileToFetch))
                return;
            if (!_tilesInProgress.Add(tileToFetch.Index))
                Logger.Log(LogLevel.Warning, "Could not add the tile index to the tiles in progress list. This was not expected");
            _fetchMachine.Enqueue(() => FetchOnThreadAsync(tileToFetch));
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
            var feature = await _fetchTileAsFeature(tileInfo).ConfigureAwait(false);
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
            _tileCache.Add(tileInfo.Index, feature);

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

    private ConcurrentQueue<TileInfo> GetTilesToFetch(FetchInfo fetchInfo, ITileSchema tileSchema)
    {
        if (fetchInfo is null || tileSchema is null)
            return [];

        var levelId = BruTile.Utilities.GetNearestLevel(tileSchema.Resolutions, fetchInfo.Resolution);
        var tilesToCoverViewport = _dataFetchStrategy.Get(tileSchema, fetchInfo.Extent.ToExtent(), levelId);
        NumberTilesNeeded = tilesToCoverViewport.Count;

        var tilesToFetch = tilesToCoverViewport.Where(t =>
            _tileCache.Find(t.Index) == null
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

        var queue = new ConcurrentQueue<TileInfo>();

        foreach (var tile in tilesToFetch)
            queue.Add(tile);

        return queue;
    }
}
