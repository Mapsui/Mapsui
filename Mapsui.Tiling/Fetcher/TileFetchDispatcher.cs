using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Tiling.Extensions;
using Mapsui.Utilities;

namespace Mapsui.Tiling.Fetcher;

public class TileFetchDispatcher : INotifyPropertyChanged
{
    private FetchInfo? _fetchInfo;
    private readonly object _lockRoot = new();
    private bool _busy;
    private readonly ITileCache<IFeature?> _tileCache;
    private readonly IDataFetchStrategy _dataFetchStrategy;
    private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = [];
    private readonly ConcurrentHashSet<TileIndex> _tilesThatFailed = [];
    private readonly ITileSchema? _tileSchema;
    private readonly FetchMachine _fetchMachine;
    private readonly Func<TileInfo, Task<IFeature?>> _fetchTileAsFeature;
    private readonly int _fetchThreadCount = 4;

    public TileFetchDispatcher(
        ITileCache<IFeature?> tileCache,
        ITileSchema? tileSchema,
        Func<TileInfo, Task<IFeature?>> fetchTileAsFeature,
        IDataFetchStrategy? dataFetchStrategy = null)
    {
        _tileCache = tileCache;
        _tileSchema = tileSchema;
        _fetchTileAsFeature = fetchTileAsFeature;
        _dataFetchStrategy = dataFetchStrategy ?? new MinimalDataFetchStrategy();
        _fetchMachine = new FetchMachine(_fetchThreadCount);
    }

    public event EventHandler<Exception?>? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public int NumberTilesNeeded { get; private set; }

    public static int MaxTilesInOneRequest { get; set; } = 128;

    public void RefreshData(FetchInfo fetchInfo)
    {
        lock (_lockRoot)
        {
            // Todo: Only refresh if either
            // - The fetchInfo has changes. This not so hard to check.
            // - The data has changed. We have no mechanism for this.


            _tilesThatFailed.Clear(); // Try them again on new data refresh.
            _fetchInfo = fetchInfo;
            Busy = true;
            FetchNextTiles();
        }
    }

    public void FetchNextTiles()
    {
        lock (_lockRoot)
        {
            var tilesToFetch = GetTilesToFetch();

            if (tilesToFetch.Length > 0)
            {
                var tilesToQueue = GetNumberOfTilesToQueue(tilesToFetch);

                for (var i = 0; i < tilesToQueue; i++)
                {
                    var tileToFetch = tilesToFetch[i];
                    _tilesInProgress.Add(tileToFetch.Index);
                    _fetchMachine.Start(() => FetchOnThreadAsync(tileToFetch));
                }
            }
            Busy = _tilesInProgress.Count > 0 || tilesToFetch.Length > 0;
        }
    }

    private int GetNumberOfTilesToQueue(TileInfo[] tilesToFetch)
    {
        var spaceLeftOnQueue = Math.Max(_fetchThreadCount - _tilesInProgress.Count(), 0);
        return Math.Min(tilesToFetch.Length, spaceLeftOnQueue);
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
        lock (_lockRoot)
        {
            if (exception != null)
                _tilesThatFailed.Add(tileInfo.Index);
            else
                _tileCache.Add(tileInfo.Index, feature);

            _tilesInProgress.TryRemove(tileInfo.Index);

            DataChanged?.Invoke(this, exception);

            FetchNextTiles();
        }
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
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private TileInfo[] GetTilesToFetch()
    {
        // Use local fields to avoid changes caused by other threads during this calculation.
        var localFetchInfo = _fetchInfo;
        var localTileSchema = _tileSchema;

        if (localFetchInfo is null || localTileSchema is null)
            return [];

        var levelId = BruTile.Utilities.GetNearestLevel(localTileSchema.Resolutions, localFetchInfo.Resolution);
        var tilesToCoverViewport = _dataFetchStrategy.Get(localTileSchema, localFetchInfo.Extent.ToExtent(), levelId);
        NumberTilesNeeded = tilesToCoverViewport.Count;
        var tilesToFetch = tilesToCoverViewport.Where(t =>
            _tileCache.Find(t.Index) == null
            && !_tilesInProgress.Contains(t.Index)
            && !_tilesThatFailed.Contains(t.Index));
        if (tilesToFetch.Count() > MaxTilesInOneRequest)
        {
            tilesToFetch = tilesToFetch.Take(MaxTilesInOneRequest).ToList();
            Logger.Log(LogLevel.Warning,
                $"The number tiles requested is '{tilesToFetch.Count()}' which exceeds the maximum " +
                $"of '{MaxTilesInOneRequest}'. The number of tiles will be limited to the maximum. Note, " +
                $"that this may indicate a bug or configuration error");
        }

        return tilesToFetch.ToArray();
    }
}
