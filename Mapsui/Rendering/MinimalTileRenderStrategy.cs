using System.Collections.Generic;
using System.Linq;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Rendering
{
    public class MinimalTileRenderStrategy : ITileRenderStrategy
    {
        public List<Feature> GetFeatures(BoundingBox extent, double resolution, ITileSchema schema, ITileCache<IEnumerable<Feature>> memoryCache)
        {
            var tiles = schema.GetTileInfos(extent.ToExtent(), resolution);
            var result = new List<Feature>();
            foreach (var tileInfo in tiles)
            {
                var feature = memoryCache.Find(tileInfo.Index);

                if (feature != null)
                {
                    result.AddRange(feature);
                }
            }
            return result;
        }
    }
}
