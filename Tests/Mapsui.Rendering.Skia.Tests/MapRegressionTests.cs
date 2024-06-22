// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.Tests.Helpers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Animations;
using Mapsui.Samples.Common.Maps.DataFormats;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Samples.Common.Maps.Geometries;
using Mapsui.Samples.Common.Maps.Info;
using Mapsui.Samples.Common.Maps.Special;
using Mapsui.Samples.Common.Maps.Widgets;
using Mapsui.Styles;
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
        CacheHelper.InitCaches();
    }

    public static object[] RegressionSamples => _regressionSamples ??=
    [
        .. AllSamples.GetSamples().Where(f => ExcludedSamples
            .All(e => e.GetType() != f.GetType())).OrderBy(f => f.GetType().FullName),
    ];

    public static object[] ExcludedSamples =>
        _excludedSamples ??=
        [
            new AnimatedPointsSample(), // We have no reliable way yet to compare animations.
            new MutatingTriangleSample(), // We have no reliable way yet to compare animations.
            new ManyMutatingLayersSample(), // We have no reliable way yet to compare animations.
            new ArcGISDynamicServiceSample(), // Excluded cause it was not reliable and had no priority to fix.
            new CustomSvgStyleSample(), // Is currently not functioning and should be fixed with a redesign.
            new ImageCalloutSample(), // Is currently not functioning and should be fixed with a rewrite of the sample.
        ];

    [Test]
    [Retry(5)]
    [TestCaseSource(nameof(RegressionSamples))]
    public async Task TestSampleAsync(ISampleBase sample)
    {
        var original = Logger.LogDelegate;
        try
        {
            Logger.LogDelegate = SampleHelper.ConsoleLog;
            // At the moment of writing this comment we do not have logging in the map. To compare
            // images we disable it for now. Perhaps we want logging to be part of the test image in some cases.
            LoggingWidget.ShowLoggingInMap = ShowLoggingInMap.Never;
            SampleHelper.ConsoleLog(LogLevel.Debug, $"Start MapRegressionTest {sample.GetType().Name}", null);
            await TestSampleAsync(sample, true).ConfigureAwait(false);
        }
        finally
        {
            SampleHelper.ConsoleLog(LogLevel.Debug, $"End MapRegressionTest {sample.GetType().Name}", null);
            Logger.LogDelegate = original;
        }
    }

    public static async Task TestSampleAsync(ISampleBase sample, bool compareImages)
    {
        try
        {
            var fileName = sample.GetType().Name + ".Regression.png";
            using var mapControl = await SampleHelper.InitMapAsync(sample).ConfigureAwait(false);
            var map = mapControl.Map;
            await SampleHelper.DisplayMapAsync(mapControl).ConfigureAwait(false);

            if (map != null)
            {
                // act
                using var mapRenderer = CreateMapRenderer(mapControl);
                {
                    _ = await ImageSourceCacheInitializer.FetchImagesInViewportAsync(mapRenderer.ImageSourceCache,
                        map.Navigator.Viewport, map.Layers, map.Widgets);

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
        await TestSampleAsync(sample, false);
    }
}
