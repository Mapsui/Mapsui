using System;
using BruTile;
using BruTile.Cache;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Manages tile caching with dynamic size adjustment based on viewport requirements.
/// Implements ITileCache for use with tile fetching and rendering strategies.
/// </summary>
/// <remarks>
/// Creates a new TileCache instance with configurable cache size limits
/// </remarks>
/// <param name="updateMinAndMaxTilesInCache">Optional custom function to update cache limits based on viewport needs. 
/// If null, uses default implementation that adds 50 extra tiles to min and 100 extra tiles to max beyond viewport needs.</param>
public sealed class TileCache(Action<MemoryCache<IFeature?>, int>? updateMinAndMaxTilesInCache = null) : ITileCache<IFeature?>, IDisposable
{
    private readonly MemoryCache<IFeature?> _memoryCache = new MemoryCache<IFeature?>();
    private readonly Action<MemoryCache<IFeature?>, int> _updateMinAndMaxTilesInCache = updateMinAndMaxTilesInCache ?? DefaultUpdateMinAndMaxTilesInCache;
    private int _numberTilesNeeded;

    /// <summary>
    /// Default implementation for updating cache limits.
    /// Adds 50 tiles to minimum and 100 tiles to maximum beyond viewport needs.
    /// </summary>
    private static void DefaultUpdateMinAndMaxTilesInCache(MemoryCache<IFeature?> memoryCache, int numberTilesNeeded)
    {
        const int minExtraTiles = 50;
        const int maxExtraTiles = 100;

        memoryCache.MinTiles = numberTilesNeeded + minExtraTiles;
        memoryCache.MaxTiles = numberTilesNeeded + maxExtraTiles;
    }

    /// <summary>
    /// Updates the cache min and max tile limits based on the number of tiles needed by the viewport
    /// </summary>
    /// <param name="numberTilesNeeded">The total number of tiles necessary to fill the area visible in the current viewport.</param>
    public void UpdateMinAndMaxTilesInCache(int numberTilesNeeded)
    {
        if (_numberTilesNeeded == numberTilesNeeded) return;

        _numberTilesNeeded = numberTilesNeeded;
        _updateMinAndMaxTilesInCache(_memoryCache, numberTilesNeeded);
    }

    // ITileCache<IFeature?> implementation

    /// <summary>
    /// Adds a feature to the cache at the specified tile index
    /// </summary>
    /// <param name="index">The tile index</param>
    /// <param name="feature">The feature to cache</param>
    public void Add(TileIndex index, IFeature? feature)
    {
        _memoryCache.Add(index, feature);
    }

    /// <summary>
    /// Removes a cached feature at the specified tile index
    /// </summary>
    /// <param name="index">The tile index</param>
    public void Remove(TileIndex index)
    {
        _memoryCache.Remove(index);
    }

    /// <summary>
    /// Finds and returns a cached feature at the specified tile index
    /// </summary>
    /// <param name="index">The tile index</param>
    /// <returns>The cached feature if found, null otherwise</returns>
    public IFeature? Find(TileIndex index)
    {
        return _memoryCache.Find(index);
    }

    /// <summary>
    /// Clears all cached tiles
    /// </summary>
    public void Clear()
    {
        _memoryCache.Clear();
    }

    /// <summary>
    /// Disposes the cache and releases all resources
    /// </summary>
    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}
