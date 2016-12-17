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
            var tileSource =  new TileSource(tileProvider, tileSchema);
            var tileFetcher = new TileFetcher(tileSource, new MemoryCache<Feature>());

            // Act
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    tileFetcher.ViewChanged(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[j.ToString()].UnitsPerPixel);
                    System.Threading.Thread.Sleep(10);
                }
            }
            
            // Assert
            while (tileFetcher.Busy) {}

            Assert.Pass("The fetcher did not go into an infinite loop");
        }

        [Test]
        public void TileFetcherWithReturningNull()
        {
            // Arrange
            var tileProvider = new NullTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var tileFetcher = new TileFetcher(tileSource, new MemoryCache<Feature>());

            // Act
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    tileFetcher.ViewChanged(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[j.ToString()].UnitsPerPixel);
                    System.Threading.Thread.Sleep(10);
                }
            }

            // Assert
            while (tileFetcher.Busy) { }

            Assert.Pass("The fetcher did not go into an infinite loop");
        }
    }
}