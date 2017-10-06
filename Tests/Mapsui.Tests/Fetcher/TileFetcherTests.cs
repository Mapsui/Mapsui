using System.Linq;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher
{
    [TestFixture]
    public class TileFetcherTests
    {
        [Test]
        public void TileFetcherWithFailingFetchesTest()
        {
            // Arrange
            var tileProvider = new SometimesFailingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var tileFetcher = new TileFetcher(tileSource, new MemoryCache<Feature>(), 2, 8);

            // Act
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tileFetcher.ViewChanged(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[j.ToString()].UnitsPerPixel);
                }
            }

            // Assert
            while (tileFetcher.Busy) { }

            Assert.Pass("The fetcher did not go into an infinite loop");
        }

        [Test]
        public void TileFetcherWithReturningNull()
        {
            // Arrange
            var tileProvider = new NullTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var tileFetcher = new TileFetcher(tileSource, new MemoryCache<Feature>(), 2, 8);

            // Act
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tileFetcher.ViewChanged(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[j.ToString()].UnitsPerPixel);
                }
            }

            // Assert
            while (tileFetcher.Busy) { }

            Assert.Pass("The fetcher did not go into an infinite loop");
        }

        [Test]
        public void TileFetcherShouldRequestAllTilesJustOnes()
        {
            // Arrange
            var tileProvider = new CountingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var tileFetcher = new TileFetcher(tileSource, new MemoryCache<Feature>(), 2, 8, new MinimalFetchStrategy());
            var level = "4";
            var expextedTiles = 256;

            // Act
            // Get all tiles of level 3
            tileFetcher.ViewChanged(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            
            // Assert
            while (tileFetcher.Busy) { }
   
            Assert.AreEqual(expextedTiles, tileProvider.CountByTile.Keys.Count);
            Assert.AreEqual(expextedTiles, tileProvider.CountByTile.Values.Sum());
            Assert.AreEqual(expextedTiles, tileProvider.TotalCount);

            Assert.Pass("The fetcher did not go into an infinite loop");
        }
    }
}