using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Tiling.Extensions;
using Mapsui.Tiling.Fetcher;

namespace Mapsui.Tiling.Rendering;

public class MinimalRenderFetchStrategy : IRenderFetchStrategy
{
    public IList<IFeature> Get(MRect? extent, double resolution, ITileSchema schema, ITileCache<IFeature?> memoryCache)
    {
        var result = new List<IFeature>();
        if (extent == null)
            return result;

        var tiles = schema.GetTileInfos(extent.ToExtent(), resolution);
        foreach (var tileInfo in tiles)
        {
            var feature = memoryCache.Find(tileInfo.Index);

            if (feature != null && feature is not TileFetchStatusFeature)
                result.Add(feature);
        }
        return result;
    }
}
