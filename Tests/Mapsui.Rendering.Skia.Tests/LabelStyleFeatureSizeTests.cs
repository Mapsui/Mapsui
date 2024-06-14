using System;
using System.Runtime.InteropServices;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using Mapsui.Utilities;
using NUnit.Framework;
using SkiaSharp;
using OSPlatform = System.Runtime.InteropServices.OSPlatform;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class LabelStyleFeatureSizeTests
{
    // The Sizes are different on MacOs and Windows (windows it is 39.6 and macOS it is 42.6)
    const double labelSizeOnMac = 42.642578125d;
    const double labelSizeOnWindows = 39.642578125d;
    const double labelSizeOnLinux = 40.181640625d;
    public readonly double LabelSize = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? labelSizeOnWindows
        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? labelSizeOnLinux
            : labelSizeOnMac;



    [Test]
    public void DefaultSizeFeatureSize()
    {
        var labelStyle = new LabelStyle
        {
            LabelColumn = "test",
        };

        var feature = new PointFeature(new MPoint(0, 0));
        feature["test"] = "Mapsui";

        using var skPaint = new SKPaint();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, new LabelCache());

        Assert.That(size, Is.EqualTo(LabelSize).Within(Constants.Epsilon));
    }

    [Test]
    public void DefaultSizeFeatureSize_Font()
    {
        var labelStyle = new LabelStyle
        {
            LabelColumn = "test",
        };

        labelStyle.Font.Size *= 2;

        var feature = new PointFeature(new MPoint(0, 0));
        feature["test"] = "Mapsui";

        using var skPaint = new SKPaint();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, new LabelCache());

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.That(size, Is.EqualTo(LabelSize * 2).Within(Constants.Epsilon));
        }
        else
        {
            // on macos it is not two times as big but almost two times with 3 less
            Assert.That(size, Is.EqualTo(LabelSize * 2 - 3).Within(Constants.Epsilon));
        }
    }

    [Test]
    public void DefaultSizeFeatureSize_Offset_x()
    {
        var labelStyle = new LabelStyle
        {
            LabelColumn = "test",
            Offset = new Offset(2, 0),
        };

        var feature = new PointFeature(new MPoint(0, 0));
        feature["test"] = "Mapsui";

        using var skPaint = new SKPaint();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, new LabelCache());

        Assert.That(size, Is.EqualTo(LabelSize + 2 * 2).Within(Constants.Epsilon));
    }

    [Test]
    public void DefaultSizeFeatureSize_Offset_y()
    {
        var labelStyle = new LabelStyle
        {
            LabelColumn = "test",
            Offset = new Offset(0, 2),
        };

        var feature = new PointFeature(new MPoint(0, 0));
        feature["test"] = "Mapsui";

        using var skPaint = new SKPaint();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, new LabelCache());

        Assert.That(size, Is.EqualTo(LabelSize + 2 * 2).Within(Constants.Epsilon));
    }

    [Test]
    public void DefaultSizeFeatureSize_Offset_x_y()
    {
        var labelStyle = new LabelStyle
        {
            LabelColumn = "test",
            Offset = new Offset(2, 2),
        };

        var feature = new PointFeature(new MPoint(0, 0));
        feature["test"] = "Mapsui";

        using var skPaint = new SKPaint();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, new LabelCache());

        Assert.That(size, Is.EqualTo(LabelSize + Math.Sqrt(2 * 2 + 2 * 2) * 2).Within(Constants.Epsilon));
    }
}
