using System;
using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Tiling.Extensions;
using Mapsui.Tiling.Rendering;
using NUnit.Framework;

namespace Mapsui.Tests.Rendering;

[TestFixture]
public class RenderFetchStrategyTests
{
    [TestCase(-1, 0, Description = "Negative maxLevelsUp should throw")]
    [TestCase(0, -1, Description = "Negative searchUpModeHoldDurationMs should throw")]
    public void ConstructorShouldThrowForInvalidParameters(int maxLevelsUp, int searchUpModeHoldDurationMs)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderFetchStrategy(maxLevelsUp, searchUpModeHoldDurationMs));
    }

    [Test]
    public void GetWithDefaultParametersShouldAlwaysFallbackToHigherLevelTiles()
    {
        // arrange
        var schema = new GlobalSphericalMercator();
        var box = schema.Extent.ToMRect();
        const int level = 3;
        var resolution = schema.Resolutions[level];
        var memoryCache = PopulateMemoryCachePartially(schema, new MemoryCache<IFeature?>(), level);
        // Default parameters should preserve original behavior (always fallback)
        var renderFetchStrategy = new RenderFetchStrategy();

        // act
        var tiles = renderFetchStrategy.Get(box, resolution.UnitsPerPixel, schema, memoryCache);

        // assert - with default searchUpModeHoldDurationMs=0, fallback always happens (backward compatible)
        Assert.That(tiles.Count, Is.GreaterThan(0));
    }

    [Test]
    public void GetWithHoldDurationShouldOnlyFallbackWhenZoomingIn()
    {
        // arrange
        var schema = new GlobalSphericalMercator();
        var box = schema.Extent.ToMRect();
        const int level = 2;
        var resolution = schema.Resolutions[level];
        // Only populate level 0 and 1, not level 2
        using var cache = new MemoryCache<IFeature?>();
        PopulateLevelCompletely(schema, cache, 0);
        PopulateLevelCompletely(schema, cache, 1);
        // Level 2 is empty
        // With searchUpModeHoldDurationMs > 0, fallback only happens when zooming in
        var renderFetchStrategy = new RenderFetchStrategy(searchUpModeHoldDurationMs: 100);

        // First call - not zooming in (first call is baseline)
        var lowerResolution = schema.Resolutions[level].UnitsPerPixel;
        var tilesFirstCall = renderFetchStrategy.Get(box, lowerResolution, schema, cache);

        // Second call with same resolution - not zooming in, hold duration expired
        System.Threading.Thread.Sleep(150); // Wait for hold duration to expire
        var tilesAfterHold = renderFetchStrategy.Get(box, lowerResolution, schema, cache);

        // assert - after hold duration expires and not zooming in, no fallback
        Assert.That(tilesAfterHold.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetWithMaxLevelsUpZeroShouldNotFallback()
    {
        // arrange
        var schema = new GlobalSphericalMercator();
        var box = schema.Extent.ToMRect();
        const int level = 2;
        var resolution = schema.Resolutions[level];
        // Only populate level 0 and 1, not level 2
        using var cache = new MemoryCache<IFeature?>();
        PopulateLevelCompletely(schema, cache, 0);
        PopulateLevelCompletely(schema, cache, 1);
        // Level 2 is empty
        var renderFetchStrategy = new RenderFetchStrategy(maxLevelsUp: 0);

        // act
        var tiles = renderFetchStrategy.Get(box, resolution.UnitsPerPixel, schema, cache);

        // assert - maxLevelsUp=0 means no fallback
        Assert.That(tiles.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetWithMaxLevelsUpShouldLimitFallbackDepth()
    {
        // arrange
        var schema = new GlobalSphericalMercator();
        var box = schema.Extent.ToMRect();
        const int level = 3;
        var resolution = schema.Resolutions[level];
        // Only populate level 0 (1 tile)
        using var cache = new MemoryCache<IFeature?>();
        PopulateLevelCompletely(schema, cache, 0);
        // Levels 1, 2, 3 are empty

        var strategyUnlimited = new RenderFetchStrategy(maxLevelsUp: int.MaxValue);
        var strategyLimited = new RenderFetchStrategy(maxLevelsUp: 1);

        // act
        var tilesUnlimited = strategyUnlimited.Get(box, resolution.UnitsPerPixel, schema, cache);
        var tilesLimited = strategyLimited.Get(box, resolution.UnitsPerPixel, schema, cache);

        // assert
        // Unlimited should find level 0 tile (3 levels up from level 3)
        Assert.That(tilesUnlimited.Count, Is.EqualTo(1));
        // Limited to 1 level up should not reach level 0 from level 3
        Assert.That(tilesLimited.Count, Is.EqualTo(0));
    }

    private static ITileCache<IFeature?> PopulateMemoryCachePartially(GlobalSphericalMercator schema, MemoryCache<IFeature?> cache, int maxLevel)
    {
        for (var i = maxLevel; i >= 0; i--)
        {
            var tiles = schema.GetTileInfos(schema.Extent, i);
            foreach (var tile in tiles)
            {
                // Add only 50% of the tiles with arbitrary rule
                if ((tile.Index.Col + tile.Index.Row) % 2 == 0)
                {
                    cache.Add(tile.Index, new RasterFeature(new MRaster(Array.Empty<byte>(), new MRect(0, 0, 1, 1))));
                }
            }
        }
        return cache;
    }

    private static void PopulateLevelCompletely(GlobalSphericalMercator schema, MemoryCache<IFeature?> cache, int level)
    {
        var tiles = schema.GetTileInfos(schema.Extent, level);
        foreach (var tile in tiles)
        {
            cache.Add(tile.Index, new RasterFeature(new MRaster(Array.Empty<byte>(), new MRect(0, 0, 1, 1))));
        }
    }
}
