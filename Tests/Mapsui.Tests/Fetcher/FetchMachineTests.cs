using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Tests.Fetcher.Providers;
using Mapsui.Tiling.Extensions;
using Mapsui.Tiling.Fetcher;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class FetchMachineTests
{
#pragma warning disable IDISP006 // Implement IDisposable
    MemoryLayer _layer = new();
#pragma warning restore IDISP006 // Implement IDisposable

    [Test]
    public async Task TileFetcherShouldRequestAllTilesJustOnesAsync()
    {
        // Arrange
        var tileSource = new CountingTileSource();
        using var cache = new MemoryCache<IFeature?>();
        var fetchJobPlanner = new TileFetchJobPlanner(cache, tileSource.Schema, async tileInfo => await TileToFeatureAsync(tileSource, tileInfo), new MinimalDataFetchStrategy(), _layer);
        var fetchMachine = new FetchMachine();
        var level = 3;
        var expectedTiles = 64;
        var fetchInfo = new FetchInfo(new MSection(tileSource.Schema.Extent.ToMRect(), tileSource.Schema.Resolutions[level].UnitsPerPixel));
        fetchJobPlanner.ViewportChanged(fetchInfo);

        // Act
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        // Assert
        ClassicAssert.AreEqual(expectedTiles, tileSource.CountByTile.Keys.Count);
        ClassicAssert.AreEqual(expectedTiles, tileSource.CountByTile.Values.Sum());
        ClassicAssert.AreEqual(expectedTiles, tileSource.TotalCount);
    }

    [Test]
    public async Task TilesFetchedShouldNotBeFetchAgainAsync()
    {
        // Arrange
        var tileSource = new CountingTileSource();
        using var cache = new MemoryCache<IFeature?>();
        var fetchJobPlanner = new TileFetchJobPlanner(
            cache,
            tileSource.Schema,
            async tileInfo => await TileToFeatureAsync(tileSource, tileInfo),
            new MinimalDataFetchStrategy(),
            _layer);
        var level = 3;
        var expectedTiles = 64;
        var fetchInfo = new FetchInfo(new MSection(tileSource.Schema.Extent.ToMRect(), tileSource.Schema.Resolutions[level].UnitsPerPixel));
        fetchJobPlanner.ViewportChanged(fetchInfo);

        // Act (first round)
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        var countAfterFirstTry = tileSource.CountByTile.Keys.Count;

        // Act (second round)
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        // Assert
        ClassicAssert.AreEqual(countAfterFirstTry, tileSource.CountByTile.Values.Sum());
        ClassicAssert.AreEqual(expectedTiles, tileSource.CountByTile.Keys.Count);
        ClassicAssert.AreEqual(expectedTiles, tileSource.CountByTile.Values.Sum());
        ClassicAssert.AreEqual(expectedTiles, tileSource.TotalCount);
    }

    [Test]
    public async Task TileRequestThatReturnsNullShouldNotBeRequestedAgainAsync()
    {
        // Arrange
        var tileSource = new NullTileSource();
        using var cache = new MemoryCache<IFeature?>();
        var fetchJobPlanner = new TileFetchJobPlanner(
            cache,
            tileSource.Schema,
            async tileInfo => await TileToFeatureAsync(tileSource, tileInfo),
            new MinimalDataFetchStrategy(),
            _layer);
        var level = 3;
        var tilesInLevel = 64;
        var fetchInfo = new FetchInfo(new MSection(tileSource.Schema.Extent.ToMRect(), tileSource.Schema.Resolutions[level].UnitsPerPixel));
        fetchJobPlanner.ViewportChanged(fetchInfo);

        // Act (first round)
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        // Act (second round)
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        // Assert
        ClassicAssert.AreEqual(tilesInLevel, tileSource.TotalCount);
    }

    [Test]
    public async Task TileFetcherWithFailingFetchesShouldTryAgainAsync()
    {
        // Arrange
        var tileSource = new FailingTileSource();
        using var cache = new MemoryCache<IFeature?>();
        var fetchJobPlanner = new TileFetchJobPlanner(
            cache,
            tileSource.Schema,
            async tileInfo => await TileToFeatureAsync(tileSource, tileInfo),
            new MinimalDataFetchStrategy(),
            _layer);
        var level = 3;
        var tilesInLevel = 64;
        var fetchInfo = new FetchInfo(new MSection(tileSource.Schema.Extent.ToMRect(), tileSource.Schema.Resolutions[level].UnitsPerPixel));
        var fetchMachine = new FetchMachine();

        // Act (first round)
        fetchJobPlanner.ViewportChanged(fetchInfo);
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);


        // Act (second round)
        fetchJobPlanner.ViewportChanged(fetchInfo);
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        // Assert
        ClassicAssert.AreEqual(tilesInLevel * 2, tileSource.TotalCount); // tried all tiles twice
    }

    [Test]
    public async Task TileFetcherWithSometimesFailingFetchesShouldTryAgainAsync()
    {
        // Arrange
        var tileSource = new SometimesFailingTileSource();
        var tileSchema = new GlobalSphericalMercator();
        using var cache = new MemoryCache<IFeature?>();
        var fetchJobPlanner = new TileFetchJobPlanner(
            cache,
            tileSource.Schema,
            async tileInfo => await TileToFeatureAsync(tileSource, tileInfo),
            new MinimalDataFetchStrategy(),
            _layer);
        var level = 3;
        var tilesInLevel = 64;
        var fetchInfo = new FetchInfo(new MSection(tileSchema.Extent.ToMRect(), tileSchema.Resolutions[level].UnitsPerPixel));
        fetchJobPlanner.ViewportChanged(fetchInfo);

        // Act (first round)
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        var tileCountAfterFirstBatch = tileSource.TotalCount;

        // Act (second round)
        do
        {
            var requests = fetchJobPlanner.GetFetchJobs(0, 8);
            foreach (var request in requests)
            {
                await request.FetchFunc();
            }
        } while (fetchJobPlanner.Busy);

        // Assert
        ClassicAssert.GreaterOrEqual(tileSource.TotalCount, tileCountAfterFirstBatch);
        ClassicAssert.GreaterOrEqual(tileSource.CountByTile.Values.Sum(), tilesInLevel);
    }

    private static async Task<RasterFeature?> TileToFeatureAsync(ILocalTileSource tileSource, TileInfo tileInfo)
    {
        // A tile layer can return a null value. This indicates the tile is not
        // present in the source, permanently. If this is the case no further 
        // requests should be done. To avoid further fetches a feature should
        // be returned with the Geometry set to null. If a null Feature is returned
        // this equates to having no tile at all and there should be no other attempts
        // to fetch the tile. 
        // 
        // Note, the fact that we have to define this complex method on the outside
        // indicates a design flaw.
        var tile = await tileSource.GetTileAsync(tileInfo);
        if (tile == null)
            return new RasterFeature((MRaster?)null);

        return new RasterFeature(new MRaster(tile, tileInfo.Extent.ToMRect()));
    }
}
