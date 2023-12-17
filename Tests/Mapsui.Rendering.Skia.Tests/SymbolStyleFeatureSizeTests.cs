using System;
using System.IO;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class SymbolStyleFeatureSizeTests
{
    [Test]
    public void DefaultSizeFeatureSize()
    {
        var symbolStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Rectangle,
        };

        using var symbolCache = new SymbolCache();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, symbolCache);

        ClassicAssert.AreEqual(size, Math.Max(SymbolStyle.DefaultHeight, SymbolStyle.DefaultWidth) + 1);
    }

    [Test]
    public void DefaultSizeFeatureSize_Scaling()
    {
        var symbolStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Rectangle,
            SymbolScale = 2,
        };

        using var symbolCache = new SymbolCache();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, symbolCache);

        ClassicAssert.AreEqual(size, (Math.Max(SymbolStyle.DefaultHeight, SymbolStyle.DefaultWidth) + 1) * 2);
    }

    [Test]
    public void DefaultSizeFeatureSize_Offset_x()
    {
        var symbolStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Rectangle,
            SymbolOffset = new Offset(2, 0),
        };

        using var symbolCache = new SymbolCache();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, symbolCache);

        ClassicAssert.AreEqual(size, Math.Max(SymbolStyle.DefaultHeight, SymbolStyle.DefaultWidth) + 2 * 2 + 1);
    }

    [Test]
    public void DefaultSizeFeatureSize_Offset_y()
    {
        var symbolStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Rectangle,
            SymbolOffset = new Offset(0, 2),
        };

        using var symbolCache = new SymbolCache();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, symbolCache);

        ClassicAssert.AreEqual(size, Math.Max(SymbolStyle.DefaultHeight, SymbolStyle.DefaultWidth) + 2 * 2 + 1);
    }

    [Test]
    public void DefaultSizeFeatureSize_Offset_x_y()
    {
        var symbolStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Rectangle,
            SymbolOffset = new Offset(2, 2),
        };

        using var symbolCache = new SymbolCache();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, symbolCache);

        ClassicAssert.AreEqual(size, Math.Max(SymbolStyle.DefaultHeight, SymbolStyle.DefaultWidth) + 1 + Math.Sqrt(2 * 2 + 2 * 2) * 2);
    }

    [Test]
    public void BitmapInfoFeatureSize()
    {
        using var symbolCache = new SymbolCache();

        var bitmapId = BitmapRegistry.Instance.Register(CreatePng(100, 100));

        var symbolStyle = new SymbolStyle
        {
            BitmapId = bitmapId,
        };

        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, symbolCache);

        ClassicAssert.AreEqual(size, 100);
    }

    private object CreatePng(int x, int y)
    {
        var imageInfo = new SKImageInfo(x, y);

        using var surface = SKSurface.Create(imageInfo);
        using var image = surface.Snapshot();
        using var data = image.Encode();
        using var memoryStream = new MemoryStream();
        data.SaveTo(memoryStream);
        return memoryStream.ToArray();
    }
}
