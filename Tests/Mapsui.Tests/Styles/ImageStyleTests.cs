using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
internal class ImageStyleTests
{
    [Test]
    public void DefaultConstructor_InitializesWithNullImage()
    {
        var style = new ImageStyle();

        Assert.That(style.Image, Is.Null);
    }

    [Test]
    public void CanSetAndGetImage()
    {
        var image = new Image { Source = "file://test.png" };
        var style = new ImageStyle { Image = image };

        Assert.That(style.Image, Is.SameAs(image));
        Assert.That(style.Image!.Source, Is.EqualTo("file://test.png"));
    }

    [Test]
    public void InheritsBasePointStyleProperties()
    {
        var style = new ImageStyle
        {
            SymbolRotation = 45,
            RotateWithMap = true,
            SymbolScale = 2.0,
            Offset = new Offset(10, 20),
            RelativeOffset = new RelativeOffset(0.1, 0.2),
            SymbolOffsetRotatesWithMap = true
        };

        Assert.That(style.SymbolRotation, Is.EqualTo(45));
        Assert.That(style.RotateWithMap, Is.True);
        Assert.That(style.SymbolScale, Is.EqualTo(2.0));
        Assert.That(style.Offset, Is.EqualTo(new Offset(10, 20)));
        Assert.That(style.RelativeOffset, Is.EqualTo(new RelativeOffset(0.1, 0.2)));
        Assert.That(style.SymbolOffsetRotatesWithMap, Is.True);
    }

    [Test]
    public void InheritsStyleProperties()
    {
        var style = new ImageStyle
        {
            MinVisible = 1.0,
            MaxVisible = 10.0,
            Enabled = false,
            Opacity = 0.5f
        };

        Assert.That(style.MinVisible, Is.EqualTo(1.0));
        Assert.That(style.MaxVisible, Is.EqualTo(10.0));
        Assert.That(style.Enabled, Is.False);
        Assert.That(style.Opacity, Is.EqualTo(0.5f));
    }
}
