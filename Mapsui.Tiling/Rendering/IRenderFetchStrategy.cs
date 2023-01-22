using System.Collections.Generic;
using BruTile;
using BruTile.Cache;

namespace Mapsui.Tiling.Rendering;

public interface IRenderFetchStrategy
{
    /// <summary>
    /// Given the current extent and resolution it determines which tiles should
    /// be returned from the memory cache
    /// </summary>
    /// <param name="extent">The extent of the target area</param>
    /// <param name="resolution">The resolution of the target area</param>
    /// <param name="schema">The tile schema of the tile source</param>
    /// <param name="memoryCache">The cached features from which to select</param>
    /// <returns></returns>
    IList<IFeature> Get(MRect extent, double resolution, ITileSchema schema,
        ITileCache<IFeature?> memoryCache);
}
