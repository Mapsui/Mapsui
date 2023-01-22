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

        Assert.True(color.R == 64);
        Assert.True(color.G == 128);
        Assert.True(color.B == 192);
        Assert.True(color.A == 255);
    }

    [Test]
    public static void ColorFromKnownString()
    {
        var color = Color.FromString("LightBlue");
        Assert.True(color.R == 0xAD && color.G == 0xD8 && color.B == 0xE6);

        color = Color.FromString("PaleVioletRed");
        Assert.True(color.R == 0xDB && color.G == 0x70 && color.B == 0x93);
    }

    [Test]
    public static void ColorFromHtmlString()
    {
        var color = Color.FromString("#ADD8E6");
        Assert.True(color.R == 0xAD && color.G == 0xD8 && color.B == 0xE6);

        color = Color.FromString("#ABC");
        Assert.True(color.R == 0xAA && color.G == 0xBB && color.B == 0xCC);
    }

    [Test]
    public static void ColorFromRgbString()
    {
        var color = Color.FromString("rgb(64,128,192)");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192);

        color = Color.FromString("RGB(    64,  128,192    )   ");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192);

        color = Color.FromString("RGB(    64,  128,192    )   ");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192);

        color = Color.FromString("rGb    (    64 ,  128   ,192    )");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192);
    }

    [Test]
    public static void ColorFromRgbaString()
    {
        var color = Color.FromString("rgba(64,128,192,0.5)");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);

        color = Color.FromString("RGBa(    64,  128,192  ,0.5  )   ");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);

        color = Color.FromString("RGBA(    64,  128,192    ,0.5)   ");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);

        color = Color.FromString("rGbA    (    64 ,  128   ,192,0.5    )");
        Assert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);
    }

    [Test]
    public static void ColorFromHslString()
    {
        var color = Color.FromString("hsl(180.0,50%,50%)");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = Color.FromString("HSL(    180,  50,50  %  )   ");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = Color.FromString("HsL(    180,  50,50%)   ");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = Color.FromString("hSl    (    180. ,  50%   ,50    )");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = Color.FromString("hSl    (    90.0 ,  25%   ,75    )");
        Assert.True(color.R == 191 && color.G == 207 && color.B == 175);

        color = Color.FromString("hSl    (    270. ,  75%   ,25    )");
        Assert.True(color.R == 64 && color.G == 16 && color.B == 112);
    }

    [Test]
    public static void ColorFromHslaString()
    {
        var color = Color.FromString("hsla(180.0,50%,50%,0.5)");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = Color.FromString("HSLA(    180,  50,50  % , 0.5 )   ");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = Color.FromString("HsLa(    180,  50,50%,0.5)   ");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = Color.FromString("hSlA    (    180. ,  50%   ,50  ,.5  )");
        Assert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = Color.FromString("hSlA    (    90.0 ,  25%   ,75  , 0.5  )");
        Assert.True(color.R == 191 && color.G == 207 && color.B == 175 && color.A == 127);

        color = Color.FromString("hSlA    (    270. ,  75%   ,25 ,0.5   )");
        Assert.True(color.R == 64 && color.G == 16 && color.B == 112 && color.A == 127);
    }
}
