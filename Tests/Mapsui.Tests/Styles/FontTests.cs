using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class FontTests
{
    [Test]
    public static void FontWithNullFontSourceEqualsOtherWithNullFontSource()
    {
        var a = new Font { Size = 12 };
        var b = new Font { Size = 12 };
        Assert.That(a, Is.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public static void FontWithFontSourceNotEqualToFontWithoutFontSource()
    {
        var a = new Font { Size = 12, FontSource = "embedded://Mapsui.Tests.Resources.Fonts.OpenSans-Regular.ttf" };
        var b = new Font { Size = 12 };
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void FontsWithSameFontSourceAreEqual()
    {
        var a = new Font { Size = 12, FontSource = "embedded://Mapsui.Tests.Resources.Fonts.OpenSans-Regular.ttf" };
        var b = new Font { Size = 12, FontSource = "embedded://Mapsui.Tests.Resources.Fonts.OpenSans-Regular.ttf" };
        Assert.That(a, Is.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public static void FontsWithDifferentFontSourceAreNotEqual()
    {
        var a = new Font { Size = 12, FontSource = "embedded://Mapsui.Tests.Resources.Fonts.OpenSans-Regular.ttf" };
        var b = new Font { Size = 12, FontSource = "file:///some/other/font.ttf" };
        Assert.That(a, Is.Not.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }

    [Test]
    public static void CopyConstructorCopiesFontSource()
    {
        FontSource source = "embedded://Mapsui.Tests.Resources.Fonts.OpenSans-Regular.ttf";
        var original = new Font { Size = 14, Bold = true, FontSource = source };
        var copy = new Font(original);
        Assert.That(copy.FontSource, Is.EqualTo(original.FontSource));
        Assert.That(copy, Is.EqualTo(original));
    }
}
