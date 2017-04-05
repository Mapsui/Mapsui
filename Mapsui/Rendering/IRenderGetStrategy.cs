using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Rendering
{
    public interface IRenderGetStrategy
    {
        /// <summary>
        /// Given the current extent and resolution it determines which tiles should
        /// be returned from the memorycache
        /// </summary>
        /// <param name="extent">The extent of the target area</param>
        /// <param name="resolution">The resolution of the target area</param>
        /// <param name="schema">The tile schema of the tile source</param>
        /// <param name="memoryCache">The cached features from which to select</param>
        /// <returns></returns>
        IList<IFeature> GetFeatures(BoundingBox extent, double resolution, ITileSchema schema,
            ITileCache<Feature> memoryCache);
    }
}
