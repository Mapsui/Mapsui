using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using ConcurrentCollections;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Tiling.Extensions;

namespace Mapsui.Tiling.Fetcher;

public class TileFetchDispatcher : IFetchDispatcher, INotifyPropertyChanged
{
    private FetchInfo? _fetchInfo;
    private readonly object _lockRoot = new();
    private bool _busy;
    private bool _viewportIsModified;
    private readonly ITileCache<IFeature?> _tileCache;
    private readonly IDataFetchStrategy _dataFetchStrategy;
    private readonly ConcurrentQueue<TileInfo> _tilesToFetch = new();
    private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = new();
    private readonly ITileSchema? _tileSchema;
    private readonly FetchMachine _fetchMachine;
    private readonly Func<TileInfo, Task<IFeature?>> _fetchTileAsFeature;

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
        _fetchMachine = new FetchMachine(this);
    }

    public event DataChangedEventHandler? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public int NumberTilesNeeded { get; private set; }

    public void SetViewport(FetchInfo fetchInfo)
    {
        lock (_lockRoot)
        {
            _fetchInfo = fetchInfo;
            Busy = true;
            _viewportIsModified = true;
        }
    }

    public bool TryTake([NotNullWhen(true)] out Func<Task>? method)
    {
        lock (_lockRoot)
        {
            UpdateIfViewportIsModified();
            if (_tilesToFetch.TryDequeue(out var tileInfo))
            {
                _tilesInProgress.Add(tileInfo.Index);
                method = async () => await FetchOnThreadAsync(tileInfo);
                return true;
            }

            Busy = _tilesInProgress.Count > 0 || _tilesToFetch.Count > 0;
            // else the queue is empty, we are done.
            method = null;
            return false;
        }
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    private async Task FetchOnThreadAsync(TileInfo tileInfo)
    {
        try
        {
            var feature = await _fetchTileAsFeature(tileInfo);
            FetchCompleted(tileInfo, feature, null);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            FetchCompleted(tileInfo, null, ex);
        }
    }

    private void UpdateIfViewportIsModified()
    {
        if (_viewportIsModified)
        {
            UpdateTilesToFetchForViewportChange();
            _viewportIsModified = false;
        }
    }

    private void FetchCompleted(TileInfo tileInfo, IFeature? feature, Exception? exception)
    {
        lock (_lockRoot)
        {
            if (exception == null)
                _tileCache.Add(tileInfo.Index, feature);
            _tilesInProgress.TryRemove(tileInfo.Index);

            Busy = _tilesInProgress.Count > 0 || _tilesToFetch.Count > 0;

            DataChanged?.Invoke(this, new DataChangedEventArgs(exception, false, tileInfo));
        }
    }

    public bool Busy
    {
        get => _busy;
        private set
        {
            if (_busy == value) return; // prevent notify              
            _busy = value;
            OnPropertyChanged(nameof(Busy));
        }
    }

    public void StopFetching()
    {
        _fetchMachine.Stop();
    }

    public void StartFetching()
    {
        _fetchMachine.Start();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateTilesToFetchForViewportChange()
    {
        if (_fetchInfo == null || _tileSchema == null)
            return;

        var levelId = BruTile.Utilities.GetNearestLevel(_tileSchema.Resolutions, _fetchInfo.Resolution);
        var tilesToCoverViewport = _dataFetchStrategy.Get(_tileSchema, _fetchInfo.Extent.ToExtent(), levelId);
        NumberTilesNeeded = tilesToCoverViewport.Count;
        var tilesToFetch = tilesToCoverViewport.Where(t => _tileCache.Find(t.Index) == null && !_tilesInProgress.Contains(t.Index));
        _tilesToFetch.Clear();
        _tilesToFetch.AddRange(tilesToFetch);
        if (_tilesToFetch.Count > 0) Busy = true;
    }
}
