using System.Collections.Generic;
using Mapsui.Experimental.VectorTiles.VexTileCopies;
using NUnit.Framework;
using VexTile.Common.Data;
using VexTile.Common.Sources;

namespace Mapsui.Experimental.VectorTiles.Tests.VexTileCopies;

[TestFixture]
public class VectorTilesSourceTests
{
    // Dutch uses ',' as decimal separator and '.' as group separator, so a culture-sensitive parse
    // turned "-180.0" into -1800.
    [Test, SetCulture("nl-NL")]
    public void LoadMetadata_UnderCommaDecimalCulture_ParsesNumbersInvariantly()
    {
        // Arrange
        using var dataSource = new FakeTileDataSource(
            new MetaData { Name = "bounds", Value = "-180.0,-85.0511,180.0,85.0511" },
            new MetaData { Name = "center", Value = "4.5,52.25" },
            new MetaData { Name = "minzoom", Value = "0" },
            new MetaData { Name = "maxzoom", Value = "14" });

        // Act
        var source = new VectorTilesSource(dataSource);

        // Assert
        Assert.That(source.Bounds, Is.Not.Null);
        Assert.That(source.Bounds!.West, Is.EqualTo(-180.0));
        Assert.That(source.Bounds.South, Is.EqualTo(-85.0511));
        Assert.That(source.Bounds.East, Is.EqualTo(180.0));
        Assert.That(source.Bounds.North, Is.EqualTo(85.0511));
        Assert.That(source.Center, Is.Not.Null);
        Assert.That(source.Center!.X, Is.EqualTo(4.5));
        Assert.That(source.Center.Y, Is.EqualTo(52.25));
        Assert.That(source.MinZoom, Is.EqualTo(0));
        Assert.That(source.MaxZoom, Is.EqualTo(14));
    }

    private sealed class MetaData : IMetaData
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    private sealed class FakeTileDataSource(params IMetaData[] metaData) : ITileDataSource
    {
        public IEnumerable<IMetaData> GetMetaData() => metaData;
        public ITile GetTile(int x, int y, int zoom) => null!;
        public void Dispose() { }
    }
}
