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
        public void TileFetcherIterationCounterTest()
        {
            // Arrange
            var tileProvider = new CountingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var tileFetcher = new TileFetcher(tileSource, new MemoryCache<Feature>(), 2, 8, new MinimalFetchStrategy());
            var level = "4";

            // Act
            // Get all 64 tiles of level 3
            tileFetcher.ViewChanged(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            
            // Assert
            while (tileFetcher.Busy) { }

            Assert.AreEqual(1, tileFetcher.NumberOfTimesLoopStarted);
    
            Assert.AreEqual(256, tileProvider.CountByTile.Keys.Count);
            //!!! Assert.AreEqual(256, tileProvider.CountByTile.Values.Sum()); ???
            //!!!! Assert.AreEqual(256, tileProvider.TotalCount); ???

            Assert.AreEqual(1, tileProvider.CountByTile[new TileIndex(0, 0, level)]);
            //!!!Assert.AreEqual(1, tileFetcher.IterationsInLoop); // This should not faile

            Assert.Pass("The fetcher did not go into an infinite loop");
        }
    }
}