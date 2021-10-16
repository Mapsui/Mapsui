using System.IO;
using System.Linq;
using System.Threading;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Tests.Fetcher.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher
{
    [TestFixture]
    public class FetchMachineTests
    {
        // Note, The Thread.Sleep(1) in the while loop is necessary to avoid
        // a hang in some rare cases.

        [Test]
        public void TileFetcherShouldRequestAllTilesJustOnes()
        {
            // Arrange
            var tileProvider = new CountingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, tileInfo => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 3;
            var expectedTiles = 64;

            // Act
            // Get all tiles of level 3
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            // Assert
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }

            Assert.AreEqual(expectedTiles, tileProvider.CountByTile.Keys.Count);
            Assert.AreEqual(expectedTiles, tileProvider.CountByTile.Values.Sum());
            Assert.AreEqual(expectedTiles, tileProvider.TotalCount);
        }

        [Test]
        public void TilesFetchedShouldNotBeFetchAgain()
        {
            // Arrange
            var tileProvider = new CountingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, tileInfo => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 3;
            var expectedTiles = 64;

            // Act
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }
            var countAfterFirstTry = tileProvider.CountByTile.Keys.Count;
            // do it again
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }

            // Assert
            Assert.AreEqual(countAfterFirstTry, tileProvider.CountByTile.Values.Sum());
            Assert.AreEqual(expectedTiles, tileProvider.CountByTile.Keys.Count);
            Assert.AreEqual(expectedTiles, tileProvider.CountByTile.Values.Sum());
            Assert.AreEqual(expectedTiles, tileProvider.TotalCount);
        }


        [Test]
        public void TileRequestThatReturnsNullShouldNotBeRequestedAgain()
        {
            // Arrange
            var tileProvider = new NullTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, tileInfo => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 3;
            var tilesInLevel = 64;

            // Act
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }
            // do it again
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }
            
            // Assert
            Assert.AreEqual(tilesInLevel, tileProvider.TotalCount);
        }

        [Test]
        public void TileFetcherWithFailingFetchesShouldTryAgain()
        {
            // Arrange
            var tileProvider = new FailingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, tileInfo => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 3;
            var tilesInLevel = 64;

            // Act
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }

            // Act again
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }

            // Assert
            Assert.AreEqual(tilesInLevel * 2, tileProvider.TotalCount); // tried all tiles twice
        }

        [Test]
        public void TileFetcherWithSometimesFailingFetchesShouldTryAgain()
        {
            // Arrange
            var tileProvider = new SometimesFailingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, tileInfo => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 3;
            var tilesInLevel = 64;

            // Act
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }

            var tileCountAfterFirstBatch = tileProvider.TotalCount;

            // Act again
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }

            // Assert
            Assert.GreaterOrEqual(tileProvider.TotalCount, tileCountAfterFirstBatch);
            Assert.GreaterOrEqual(tileProvider.CountByTile.Values.Sum(), tilesInLevel);

        }

        [Test]
        public void RepeatedRestartsShouldNotCauseInfiniteLoop()
        {
            // Arrange
            var tileProvider = new CountingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, tileInfo => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var numberOfWorkers = 8;
            var numberOfRestarts = 3;

            // Act
            for (int i = 0; i < numberOfRestarts; i++)
            {
                fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[3].UnitsPerPixel);
                tileMachine.Start();
                while (fetchDispatcher.Busy) { Thread.Sleep(1); }
            }

            // Assert
            Assert.Greater(numberOfWorkers * numberOfRestarts, FetchWorker.RestartCounter);
        }

        private Feature TileToFeature(ITileSource tileProvider, TileInfo tileInfo)
        {
            var tile = tileProvider.GetTile(tileInfo);
            // A tile layer can return a null value. This indicates the tile is not
            // present in the source, permanently. If this is the case no further 
            // requests should be done. To avoid further fetches a feature should
            // be returned with the Geometry set to null. If a null Feature is returned
            // this equates to having no tile at all and attempts to fetch the tile will
            // continue. TileLayer.ToGeometry() follows the same implementations.
            // 
            // Note, the fact that we have to define this complex method on the outside
            // indicates a design flaw.
            if (tile == null) return new Feature { Geometry = null }; 
            return new Feature { Geometry = new Raster(new MemoryStream(tile), tileInfo.Extent.ToBoundingBox()) };
        }
    }
}