using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Providers;
using Mapsui.Rendering;
using NUnit.Framework;
using System.Globalization;
using Mapsui.Geometries;

namespace Mapsui.Tests.Rendering
{
    [TestFixture]
    public class RenderGetStrategyTests
    {
        [Test]
        public void GetFeaturesWithPartOfOptimalResolutionTilesMissing()
        {
            // arrange
            var schema = new GlobalSphericalMercator();
            var box = schema.Extent.ToBoundingBox();
            const int levelId = 3;
            var resolution = schema.Resolutions[levelId.ToString(CultureInfo.InvariantCulture)];
            var memoryCache = PopulateMemoryCache(schema, new MemoryCache<Feature>(), levelId);
            var renderGetStrategy = new RenderGetStrategy();

            // act
            var tiles = renderGetStrategy.GetFeatures(box, resolution.UnitsPerPixel, schema, memoryCache);

            // assert
            Assert.True(tiles.Count == 43);
        }

        private static ITileCache<Feature> PopulateMemoryCache(GlobalSphericalMercator schema, MemoryCache<Feature> cache, int levelId)
        {
            for (var i = levelId; i >= 0; i--)
            {
                var tiles = schema.GetTileInfos(schema.Extent, i.ToString(CultureInfo.InvariantCulture));
                foreach (var tile in tiles)
                {
                    if ((tile.Index.Col + tile.Index.Row) % 2 == 0) // Add only 50% of the tiles with the arbitrary rule.
                    {
                        cache.Add(tile.Index, new Feature { Geometry = new Point()});
                    }
                }
            }
            return cache;
        }
    }
}
