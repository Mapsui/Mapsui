using System.Collections.Generic;
using System.Linq;
using BruTile;
using BruTile.Cache;
using Mapsui.Tiling.Extensions;

namespace Mapsui.Tiling.Rendering;

public class RenderFetchStrategy : IRenderFetchStrategy
{
    public IList<IFeature> Get(MRect extent, double resolution, ITileSchema schema, ITileCache<IFeature?> memoryCache)
    {
        var dictionary = new Dictionary<TileIndex, IFeature>();
        var level = BruTile.Utilities.GetNearestLevel(schema.Resolutions, resolution);
        GetRecursive(dictionary, schema, memoryCache, extent.ToExtent(), level);
        var sortedFeatures = dictionary.OrderByDescending(t => schema.Resolutions[t.Key.Level].UnitsPerPixel);
        return sortedFeatures.ToDictionary(pair => pair.Key, pair => pair.Value).Values.ToList();
    }

    public static void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema,
        ITileCache<IFeature?> cache, Extent extent, int level)
    {
        // to improve performance, convert the resolutions to a list so they can be walked up by
        // simply decrementing an index when the level index needs to change
        var resolutions = schema.Resolutions.OrderByDescending(pair => pair.Value.UnitsPerPixel).ToList();
        for (var i = 0; i < resolutions.Count; i++)
            if (level == resolutions[i].Key)
            {
                GetRecursive(resultTiles, schema, cache, extent, resolutions, i);
                break;
            }
    }

    private static void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema,
        ITileCache<IFeature?> cache, Extent extent, IList<KeyValuePair<int, Resolution>> resolutions, int resolutionIndex)
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
                // only continue the recursive search if this tile is within the extent
                if (tileInfo.Extent.Intersects(extent))
                {
                    GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), resolutions, resolutionIndex - 1);
                }
            }
            else
            {
                resultTiles[tileInfo.Index] = feature;
            }
        }
    }
}
