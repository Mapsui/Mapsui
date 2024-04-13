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

        using var renderService = new RenderService();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, renderService);

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

        using var renderService = new RenderService();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, renderService);

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

        using var renderService = new RenderService();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, renderService);

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

        using var renderService = new RenderService();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, renderService);

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

        using var renderService = new RenderService();
        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, renderService);

        ClassicAssert.AreEqual(size, Math.Max(SymbolStyle.DefaultHeight, SymbolStyle.DefaultWidth) + 1 + Math.Sqrt(2 * 2 + 2 * 2) * 2);
    }

    [Test]
    public void BitmapInfoFeatureSize()
    {
        using var renderService = new RenderService();

        var bitmapId = BitmapRegistry.Instance.Register(CreatePng(100, 100));

        var symbolStyle = new SymbolStyle
        {
            BitmapId = bitmapId,
        };

        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, renderService);

        ClassicAssert.AreEqual(size, 100);
    }

    [Test]
    public void BitmapInfoFeatureSizeFromBitmap()
    {
        using var renderService = new RenderService();

        var bitmap = CreatePng(100, 100);
        var bitmapId = renderService.BitmapRegistry.Register(bitmap);

        var symbolStyle = new SymbolStyle
        {
            BitmapId = bitmapId,
        };

        var size = SymbolStyleRenderer.FeatureSize(symbolStyle, renderService);

        ClassicAssert.AreEqual(size, 100);
    }

    [Test]
    public void UriTests()
    {
        var file = new Uri("file://myfolder/myimage.png");
        var http = new Uri("http://mywebsite.com/myimage.png");
        var resource = new Uri("embeddedResource://myassembly.resourses.images.myimage.png");
        Assert.That(file.Scheme == "file");
        Assert.That(http.Scheme == "http");
        Assert.That(resource.Scheme == "embeddedresource");
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
