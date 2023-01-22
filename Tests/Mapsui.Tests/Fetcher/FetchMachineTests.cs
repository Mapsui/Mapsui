using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Tests.Fetcher.Providers;
using Mapsui.Tiling.Extensions;
using Mapsui.Tiling.Fetcher;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher;

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
        using var cache = new MemoryCache<IFeature?>();
        var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, async tileInfo => await TileToFeatureAsync(tileSource, tileInfo));
        var tileMachine = new FetchMachine(fetchDispatcher);
        var level = 3;
        var expectedTiles = 64;

        var fetchInfo = new FetchInfo(tileSchema.Extent.ToMRect(), tileSchema.Resolutions[level].UnitsPerPixel);

        // Act
        // Get all tiles of level 3
        fetchDispatcher.SetViewport(fetchInfo);
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
        using var cache = new MemoryCache<IFeature?>();
        var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, async tileInfo => await TileToFeatureAsync(tileSource, tileInfo));
        var tileMachine = new FetchMachine(fetchDispatcher);
        var level = 3;
        var expectedTiles = 64;
        var fetchInfo = new FetchInfo(tileSchema.Extent.ToMRect(), tileSchema.Resolutions[level].UnitsPerPixel);

        // Act
        fetchDispatcher.SetViewport(fetchInfo);
        tileMachine.Start();
        while (fetchDispatcher.Busy) { Thread.Sleep(1); }
        var countAfterFirstTry = tileProvider.CountByTile.Keys.Count;
        // do it again
        fetchDispatcher.SetViewport(fetchInfo);
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
        using var cache = new MemoryCache<IFeature?>();
        var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, async tileInfo => await TileToFeatureAsync(tileSource, tileInfo));
        var tileMachine = new FetchMachine(fetchDispatcher);
        var level = 3;
        var tilesInLevel = 64;
        var fetchInfo = new FetchInfo(tileSchema.Extent.ToMRect(), tileSchema.Resolutions[level].UnitsPerPixel);
        // Act
        fetchDispatcher.SetViewport(fetchInfo);
        tileMachine.Start();
        while (fetchDispatcher.Busy) { Thread.Sleep(1); }
        // do it again
        fetchDispatcher.SetViewport(fetchInfo);
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
        using var cache = new MemoryCache<IFeature?>();
        var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, async tileInfo => await TileToFeatureAsync(tileSource, tileInfo));
        var tileMachine = new FetchMachine(fetchDispatcher);
        var level = 3;
        var tilesInLevel = 64;
        var fetchInfo = new FetchInfo(tileSchema.Extent.ToMRect(), tileSchema.Resolutions[level].UnitsPerPixel);

        // Act
        fetchDispatcher.SetViewport(fetchInfo);
        tileMachine.Start();
        while (fetchDispatcher.Busy) { Thread.Sleep(1); }

        // Act again
        fetchDispatcher.SetViewport(fetchInfo);
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
        using var cache = new MemoryCache<IFeature?>();
        var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, async tileInfo => await TileToFeatureAsync(tileSource, tileInfo));
        var tileMachine = new FetchMachine(fetchDispatcher);
        var level = 3;
        var tilesInLevel = 64;
        var fetchInfo = new FetchInfo(tileSchema.Extent.ToMRect(), tileSchema.Resolutions[level].UnitsPerPixel);

        // Act
        fetchDispatcher.SetViewport(fetchInfo);
        tileMachine.Start();
        while (fetchDispatcher.Busy) { Thread.Sleep(1); }

        var tileCountAfterFirstBatch = tileProvider.TotalCount;

        // Act again
        fetchDispatcher.SetViewport(fetchInfo);
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
        using var cache = new MemoryCache<IFeature?>();
        var fetchDispatcher = new TileFetchDispatcher(cache, tileSource.Schema, async tileInfo => await TileToFeatureAsync(tileSource, tileInfo));
        var tileMachine = new FetchMachine(fetchDispatcher);
        var numberOfWorkers = 8;
        var numberOfRestarts = 3;
        var fetchInfo = new FetchInfo(tileSchema.Extent.ToMRect(), tileSchema.Resolutions[3].UnitsPerPixel);

        // Act
        for (var i = 0; i < numberOfRestarts; i++)
        {
            fetchDispatcher.SetViewport(fetchInfo);
            tileMachine.Start();
            while (fetchDispatcher.Busy) { Thread.Sleep(1); }
        }

        // Assert
        Assert.Greater(numberOfWorkers * numberOfRestarts, FetchWorker.RestartCounter);
    }

    private async Task<RasterFeature> TileToFeatureAsync(ITileSource tileProvider, TileInfo tileInfo)
    {
        var tile = await tileProvider.GetTileAsync(tileInfo);
        // A tile layer can return a null value. This indicates the tile is not
        // present in the source, permanently. If this is the case no further 
        // requests should be done. To avoid further fetches a feature should
        // be returned with the Geometry set to null. If a null Feature is returned
        // this equates to having no tile at all and attempts to fetch the tile will
        // continue. TileLayer.ToGeometry() follows the same implementations.
        // 
        // Note, the fact that we have to define this complex method on the outside
        // indicates a design flaw.
        if (tile == null) return new RasterFeature((MRaster?)null);
        return new RasterFeature(new MRaster(tile, tileInfo.Extent.ToMRect()));
    }
}
