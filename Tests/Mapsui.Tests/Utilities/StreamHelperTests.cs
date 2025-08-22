using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Tests;
using NUnit.Framework;
namespace Mapsui.Tests.Utilities;

[TestFixture]
public class StreamHelperTests
{
    [Test]
    public void StreamIsSvgPin()
    {
        // act
        using var stream = File.ReadFromImagesFolder("Pin.svg");

        // assert
        Assert.That(stream.IsSvg(), Is.True);
    }

    [Test]
    public void StreamIsSvgPinXml()
    {
        // act
        using var stream = File.ReadFromImagesFolder("PinXml.svg");

        // assert
        Assert.That(stream.IsSvg(), Is.True);
    }

    [Test]
    public void StreamIsSvgVectorSymbolUnittype()
    {
        // act
        using var stream = File.ReadFromImagesFolder("vector_symbol_unittype.png");

        // assert
        Assert.That(stream.IsSvg(), Is.False);
    }
}
