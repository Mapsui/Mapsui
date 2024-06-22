// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.Tests.Helpers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Animations;
using Mapsui.Samples.Common.Maps.DataFormats;
using Mapsui.Samples.Common.Maps.Geometries;
using Mapsui.Samples.Common.Maps.Special;
using Mapsui.Samples.Common.Maps.Widgets;
using Mapsui.UI;
using Mapsui.Widgets.InfoWidgets;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class MapLiveTests
{
    static MapLiveTests()
    {
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();
    }

    private static ISampleBase[]? _excludedSamples;
    private static ISampleBase[]? _liveSamples;

    public MapLiveTests()
    {
        CacheHelper.NullifyCaches();
    }

    public static object[] LiveSamples => _liveSamples ??=
    [
        .. AllSamples.GetSamples().Where(f => ExcludedSamples
            .All(e => e.GetType() != f.GetType())).OrderBy(f => f.GetType().FullName),
    ];

    public static object[] ExcludedSamples =>
        _excludedSamples ??= [new AnimatedPointsSample(), new MutatingTriangleSample(), new ArcGISDynamicServiceSample(), new ManyMutatingLayersSample()];

    [Test]
    [TestCaseSource(nameof(LiveSamples))]
    [Explicit]
    [Category("LiveSample")]
    public async Task TestLiveSampleAsync(ISampleBase sample)
    {
        var original = Logger.LogDelegate;
        try
        {
            Logger.LogDelegate = ConsoleLog;
            // At the moment of writing this comment we do not have logging in the map. To compare
            // images we disable it for now. Perhaps we want logging to be part of the test image in some cases.
            LoggingWidget.ShowLoggingInMap = ShowLoggingInMap.Never;
            ConsoleLog(LogLevel.Debug, $"Start MapLiveTest {sample.GetType().Name}", null);
            await TestSampleAsync(sample).ConfigureAwait(false);
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

    public static async Task TestSampleAsync(ISampleBase sample)
    {
        try
        {
            var fileName = sample.GetType().Name + ".Regression.png";
            using var mapControl = await SampleHelper.InitMapAsync(sample).ConfigureAwait(false);
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
            }
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
        await TestSampleAsync(sample);
    }


    private static async Task DisplayMapAsync(IMapControl mapControl)
    {
        await mapControl.WaitForLoadingAsync().ConfigureAwait(false);

        // wait for rendering to finish to make the Tests more reliable
        await Task.Delay(300).ConfigureAwait(false);
    }
}
