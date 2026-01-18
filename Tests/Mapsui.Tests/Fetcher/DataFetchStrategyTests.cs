// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using BruTile;
using BruTile.Predefined;
using Mapsui.Tiling.Fetcher;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher;

[TestFixture]
public class DataFetchStrategyTests
{
    [Test]
    public void GetShouldReturnTilesFromMultipleLevelsLimitedByMaxLevelsUp()
    {
        // arrange
        var schema = new GlobalSphericalMercator();
        var extent = schema.Extent;
        const int level = 2;

        // act
        var strategyDefault = new DataFetchStrategy();
        var tilesDefault = strategyDefault.Get(schema, extent, level);
        var strategyLimited = new DataFetchStrategy(maxLevelsUp: 1);
        var tilesLimited = strategyLimited.Get(schema, extent, level);

        // assert
        // Default should include levels 0, 1, 2 (21 tiles total: 1 + 4 + 16)
        Assert.That(tilesDefault.Count, Is.EqualTo(21));
        var defaultLevels = tilesDefault.Select(t => t.Index.Level).Distinct().OrderBy(l => l).ToList();
        Assert.That(defaultLevels, Is.EquivalentTo(new[] { 0, 1, 2 }));
        // Limited to maxLevelsUp=1 should include only levels 1, 2 (20 tiles: 4 + 16)
        Assert.That(tilesLimited.Count, Is.EqualTo(20));
        var limitedLevels = tilesLimited.Select(t => t.Index.Level).Distinct().OrderBy(l => l).ToList();
        Assert.That(limitedLevels, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void GetShouldOrderTilesByDistanceFromCenter()
    {
        // arrange
        var schema = new GlobalSphericalMercator();
        var strategy = new DataFetchStrategy(maxLevelsUp: 0);
        const int level = 1;
        var extent = schema.Extent;

        // act
        var tiles = strategy.Get(schema, extent, level).ToList();

        // assert
        var centerX = extent.CenterX;
        var centerY = extent.CenterY;
        for (var i = 0; i < tiles.Count - 1; i++)
        {
            var dist1 = Distance(centerX, centerY, tiles[i].Extent.CenterX, tiles[i].Extent.CenterY);
            var dist2 = Distance(centerX, centerY, tiles[i + 1].Extent.CenterX, tiles[i + 1].Extent.CenterY);
            Assert.That(dist1, Is.LessThanOrEqualTo(dist2));
        }
    }

    [Test]
    public void GetWithAdditionalMarginShouldExpandExtentAndIncludeOriginalTiles()
    {
        // arrange
        var schema = new GlobalSphericalMercator();
        var strategyWithoutMargin = new DataFetchStrategy(maxLevelsUp: 0);
        var strategyWithMargin = new DataFetchStrategy(maxLevelsUp: 0, additionalMarginAsPercentage: 50);
        const int level = 2;
        var fullExtent = schema.Extent;
        var centerExtent = new Extent(
            fullExtent.MinX + fullExtent.Width * 0.25,
            fullExtent.MinY + fullExtent.Height * 0.25,
            fullExtent.MaxX - fullExtent.Width * 0.25,
            fullExtent.MaxY - fullExtent.Height * 0.25);

        // act
        var tilesWithoutMargin = strategyWithoutMargin.Get(schema, centerExtent, level);
        var tilesWithMargin = strategyWithMargin.Get(schema, centerExtent, level);

        // assert
        Assert.That(tilesWithMargin.Count, Is.GreaterThan(tilesWithoutMargin.Count));
        var tilesWithoutMarginSet = tilesWithoutMargin.Select(t => t.Index).ToHashSet();
        var tilesWithMarginSet = tilesWithMargin.Select(t => t.Index).ToHashSet();
        Assert.That(tilesWithMarginSet, Is.SupersetOf(tilesWithoutMarginSet));
    }

    [TestCase(150, Description = "Values above 100 should throw")]
    [TestCase(-10, Description = "Negative values should throw")]
    public void ConstructorShouldThrowForInvalidAdditionalMargin(double invalidMargin)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DataFetchStrategy(additionalMarginAsPercentage: invalidMargin));
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
