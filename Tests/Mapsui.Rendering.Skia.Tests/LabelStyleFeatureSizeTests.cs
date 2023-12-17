using System;
using System.IO;
using System.Runtime.InteropServices;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NUnit.Framework.Internal;
using SkiaSharp;
using OSPlatform = System.Runtime.InteropServices.OSPlatform;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class LabelStyleFeatureSizeTests
{
    // The Sizes are different on MacOs and Windows (windows it is 39.6 and macOS it is 42.6)
    public readonly double LabelSize = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 39.6 : 42.6;

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
        using var renderCache = new RenderCache();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, renderCache);

        ClassicAssert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize, 0));
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
        using var renderCache = new RenderCache();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, renderCache);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ClassicAssert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize * 2, 0));
        }
        else
        {
            // on macos it is not two times as big but almost two times with 3 less
            ClassicAssert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize * 2 - 3, 0));
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
        using var renderCache = new RenderCache();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, renderCache);

        ClassicAssert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize + 2 * 2, 0));
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
        using var renderCache = new RenderCache();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, renderCache);

        ClassicAssert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize + 2 * 2, 0));
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
        using var renderCache = new RenderCache();
        var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint, renderCache);

        ClassicAssert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize + Math.Sqrt(2 * 2 + 2 * 2) * 2, 0));
    }
}
