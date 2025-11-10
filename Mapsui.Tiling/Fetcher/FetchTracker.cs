using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Tiling.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Mapsui.Tiling.Fetcher;

public class FetchTracker
{
    private readonly object _lock = new();
    private Queue<TileInfo> _tilesToFetch = [];
    private readonly HashSet<TileIndex> _tilesInProgress = [];
    private readonly HashSet<TileIndex> _tilesThatFailed = [];

    public static int MaxTilesInOneRequest { get; set; } = 256;
    public static int DefaultTilesToCoverViewportCount = 32; // We need this number to know how many tiles to keep in cache. Usually we calculate it but this is not always possible. We return something not too big or small.

    public int Update(FetchInfo fetchInfo, ITileSchema tileSchema, IDataFetchStrategy dataFetchStrategy, ITileCache<IFeature?> tileCache)
    {
        lock (_lock)
        {
            _tilesThatFailed.Clear(); // Try them again on new refresh data event.

            var levelId = BruTile.Utilities.GetNearestLevel(tileSchema.Resolutions, fetchInfo.Resolution);

            var tilesToCoverViewport = dataFetchStrategy.Get(tileSchema, fetchInfo.Extent.ToExtent(), levelId);
            var tilesToFetchList = tilesToCoverViewport.Where(t =>
                  tileCache.Find(t.Index) == null
                  && !_tilesInProgress.Contains(t.Index)
                  && !_tilesThatFailed.Contains(t.Index)).ToList();

            if (tilesToFetchList.Count > MaxTilesInOneRequest)
            {
                var message =
                    $"The tiles requested exceeds the maximum. " +
                    $"The number of tiles fetched will be limited to the maximum. " +
                    $"This may indicate a bug or configuration error. " +
                    $"Tiles requested: '{tilesToFetchList.Count}'. " +
                    $"The maximum tiles to request: '{MaxTilesInOneRequest}'. " +
                    $"The level: '{levelId}'. " +
                    $"The resolution: '{fetchInfo.Resolution}'. " +
                    $"The extent: '{fetchInfo.Extent}'.";

                try // Throwing and catching to log the stack trace.
                {
                    throw new InvalidOperationException(message);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message, ex);
                }

                tilesToFetchList = tilesToFetchList.Take(MaxTilesInOneRequest).ToList();
            }

            _tilesToFetch = new Queue<TileInfo>(tilesToFetchList);

            return tilesToCoverViewport.Count;
        }
    }

    public bool IsDone()
    {
        lock (_lock)
        {
            return _tilesToFetch.Count == 0 && _tilesInProgress.Count == 0;
        }
    }

    public void FetchFailed(TileIndex index)
    {
        lock (_lock)
        {
            if (!_tilesInProgress.Remove(index))
                Logger.Log(LogLevel.Error, "Could not remove the tile index to the in-progress tiles list. This was not expected");
            if (!_tilesThatFailed.Add(index))
                Logger.Log(LogLevel.Error, "Could not add the tile index to the failed tiles list. This was not expected");
        }
    }

    public void FetchDone(TileIndex index)
    {
        lock (_lock)
        {
            if (!_tilesInProgress.Remove(index))
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

    public void Clear()
    {
        lock (_lock)
        {
            _tilesToFetch.Clear();
            _tilesInProgress.Clear();
            _tilesThatFailed.Clear();
        }
    }
}
