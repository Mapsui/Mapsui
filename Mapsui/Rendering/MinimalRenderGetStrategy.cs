using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;

namespace Mapsui.Rendering
{
    public class MinimalRenderGetStrategy<T> : IRenderGetStrategy<T>
    {
        public IList<T> GetFeatures(BoundingBox extent, double resolution, ITileSchema schema, ITileCache<T> memoryCache)
        {
            var tiles = schema.GetTileInfos(extent.ToExtent(), resolution);
            var result = new List<T>();
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
