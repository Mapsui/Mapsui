// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Data;
using Mapsui.Samples.Common.Maps.Navigation;
using Mapsui.Samples.Common.Maps.Projection;
using Mapsui.Samples.Common.Maps.Special;
using Mapsui.Tiling;
using Mapsui.UI;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class MapRegressionTests
{
    private static ISample[]? _excludedSamples;
    private static ISample[]? _regressionSamples;

    public MapRegressionTests()
    {
        OpenStreetMap.DefaultCache ??= File.ReadFromCacheFolder("OpenStreetMap");
        BingArial.DefaultCache ??= File.ReadFromCacheFolder("BingArial");
        BingHybrid.DefaultCache ??= File.ReadFromCacheFolder("BingHybrid");
        Michelin.DefaultCache ??= File.ReadFromCacheFolder("Michelin");
    }

    public static object[] RegressionSamples => _regressionSamples ??= AllSamples.GetSamples().Where(f => ExcludedSamples.All(e => e.GetType() != f.GetType())).OrderBy(f => f.GetType().FullName).ToArray();

    public static object[] ExcludedSamples => _excludedSamples ??= new ISample[] {
        new PanLockSample(), 
        new PenStrokeCapSample(), 
        new WfsSample(),
        new PolygonSample(),
        new PointProjectionSample(),
        new RasterizingTileLayerSample(),
        new PointFeatureAnimationSamples(),
        new StackedLabelsSample(),
        new MutatingTriangleSample(), // Causes Synchronization Context Errors
    };

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

    [Test]
    [Ignore("Don't work currently")]
    [TestCaseSource(nameof(ExcludedSamples))]
    public async Task ExcludedTestSample(Type sample)
    {
        await TestSample((ISample)Activator.CreateInstance(sample));
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