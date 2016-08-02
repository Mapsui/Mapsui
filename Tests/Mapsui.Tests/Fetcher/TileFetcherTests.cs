using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public void TileFetcherShouldBehaveProperlyWithNoisyResponses()
        {
            // Arrange
            var schema = new GlobalSphericalMercator();
            var tileSource = new TileSource(new SometimesFailingTileProvider(), schema);
            var memoryCache = new MemoryCache<Feature>(14, 17);
            var tileFetcher = new TileFetcher(tileSource, memoryCache);
            var random = new Random(31747074);
            var tiles = new List<Feature>();

            // Act
            for (int i = 0; i < 100; i++)
            {
                var randomLevel = "5";
                var randomCol = random.Next(schema.GetMatrixWidth(randomLevel));
                var randomRow = random.Next(schema.GetMatrixHeight(randomLevel));
                var tileRange = new TileRange(randomCol - 2, randomRow - 2, 5, 5);
                var unitsPerPixel = schema.Resolutions[randomLevel].UnitsPerPixel;
                var extent = TileTransform.TileToWorld(tileRange, randomLevel, schema);
                tileFetcher.ViewChanged(TileTransform.TileToWorld(tileRange, randomLevel, schema).ToBoundingBox(),unitsPerPixel );
                var tileInfos = schema.GetTileInfos(extent, randomLevel);
                foreach (var tileInfo in tileInfos)
                {
                    tiles.Add(memoryCache.Find(tileInfo.Index));

                }
            }

            // Assert
            Assert.True(tiles.Count > 0);
            Assert.True(memoryCache.TileCount == 0);
        }

        public int GetTileCount(ITileSchema schema)
        {
            var result = 0;

            foreach (var resolution in schema.Resolutions)
            {
                result += resolution.Value.MatrixHeight*resolution.Value.MatrixWidth;
            }

            return result;
        }

        [Test]
        [Ignore] // handing in build server but not on client
        public void TileFetcherShouldBehaveProperlyWithFailingTileRequests()
        {
            // Arrange
            var schema = new GlobalSphericalMercator();
            var tileSource = new TileSource(new FailingTileProvider(), schema);
            var memoryCache = new MemoryCache<Feature>();
            var tileFetcher = new TileFetcher(tileSource, memoryCache);

            // Act
            Task.Run(() =>
            {
                Task.Delay(5000);
                if (tileFetcher.Busy) Assert.Fail("The fetcher hangs");
            });

            tileFetcher.ViewChanged(schema.Extent.ToBoundingBox(), schema.Resolutions["2"].UnitsPerPixel);
            while (tileFetcher.Busy) { }

            // Assert
            Assert.True(memoryCache.TileCount == 0);
        }

        [Test] // handing in build server but not on client
        public void TileFetcherShouldBehaveProperlyWithTileProviderReturningNull()
        {
            // Arrange
            var schema = new GlobalSphericalMercator();
            var tileSource = new TileSource(new NullTileProvider(), schema);
            var memoryCache = new MemoryCache<Feature>();
            var tileFetcher = new TileFetcher(tileSource, memoryCache);

            // Act
            Task.Run(() =>
            {
                Task.Delay(5000);
                if (tileFetcher.Busy) Assert.Fail("The fetcher hangs");
            });
            tileFetcher.ViewChanged(schema.Extent.ToBoundingBox(), schema.Resolutions["2"].UnitsPerPixel);
            while (tileFetcher.Busy) { }

            // Assert
            Assert.True(memoryCache.TileCount == 0);
        }
    }
}
