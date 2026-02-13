using System;

namespace Mapsui.Rendering;

/// <summary>
/// Interface for caches that store pre-created drawable objects per feature.
/// Different implementations can use different eviction strategies
/// (e.g. strict iteration-based for regular layers, LRU for tile layers).
/// Each entry is stamped with the iteration at which it was last used.
/// </summary>
public interface IDrawableCache : IDisposable
{
    /// <summary>
    /// Gets the drawable for a feature, or null if not cached.
    /// Stamps the entry with <paramref name="iteration"/> so that
    /// <see cref="Cleanup"/> knows the entry is still in use.
    /// </summary>
    IDrawable? Get(long featureId, long iteration);

    /// <summary>
    /// Stores a drawable for a feature. Feature IDs are immutable generation IDs —
    /// each ID is only ever set once. If the entry already exists it is not replaced.
    /// The entry is stamped with <paramref name="iteration"/>.
    /// </summary>
    void Set(long featureId, IDrawable drawable, long iteration);

    /// <summary>
    /// Evicts stale cache entries. The exact strategy depends on the implementation:
    /// <list type="bullet">
    ///   <item><see cref="DrawableCache"/>: removes everything not stamped with
    ///         <paramref name="currentIteration"/>.</item>
    ///   <item><see cref="TileDrawableCache"/>: keeps a number of extra tiles and
    ///         removes the oldest by iteration.</item>
    /// </list>
    /// </summary>
    void Cleanup(long currentIteration);

    /// <summary>
    /// Clears all cached entries and disposes them.
    /// </summary>
    void Clear();
}
