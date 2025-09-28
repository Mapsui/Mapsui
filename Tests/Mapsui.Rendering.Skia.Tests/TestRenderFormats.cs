using System.Threading;
using Mapsui.Rendering.Skia.Tests.Utilities;
using Mapsui.Samples.Common.Maps.Tests;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class TestRenderFormats
{
    [Test]
    [Retry(5)]
    public void TestSameOutputPngAndJpeg()
    {
        // Arrange
        using var map = SymbolTypesSample.CreateMap();
        map.Navigator.SetSize(800, 600);
        //await SampleHelper.DisplayMapAsync(mapControl).ConfigureAwait(false);
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmapPng = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers,
            map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers());
        using var bitmapJpeg = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers,
            map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg, 90);

        // Assert
        if (BitmapComparer.Compare(bitmapPng, bitmapJpeg, 1, 0.995))
        {
            Assert.Pass("Png and Jpeg are the same");
        }
        else
        {
            Assert.Pass("Png and Jpeg are too different");
        }
    }

    [Test]
    [Retry(5)]
    public void TestSameOutputJpegGetsSmallerWithLessQuality()
    {
        // Arrange
        using var map = SymbolTypesSample.CreateMap();
        map.Navigator.SetSize(800, 600);
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmapJpegLarge = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers,
            map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg);
        using var bitmapJpegSmall = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers,
            map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg, 80);

        // Assert
        Assert.That(bitmapJpegSmall.Length < bitmapJpegLarge.Length);
    }

    [Test]
    [Retry(5)]
    public void TestSameOutputWebPGetsSmallerWithLessQuality()
    {
        // Arrange
        using var map = SymbolTypesSample.CreateMap();
        map.Navigator.SetSize(800, 600);
        var mapRenderer = new MapRenderer();


        // Act
        using var bitmapWebPLarge = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers,
            map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.WebP);
        using var bitmapWebPSmall = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers,
            map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.WebP, 80);

        // Assert
        Assert.That(bitmapWebPSmall.Length < bitmapWebPLarge.Length);
    }

    [Test]
    [Retry(5)]
    public void TestSameOutputWebPIsSmallerThanJpeg()
    {
        // Arrange
        using var map = SymbolTypesSample.CreateMap();
        map.Navigator.SetSize(800, 600);
        var mapRenderer = new MapRenderer();


        // Act
        using var jpeg = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers, map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg, 80);
        using var webP = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers, map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.WebP, 80);

        // Assert
        Assert.That(webP.Length < jpeg.Length);
    }
}
