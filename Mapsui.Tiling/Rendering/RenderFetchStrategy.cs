using System;
using System.Collections.Generic;
using System.Linq;
using BruTile;
using BruTile.Cache;
using Mapsui.Tiling.Extensions;

namespace Mapsui.Tiling.Rendering;

public class RenderFetchStrategy : IRenderFetchStrategy
{
    private readonly int _maxLevelsUp;
    private readonly int _searchUpModeHoldDurationMs;
    private double _previousResolution = double.MaxValue;
    private long _lastSearchUpTimestamp;

    public RenderFetchStrategy(int maxLevelsUp = int.MaxValue, int searchUpModeHoldDurationMs = 0)
    {
        if (maxLevelsUp < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLevelsUp), "maxLevelsUp must be 0 or greater");
        if (searchUpModeHoldDurationMs < 0)
            throw new ArgumentOutOfRangeException(nameof(searchUpModeHoldDurationMs), "searchUpModeHoldDurationMs must be 0 or greater");
        _maxLevelsUp = maxLevelsUp;
        _searchUpModeHoldDurationMs = searchUpModeHoldDurationMs;
    }

    public IList<IFeature> Get(MRect extent, double resolution, ITileSchema schema, ITileCache<IFeature?> memoryCache)
    {
        var dictionary = new Dictionary<TileIndex, IFeature>();
        var level = BruTile.Utilities.GetNearestLevel(schema.Resolutions, resolution);

        var now = Environment.TickCount64;
        var isZoomingIn = resolution < _previousResolution;

        // When zooming in, update the timestamp and use _maxLevelsUp.
        // When not zooming in (panning or zooming out), keep using _maxLevelsUp if we're still within
        // the hold duration. This prevents flickering during pinch gestures where resolution may briefly fluctuate.
        if (isZoomingIn)
        {
            _lastSearchUpTimestamp = now;
        }

        var elapsedMs = now - _lastSearchUpTimestamp;
        // When searchUpModeHoldDurationMs is 0 (default for backward compatibility), always use maxLevelsUp.
        // When searchUpModeHoldDurationMs > 0, only use maxLevelsUp when zooming in or within the hold duration.
        var levelsUp = _searchUpModeHoldDurationMs == 0 || isZoomingIn || elapsedMs < _searchUpModeHoldDurationMs ? _maxLevelsUp : 0;

        GetRecursive(dictionary, schema, memoryCache, extent.ToExtent(), level, levelsUp);

        _previousResolution = resolution;

        var sortedFeatures = dictionary.OrderByDescending(t => schema.Resolutions[t.Key.Level].UnitsPerPixel);
        return sortedFeatures.ToDictionary(pair => pair.Key, pair => pair.Value).Values.ToList();
    }

    public static void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema,
        ITileCache<IFeature?> cache, Extent extent, int level, int maxLevelsUp)
    {
        // to improve performance, convert the resolutions to a list so they can be walked up by
        // simply decrementing an index when the level index needs to change
        var resolutions = schema.Resolutions.OrderByDescending(pair => pair.Value.UnitsPerPixel).ToList();
        for (var i = 0; i < resolutions.Count; i++)
            if (level == resolutions[i].Key)
            {
                GetRecursive(resultTiles, schema, cache, extent, resolutions, i, maxLevelsUp, 0);
                break;
            }
    }

    private static void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema,
        ITileCache<IFeature?> cache, Extent extent, IList<KeyValuePair<int, Resolution>> resolutions, int resolutionIndex, int maxLevelsUp, int currentLevelsUp)
    {
        if (resolutionIndex < 0 || resolutionIndex >= resolutions.Count)
            return;

        var tiles = schema.GetTileInfos(extent, resolutions[resolutionIndex].Key);

        foreach (var tileInfo in tiles)
        {
            var feature = cache.Find(tileInfo.Index);

            // Geometry can be null for some tile sources to indicate the tile is not present.
            // It is stored in the tile cache to prevent retries. It should not be returned to the 
            // renderer.
            if (feature == null)
            {
                // only continue the recursive search if this tile is within the extent and we haven't exceeded the max levels up
                if (tileInfo.Extent.Intersects(extent) && currentLevelsUp < maxLevelsUp)
                {
                    GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), resolutions, resolutionIndex - 1, maxLevelsUp, currentLevelsUp + 1);
                }
            }
            else
            {
                resultTiles[tileInfo.Index] = feature;
            }
        }
    }
}
