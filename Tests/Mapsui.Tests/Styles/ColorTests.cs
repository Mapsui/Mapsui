using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class ColorTests
{
    [Test]
    public static void ColorCreator()
    {
        var color = new Color(64, 128, 192);

        Assert.That(color.R == 64, Is.True);
        Assert.That(color.G == 128, Is.True);
        Assert.That(color.B == 192, Is.True);
        Assert.That(color.A == 255, Is.True);
    }

    [Test]
    public static void ColorFromKnownString()
    {
        var color = Color.LightBlue;
        Assert.That(color.R == 0xAD && color.G == 0xD8 && color.B == 0xE6, Is.True);

        color = Color.PaleVioletRed;
        Assert.That(color.R == 0xDB && color.G == 0x70 && color.B == 0x93, Is.True);
    }

    [Test]
    public static void ColorFromHtmlString()
    {
        var color = Color.FromString("#ADD8E6");
        Assert.That(color.R == 0xAD && color.G == 0xD8 && color.B == 0xE6, Is.True);

        color = Color.FromString("#ABC");
        Assert.That(color.R == 0xAA && color.G == 0xBB && color.B == 0xCC, Is.True);
    }

    [Test]
    public static void ColorFromKnownColor()
    {
        var color = Color.FromString("Magenta");
        Assert.That(color.R == Color.Magenta.R && color.G == Color.Magenta.G && color.B == Color.Magenta.B, Is.True);

        color = Color.FromString("Yellow");
        Assert.That(color.R == Color.Yellow.R && color.G == Color.Yellow.G && color.B == Color.Yellow.B, Is.True);
    }

    [Test]
    public static void ColorFromRgbString()
    {
        var color = Color.FromString("rgb(64,128,192)");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192, Is.True);

        color = Color.FromString("RGB(    64,  128,192    )   ");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192, Is.True);

        color = Color.FromString("RGB(    64,  128,192    )   ");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192, Is.True);

        color = Color.FromString("rGb    (    64 ,  128   ,192    )");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192, Is.True);
    }

    [Test]
    public static void ColorFromRgbaString()
    {
        var color = Color.FromString("rgba(64,128,192,0.5)");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127, Is.True);

        color = Color.FromString("RGBa(    64,  128,192  ,0.5  )   ");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127, Is.True);

        color = Color.FromString("RGBA(    64,  128,192    ,0.5)   ");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127, Is.True);

        color = Color.FromString("rGbA    (    64 ,  128   ,192,0.5    )");
        Assert.That(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127, Is.True);
    }

    [Test]
    public static void ColorFromHslString()
    {
        var color = Color.FromString("hsl(180.0,50%,50%)");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191, Is.True);

        color = Color.FromString("HSL(    180,  50,50  %  )   ");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191, Is.True);

        color = Color.FromString("HsL(    180,  50,50%)   ");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191, Is.True);

        color = Color.FromString("hSl    (    180. ,  50%   ,50    )");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191, Is.True);

        color = Color.FromString("hSl    (    90.0 ,  25%   ,75    )");
        Assert.That(color.R == 191 && color.G == 207 && color.B == 175, Is.True);

        color = Color.FromString("hSl    (    270. ,  75%   ,25    )");
        Assert.That(color.R == 64 && color.G == 16 && color.B == 112, Is.True);
    }

    [Test]
    public static void ColorFromHslaString()
    {
        var color = Color.FromString("hsla(180.0,50%,50%,0.5)");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127, Is.True);

        color = Color.FromString("HSLA(    180,  50,50  % , 0.5 )   ");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127, Is.True);

        color = Color.FromString("HsLa(    180,  50,50%,0.5)   ");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127, Is.True);

        color = Color.FromString("hSlA    (    180. ,  50%   ,50  ,.5  )");
        Assert.That(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127, Is.True);

        color = Color.FromString("hSlA    (    90.0 ,  25%   ,75  , 0.5  )");
        Assert.That(color.R == 191 && color.G == 207 && color.B == 175 && color.A == 127, Is.True);

        color = Color.FromString("hSlA    (    270. ,  75%   ,25 ,0.5   )");
        Assert.That(color.R == 64 && color.G == 16 && color.B == 112 && color.A == 127, Is.True);
    }

    [Test]
    public static void CastToSystemDrawingColorTest()
    {
        // Arrange
        var color = new Color(63, 127, 191, 255);
        var expectedSystemDrawingColor = System.Drawing.Color.FromArgb(255, 63, 127, 191);

        // Act
        var systemDrawingColor = (System.Drawing.Color)color;

        // Assert
        Assert.That(systemDrawingColor, Is.EqualTo(expectedSystemDrawingColor));
    }

    [Test]
    public static void CastFromSystemDrawingColorTest()
    {
        // Arrange
        var systemDrawingColor = System.Drawing.Color.FromArgb(255, 63, 127, 191);
        var expectedColor = new Color(63, 127, 191, 255);

        // Act
        var color = (Color)systemDrawingColor;

        // Assert
        Assert.That(color, Is.EqualTo(expectedColor));
    }
}
