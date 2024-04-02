using Mapsui.Extensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Drawing;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class ColorTests
{
    [Test]
    public static void ColorCreator()
    {
        var color = Color.FromArgb(64, 128, 192);

        ClassicAssert.True(color.R == 64);
        ClassicAssert.True(color.G == 128);
        ClassicAssert.True(color.B == 192);
        ClassicAssert.True(color.A == 255);
    }

    [Test]
    public static void ColorFromKnownString()
    {
        var color = Color.LightBlue;
        ClassicAssert.True(color.R == 0xAD && color.G == 0xD8 && color.B == 0xE6);

        color = Color.PaleVioletRed;
        ClassicAssert.True(color.R == 0xDB && color.G == 0x70 && color.B == 0x93);
    }

    [Test]
    public static void ColorFromHtmlString()
    {
        var color = ColorFunctions.FromString("#ADD8E6");
        ClassicAssert.True(color.R == 0xAD && color.G == 0xD8 && color.B == 0xE6);

        color = ColorFunctions.FromString("#ABC");
        ClassicAssert.True(color.R == 0xAA && color.G == 0xBB && color.B == 0xCC);
    }

    [Test]
    public static void ColorFromRgbString()
    {
        var color = ColorFunctions.FromString("rgb(64,128,192)");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192);

        color = ColorFunctions.FromString("RGB(    64,  128,192    )   ");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192);

        color = ColorFunctions.FromString("RGB(    64,  128,192    )   ");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192);

        color = ColorFunctions.FromString("rGb    (    64 ,  128   ,192    )");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192);
    }

    [Test]
    public static void ColorFromRgbaString()
    {
        var color = ColorFunctions.FromString("rgba(64,128,192,0.5)");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);

        color = ColorFunctions.FromString("RGBa(    64,  128,192  ,0.5  )   ");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);

        color = ColorFunctions.FromString("RGBA(    64,  128,192    ,0.5)   ");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);

        color = ColorFunctions.FromString("rGbA    (    64 ,  128   ,192,0.5    )");
        ClassicAssert.True(color.R == 64 && color.G == 128 && color.B == 192 && color.A == 127);
    }

    [Test]
    public static void ColorFromHslString()
    {
        var color = ColorFunctions.FromString("hsl(180.0,50%,50%)");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = ColorFunctions.FromString("HSL(    180,  50,50  %  )   ");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = ColorFunctions.FromString("HsL(    180,  50,50%)   ");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = ColorFunctions.FromString("hSl    (    180. ,  50%   ,50    )");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191);

        color = ColorFunctions.FromString("hSl    (    90.0 ,  25%   ,75    )");
        ClassicAssert.True(color.R == 191 && color.G == 207 && color.B == 175);

        color = ColorFunctions.FromString("hSl    (    270. ,  75%   ,25    )");
        ClassicAssert.True(color.R == 64 && color.G == 16 && color.B == 112);
    }

    [Test]
    public static void ColorFromHslaString()
    {
        var color = ColorFunctions.FromString("hsla(180.0,50%,50%,0.5)");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = ColorFunctions.FromString("HSLA(    180,  50,50  % , 0.5 )   ");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = ColorFunctions.FromString("HsLa(    180,  50,50%,0.5)   ");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = ColorFunctions.FromString("hSlA    (    180. ,  50%   ,50  ,.5  )");
        ClassicAssert.True(color.R == 64 && color.G == 191 && color.B == 191 && color.A == 127);

        color = ColorFunctions.FromString("hSlA    (    90.0 ,  25%   ,75  , 0.5  )");
        ClassicAssert.True(color.R == 191 && color.G == 207 && color.B == 175 && color.A == 127);

        color = ColorFunctions.FromString("hSlA    (    270. ,  75%   ,25 ,0.5   )");
        ClassicAssert.True(color.R == 64 && color.G == 16 && color.B == 112 && color.A == 127);
    }
}
