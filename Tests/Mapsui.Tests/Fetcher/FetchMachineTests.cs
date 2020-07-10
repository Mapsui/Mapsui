using System;
using System.Linq;
using System.Threading;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher
{
    [TestFixture]
    public class FetchMachineTests
    {
        [Test]
        public void TileFetcherShouldRequestAllTilesJustOnes()
        {
            // Arrange
            var tileProvider = new CountingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, (tileInfo) => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 4;
            var expextedTiles = 256;

            // Act
            // Get all tiles of level 3
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            // Assert
            while (fetchDispatcher.Busy) { }

            Assert.AreEqual(expextedTiles, tileProvider.CountByTile.Keys.Count);
            Assert.AreEqual(expextedTiles, tileProvider.CountByTile.Values.Sum());
            Assert.AreEqual(expextedTiles, tileProvider.TotalCount);
        }

        private Feature TileToFeature(ITileSource tileProvider, TileInfo tileInfo)
        {
            var tile = tileProvider.GetTile(tileInfo); 
            return new Feature();
        }

        [Test]
        public void TileRequestThatReturnsNullShouldNotBeRequestedAgain()
        {
            // Arrange
            var tileProvider = new NullTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, (tileInfo) => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 3;
            var tilesInLevel = 64;

            // Act
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { }
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
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, (tileInfo) => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var level = 3;
            var tilesInLevel = 64;

            // Act
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { }
            // do it again
            fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[level].UnitsPerPixel);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { }

            // Assert
            Assert.AreEqual(tilesInLevel * 2, tileProvider.TotalCount); // tried all tiles twice
        }
        
        [Test]
        public void RepeatedRestartsShouldNotCauseInfiniteLoop()
        {
            // Arrange
            var tileProvider = new CountingTileProvider();
            var tileSchema = new GlobalSphericalMercator();
            var tileSource = new TileSource(tileProvider, tileSchema);
            var cache = new MemoryCache<Feature>();
            var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, (tileInfo) => TileToFeature(tileSource, tileInfo));
            var tileMachine = new FetchMachine(fetchDispatcher);
            var numberOfWorkers = 8;
            var numberOfRestarts = 3;

            // Act
            for (int i = 0; i < numberOfRestarts; i++)
            {
                fetchDispatcher.SetViewport(tileSchema.Extent.ToBoundingBox(), tileSchema.Resolutions[3].UnitsPerPixel);
                tileMachine.Start();
                while (fetchDispatcher.Busy) { }
            }

            // Assert
            Assert.Greater(numberOfWorkers * numberOfRestarts, FetchWorker.RestartCounter);
        }
    }
}