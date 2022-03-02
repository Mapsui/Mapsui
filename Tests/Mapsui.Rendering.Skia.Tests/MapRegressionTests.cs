// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.Tiling;
using Mapsui.UI;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class MapRegressionTests
{
    public MapRegressionTests()
    {
        OpenStreetMap.DefaultCache ??= File.ReadFromCacheFolder("OpenStreetMap");
        BingArial.DefaultCache ??= File.ReadFromCacheFolder("BingArial");
        BingHybrid.DefaultCache ??= File.ReadFromCacheFolder("BingHybrid");
        Michelin.DefaultCache ??= File.ReadFromCacheFolder("Michelin");
    }

    public static object[] RegressionSamples => AllSamples.GetSamples().ToArray();

    [Test]
    [TestCaseSource(nameof(RegressionSamples))]
    public async Task TestSample(ISample sample)
    {
        var fileName = sample.GetType().Name + ".Regression.png";
        var mapControl = InitMap(sample);
        var map = mapControl.Map;
        await DisplayMap(mapControl).ConfigureAwait(false);

        if (map != null)
        {
            // act
            using var bitmap = new MapRenderer().RenderToBitmapStream(mapControl.Viewport, map.Layers, map.BackColor, 2);

            // aside
            if (bitmap is { Length: > 0 })
            {
                File.WriteToGeneratedFolder(fileName, bitmap);
            }
            else
            {
                Assert.Fail("Should generate Image");
            }
            

            // assert
            Assert.IsTrue(MapRendererTests.CompareBitmaps(File.ReadFromRegressionFolder(fileName), bitmap, 1, 0.99));
        }
    }

    private static RegressionMapControl InitMap(ISample sample)
    {
        var mapControl = new RegressionMapControl();
        mapControl.SetSize(800, 600);
        sample.Setup(mapControl);
        return mapControl;
    }

    private async Task DisplayMap(IMapControl mapControl)
    {
        var fetchInfo = new FetchInfo(mapControl.Viewport.Extent, mapControl.Viewport.Resolution, mapControl.Map?.CRS);
        mapControl.Map?.RefreshData(fetchInfo);
        await WaitForLoading(mapControl).ConfigureAwait(false);
    }

    private async Task WaitForLoading(IMapControl mapControl)
    {
        if (mapControl.Map?.Layers != null)
        {
            foreach (var layer in mapControl.Map.Layers)
            {
                await WaitForLoading(layer).ConfigureAwait(false);
            }
        }
    }

    private async Task WaitForLoading(ILayer layer)
    {
        while (layer.Busy)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }
    }
}