// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs;
using Mapsui.Providers.Wms;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.Animations;
using Mapsui.Samples.Common.Maps.DataFormats;
using Mapsui.Samples.Common.Maps.Special;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Samples.CustomWidget;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Widgets.InfoWidgets;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class MapRegressionTests
{
    static MapRegressionTests()
    {
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();
    }

    private static ISampleBase[]? _excludedSamples;
    private static ISampleBase[]? _regressionSamples;

    public MapRegressionTests()
    {
        // Tile Cache
        OpenStreetMap.DefaultCache ??= File.ReadFromCacheFolder("OpenStreetMap");
        BingArial.DefaultCache ??= File.ReadFromCacheFolder("BingArial");
        BingHybrid.DefaultCache ??= File.ReadFromCacheFolder("BingHybrid");
        Michelin.DefaultCache ??= File.ReadFromCacheFolder("Michelin");
        TiledWmsSample.DefaultCache ??= File.ReadFromCacheFolder("TiledWmsSample");
        TmsSample.DefaultCache ??= File.ReadFromCacheFolder("TmsSample");
        WmtsSample.DefaultCache ??= File.ReadFromCacheFolder("WmtsSample");

        // Url Cache
        WmsProvider.DefaultCache ??= File.ReadFromCacheFolder("WmsSample");
        WFSProvider.DefaultCache ??= File.ReadFromCacheFolder("WfsSample");
        ArcGISImageServiceSample.DefaultCache ??= File.ReadFromCacheFolder("ArcGisImageServiceSample");
    }

    public static object[] RegressionSamples => _regressionSamples ??=
    [
        .. AllSamples.GetSamples().Where(f => ExcludedSamples
            .All(e => e.GetType() != f.GetType())).OrderBy(f => f.GetType().FullName),
    ];

    public static object[] ExcludedSamples => _excludedSamples ??= [new AnimatedPointsSample(), new MutatingTriangleSample()];

    [Test]
    [Retry(5)]
    [TestCaseSource(nameof(RegressionSamples))]
    public async Task TestSampleAsync(ISampleBase sample)
    {
        var original = Logger.LogDelegate;
        try
        {
            Logger.LogDelegate = ConsoleLog;
            // At the moment of writing this comment we do not have logging in the map. To compare
            // images we disable it for now. Perhaps we want logging to be part of the test image in some cases.
            LoggingWidget.ShowLoggingInMap = ShowLoggingInMap.Never;
            ConsoleLog(LogLevel.Debug, $"Start MapRegressionTest {sample.GetType().Name}", null);
            await TestSampleAsync(sample, true).ConfigureAwait(false);
        }
        finally
        {
            ConsoleLog(LogLevel.Debug, $"End MapRegressionTest {sample.GetType().Name}", null);
            Logger.LogDelegate = original;
        }
    }

    private void ConsoleLog(LogLevel arg1, string arg2, Exception? arg3)
    {
        var message = $@"LogLevel: {arg1} Message: {arg2}";
        if (arg3 != null)
        {
            message += $@" Exception: {arg3}";
        }

        Console.WriteLine(message);
        Console.Out.Flush();
    }

    public static async Task TestSampleAsync(ISampleBase sample, bool compareImages)
    {
        try
        {
            var fileName = sample.GetType().Name + ".Regression.png";
            using var mapControl = await InitMapAsync(sample).ConfigureAwait(true);
            var map = mapControl.Map;
            await DisplayMapAsync(mapControl).ConfigureAwait(false);

            if (map != null)
            {
                // act
                using var mapRenderer = CreateMapRenderer(mapControl);
                using var bitmap = mapRenderer.RenderToBitmapStream(mapControl.Map.Navigator.Viewport, map.Layers, map.BackColor, 2, map.GetWidgetsOfMapAndLayers());

                // aside
                if (bitmap is { Length: > 0 })
                {
                    File.WriteToGeneratedRegressionFolder(fileName, bitmap);
                }
                else
                {
                    Assert.Fail("Should generate Image");
                }

                // assert
                if (compareImages)
                {
                    using var originalStream = File.ReadFromOriginalRegressionFolder(fileName);
                    if (originalStream == null)
                    {
                        Assert.Inconclusive($"No Regression Test Data for {sample.Name}");
                    }
                    else
                    {
                        ClassicAssert.IsTrue(MapRendererTests.CompareBitmaps(originalStream, bitmap, 1, 0.995));
                    }
                }
                else
                {
                    // Don't compare images here because to unreliable
                    ClassicAssert.True(true);
                }
            }

            map?.Layers.Clear();
        }
        finally
        {
            if (sample is IDisposable disposable)
            {
#pragma warning disable IDISP007 // Don't dispose injected
                disposable.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected
            }
        }
    }

    private static MapRenderer CreateMapRenderer(IMapControl mapControl)
    {
        var mapRenderer = new MapRenderer
        {
            WidgetRenders =
            {
                [typeof(CustomWidget)] = new CustomWidgetSkiaRenderer(),
            }
        };
        foreach (var widgetRender in mapControl.Renderer.WidgetRenders)
        {
            if (!mapRenderer.WidgetRenders.Contains(widgetRender))
            {
                mapRenderer.WidgetRenders[widgetRender.Key] = widgetRender.Value;
            }
        }
        return mapRenderer;
    }

    [Test]
    [Explicit]
    [TestCaseSource(nameof(ExcludedSamples))]
    public async Task ExcludedTestSampleAsync(ISampleBase sample)
    {
        await TestSampleAsync(sample, false);
    }

    private static async Task<RegressionMapControl> InitMapAsync(ISampleBase sample)
    {
        var mapControl = new RegressionMapControl();

        mapControl.SetSize(800, 600);

        if (sample is IPrepareSampleTest prepareTest)
        {
            prepareTest.PrepareTest();
        }

        await sample.SetupAsync(mapControl);
        await mapControl.WaitForLoadingAsync();

        if (sample is ISampleTest sampleTest)
        {
            await sampleTest.InitializeTestAsync(mapControl).ConfigureAwait(true);
        }

        await mapControl.WaitForLoadingAsync();
        var fetchInfo = new FetchInfo(mapControl.Map.Navigator.Viewport.ToSection(), mapControl.Map.CRS);
        mapControl.Map.RefreshData(fetchInfo);

        return mapControl;
    }

    private static async Task DisplayMapAsync(IMapControl mapControl)
    {
        await mapControl.WaitForLoadingAsync().ConfigureAwait(false);

        // wait for rendering to finish to make the Tests more reliable
        await Task.Delay(300).ConfigureAwait(false);
    }
}
