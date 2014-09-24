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
        public void TileFetcherShouldBehaveProperlyWithFailingTileRequests()
        {
            // Arrange
            var schema = new GlobalSphericalMercator();
            var tileSource = new TileSource(new FailingTileProvider(), schema);
            var memoryCache = new MemoryCache<Feature>();
            var tileFetcher = new TileFetcher(tileSource, memoryCache);

            // Act
            tileFetcher.ViewChanged(schema.Extent.ToBoundingBox(), schema.Resolutions["2"].UnitsPerPixel);
            while (tileFetcher.Busy) { }

            // Assert
            Assert.True(memoryCache.TileCount == 0);
        }
    }
}
