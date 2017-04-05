using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Rendering
{
    public class MinimalRenderGetStrategy : IRenderGetStrategy
    {
        public IList<IFeature> GetFeatures(BoundingBox extent, double resolution, ITileSchema schema, ITileCache<Feature> memoryCache)
        {
            var tiles = schema.GetTileInfos(extent.ToExtent(), resolution);
            var result = new List<IFeature>();
            foreach (var tileInfo in tiles)
            {
                var feature = memoryCache.Find(tileInfo.Index);

                if (feature != null)
                {
                    result.Add(feature);
                }
            }
            return result;
        }
    }
}
