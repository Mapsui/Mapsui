// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Logging;
using Mapsui.Rendering.Skia.Tests.Helpers;
using Mapsui.Rendering.Skia.Tests.Utilities;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.DataFormats;
using Mapsui.Samples.Common.Maps.FeatureAnimations;
using Mapsui.Samples.Common.Maps.Geometries;
using Mapsui.Samples.Common.Maps.MapInfo;
using Mapsui.Samples.Common.Maps.Performance;
using Mapsui.Samples.Common.Maps.Special;
using Mapsui.Samples.Common.Maps.Styles;
using Mapsui.Samples.Common.Maps.WFS;
using Mapsui.Samples.Common.Maps.Widgets;
using Mapsui.Samples.Common.Maps.WMS;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class MapRegressionTests
{
    static MapRegressionTests()
    {
        Samples.Common.Samples.Register();
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
            new CustomSvgColorSample(), // Is currently not functioning and should be fixed with a redesign.
            new ImageCalloutSample(), // Is currently not functioning and should be fixed with a rewrite of the sample.
            new WmsBasilicataSample(), // Times out,
            new RasterizingTileLayerWithThousandsOfPolygonsSample(), // Crashes on the build server. Perhaps a memory limitation.
            new WfsGeometryFilterSample(), // Crashes on the build server.
            new RasterizingTileLayerWithDynamicPointsSample(), // Changes because it is dynamic.
            new ArcGISImageServiceSample(), // Changes and we did not cache the reponse in the sqlite yet.
        ];

    [Test]
    [Retry(5)]
    [TestCaseSource(nameof(RegressionSamples))]
    public async Task TestSampleAsync(ISampleBase sample)
    {
        var original = Logger.LogDelegate;
        try
        {
            SQLitePCL.Batteries.Init();
            Logger.LogDelegate = SampleHelper.ConsoleLog;
            // At the moment of writing this comment we do not have logging in the map. To compare
            // images we disable it for now. Perhaps we want logging to be part of the test image in some cases.
            LoggingWidget.ShowLoggingInMap = ActiveMode.No; // We do not want logging in the mag
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
            // Arrange
            var fileName = sample.GetType().Name + ".Regression.png";
            using var mapControl = await SampleHelper.InitMapAsync(sample).ConfigureAwait(false);
            var map = mapControl.Map;
            await SampleHelper.DisplayMapAsync(mapControl).ConfigureAwait(false);
            Performance.DefaultIsActive = ActiveMode.No; // Never show performance in rendering tests so that Release and Debug runs generate the same image.
            MapRenderer.RegisterWidgetRenderer(typeof(CustomWidget), new CustomWidgetSkiaRenderer());
            var mapRenderer = new MapRenderer();

            if (map != null)
            {
                // Act

                _ = await map.RenderService.ImageSourceCache.FetchAllImageDataAsync(Image.SourceToSourceId);

                using var bitmap = mapRenderer.RenderToBitmapStream(map.Navigator.Viewport, map.Layers,
                    map.RenderService, map.BackColor, 2, map.GetWidgetsOfMapAndLayers());

                // aside
                if (bitmap is { Length: > 0 })
                {
                    File.WriteToGeneratedRegressionFolder(fileName, bitmap);
                }
                else
                {
                    Assert.Fail("Should generate Image");
                }

                // Assert
                if (compareImages)
                {
                    using var originalStream = File.ReadFromOriginalRegressionFolder(fileName);
                    if (originalStream == null)
                    {
                        Assert.Inconclusive($"No Regression Test Data for {sample.Name}");
                    }
                    else
                    {
                        Assert.That(BitmapComparer.Compare(originalStream, bitmap, 1, 0.995), Is.True,
                            $"Fail in sample '{sample.Name}' in category '{sample.Category}'. Image compare failed. The generated image is not equal to the reference image.");
                    }
                }
                else
                {
                    // Don't compare images here because to unreliable
                    Assert.That(true, Is.True);
                }
            }
        }
        finally
        {
            // At this point we would like to dispose the samples but the instance is created
            // once and reused for all retries. Instead we should create an instance per test run.
        }
    }

    [Test]
    [Explicit]
    [TestCaseSource(nameof(ExcludedSamples))]
    public async Task ExcludedTestSampleAsync(ISampleBase sample)
    {
        await TestSampleAsync(sample, false);
    }
}
