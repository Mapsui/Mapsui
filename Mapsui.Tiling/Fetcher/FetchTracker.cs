using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Tiling.Extensions;
using Mapsui.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Mapsui.Tiling.Fetcher;

public class FetchTracker
{
    private readonly object _lock = new();
    private ConcurrentQueue<TileInfo> _tilesToFetch = [];
    private readonly ConcurrentHashSet<TileIndex> _tilesInProgress = [];
    private readonly ConcurrentHashSet<TileIndex> _tilesThatFailed = [];

    public static int MaxTilesInOneRequest { get; set; } = 128;

    public int Update(FetchInfo fetchInfo, ITileSchema tileSchema, IDataFetchStrategy dataFetchStrategy, ITileCache<IFeature?> tileCache)
    {
        lock (_lock)
        {
            _tilesThatFailed.Clear(); // Try them again on new refresh data event.

            var levelId = BruTile.Utilities.GetNearestLevel(tileSchema.Resolutions, fetchInfo.Resolution);
            var tilesToCoverViewport = dataFetchStrategy.Get(tileSchema, fetchInfo.Extent.ToExtent(), levelId);

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

            var queue = new ConcurrentQueue<TileInfo>();

            foreach (var tile in tilesToFetch)
                queue.Add(tile);

            _tilesToFetch = queue;

            return tilesToCoverViewport.Count;
        }
    }

    public bool IsDone()
    {
        lock (_lock)
        {
            return _tilesToFetch.IsEmpty && _tilesInProgress.IsEmpty;
        }
    }

    public void FetchFailed(TileIndex index)
    {
        lock (_lock)
        {
            if (!_tilesInProgress.TryRemove(index))
                Logger.Log(LogLevel.Error, "Could not remove the tile index to the in-progress tiles list. This was not expected");
            if (!_tilesThatFailed.Add(index))
                Logger.Log(LogLevel.Error, "Could not add the tile index to the failed tiles list. This was not expected");
        }
    }

    public void FetchDone(TileIndex index)
    {
        lock (_lock)
        {
            if (!_tilesInProgress.TryRemove(index))
                Logger.Log(LogLevel.Error, "Could not remove the tile index to the in-progress tiles list. This was not expected");
        }
    }

    public bool TryTake([NotNullWhen(true)] out TileInfo? tileInfo, int maxInProgress)
    {
        lock (_lock)
        {
            if (_tilesInProgress.Count >= maxInProgress)
            {
                tileInfo = null;
                return false;
            }
            if (_tilesToFetch.TryDequeue(out tileInfo))
            {
                if (!_tilesInProgress.Add(tileInfo.Index))
                {
                    Logger.Log(LogLevel.Error, "Could not add the tile index to the tiles in-progress list. This was not expected");
                    _tilesToFetch.Enqueue(tileInfo); // Revert the dequeue operation
                    tileInfo = null; // Clear the output parameter
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
