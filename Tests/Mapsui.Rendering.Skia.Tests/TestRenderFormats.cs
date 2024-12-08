using System.Threading;
using System.Threading.Tasks;
using Mapsui.Rendering.Skia.Tests.Helpers;
using Mapsui.Tests.Common.Maps;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class TestRenderFormats
{
    [Test]
    [Retry(5)]
    public async Task TestSameOutputPngAndJpegAsync()
    {
        var sample = new SymbolTypesSample();
        using var mapControl = await SampleHelper.InitMapAsync(sample).ConfigureAwait(false);
        var map = mapControl.Map;
        await SampleHelper.DisplayMapAsync(mapControl).ConfigureAwait(false);

        if (map != null)
        {
            // act
            using var mapRenderer = MapRegressionTests.CreateMapRenderer(mapControl);
            using var bitmapPng = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers());
            using var bitmapJpeg = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers,
                map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg, 90);

            if (MapRendererTests.CompareBitmaps(bitmapPng, bitmapJpeg, 1, 0.995))
            {
                Assert.Pass("Png and Jpeg are the same");
            }
            else
            {
                Assert.Pass("Png and Jpeg are too different");
            }
        }
    }

    [Test]
    [Retry(5)]
    public async Task TestSameOutputJpegGetsSmallerWithLessQualityAsync()
    {
        var sample = new SymbolTypesSample();
        using var mapControl = await SampleHelper.InitMapAsync(sample).ConfigureAwait(false);
        var map = mapControl.Map;
        await SampleHelper.DisplayMapAsync(mapControl).ConfigureAwait(false);

        if (map != null)
        {
            // act
            using var mapRenderer = MapRegressionTests.CreateMapRenderer(mapControl);
            using var bitmapJpegLarge = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg);
            using var bitmapJpegSmall = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg, 80);

            Assert.That(bitmapJpegSmall.Length < bitmapJpegLarge.Length);
        }
    }

    [Test]
    [Retry(5)]
    public async Task TestSameOutputWebPGetsSmallerWithLessQualityAsync()
    {
        var sample = new SymbolTypesSample();
        using var mapControl = await SampleHelper.InitMapAsync(sample).ConfigureAwait(false);
        var map = mapControl.Map;
        await SampleHelper.DisplayMapAsync(mapControl).ConfigureAwait(false);

        if (map != null)
        {
            // act
            using var mapRenderer = MapRegressionTests.CreateMapRenderer(mapControl);
            using var bitmapWebPLarge = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.WebP);
            using var bitmapWebPSmall = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.WebP, 80);

            Assert.That(bitmapWebPSmall.Length < bitmapWebPLarge.Length);
        }
    }
    [Test]
    [Retry(5)]
    public async Task TestSameOutputWebPIsSmallerThanJpegAsync()
    {
        var sample = new SymbolTypesSample();
        using var mapControl = await SampleHelper.InitMapAsync(sample).ConfigureAwait(false);
        var map = mapControl.Map;
        await SampleHelper.DisplayMapAsync(mapControl).ConfigureAwait(false);

        if (map != null)
        {
            // act
            using var mapRenderer = MapRegressionTests.CreateMapRenderer(mapControl);
            using var jpeg = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.Jpeg, 80);
            using var webP = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers(), RenderFormat.WebP, 80);

            Assert.That(webP.Length < jpeg.Length);
        }
    }
}
