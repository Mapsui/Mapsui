using Mapsui.Experimental.VectorTiles.VexTileCopies;
using NUnit.Framework;

namespace Mapsui.Experimental.VectorTiles.Tests.VexTileCopies;

[TestFixture]
public class VectorStyleFilterTests
{
    [Test]
    public void ValuesEqual_NumbersOfDifferentTypes_CompareByValue()
    {
        // Tile integers decode as long or ulong, style JSON numbers as long or double.
        Assert.That(VectorStyle.ValuesEqual(2L, 2.0), Is.True);
        Assert.That(VectorStyle.ValuesEqual(4UL, 4L), Is.True);
        Assert.That(VectorStyle.ValuesEqual(2L, 3.0), Is.False);
    }

    [Test]
    public void ValuesEqual_TextAndNumber_AreDifferent()
    {
        Assert.That(VectorStyle.ValuesEqual("motorway", "motorway"), Is.True);
        Assert.That(VectorStyle.ValuesEqual("2", 2L), Is.False);
    }
}
