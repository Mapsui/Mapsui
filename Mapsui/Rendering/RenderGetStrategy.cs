using System;
using System.Collections.Generic;
using System.Linq;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Rendering
{
    public class RenderGetStrategy : IRenderGetStrategy
    {
        public IList<IFeature> GetFeatures(BoundingBox box, double resolution, ITileSchema schema, ITileCache<Feature> memoryCache)
        {
            var dictionary = new Dictionary<TileIndex, IFeature>();
            var levelId = BruTile.Utilities.GetNearestLevel(schema.Resolutions, resolution);
            GetRecursive(dictionary, schema, memoryCache, box.ToExtent(), levelId);
            var sortedFeatures = dictionary.OrderByDescending(t => schema.Resolutions[t.Key.Level].UnitsPerPixel);
            return sortedFeatures.ToDictionary(pair => pair.Key, pair => pair.Value).Values.ToList();
        }
        
        public static void GetRecursive(IDictionary<TileIndex, IFeature> resultTiles, ITileSchema schema,
            ITileCache<Feature> cache, Extent extent, string levelId)
        {
            var resolution = schema.Resolutions[levelId].UnitsPerPixel;
            var tiles = schema.GetTilesInView(extent, resolution);

            foreach (var tileInfo in tiles)
            {
                var feature = cache.Find(tileInfo.Index);
                var nextLevelId = schema.Resolutions.Where(r => r.Value.UnitsPerPixel > resolution)
                       .OrderBy(r => r.Value.UnitsPerPixel).FirstOrDefault().Key;

                if (feature == null)
                {
                    if (nextLevelId != null) GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), nextLevelId);
                }
                else
                {
                    resultTiles[tileInfo.Index] = feature;
                    if (!IsFullyShown(feature))
                    {
                        if (nextLevelId != null) GetRecursive(resultTiles, schema, cache, tileInfo.Extent.Intersect(extent), nextLevelId);
                    }
                }
            }
        }

        public static bool IsFullyShown(Feature feature)
        {
            var currentTile = DateTime.Now.Ticks;
            var tile = ((IRaster)feature.Geometry);
            const long second = 10000000;
            return ((currentTile - tile.TickFetched) > second);
        }
    }
}
