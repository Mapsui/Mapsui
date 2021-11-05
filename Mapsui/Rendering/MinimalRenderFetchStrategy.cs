using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;

namespace Mapsui.Rendering
{
    public class MinimalRenderFetchStrategy : IRenderFetchStrategy
    {
        public IList<IFeature> Get(BoundingBox? extent, double resolution, ITileSchema schema, ITileCache<GeometryFeature> memoryCache)
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
