// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.UI;

namespace Mapsui.Rendering.Skia.Tests.Helpers;
internal static class SampleHelper
{
    public static async Task<RegressionMapControl> InitMapAsync(ISampleBase sample)
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
            await sampleTest.InitializeTestAsync(mapControl).ConfigureAwait(false);
        }

        await mapControl.WaitForLoadingAsync();
        var fetchInfo = new FetchInfo(mapControl.Map.Navigator.Viewport.ToSection(), mapControl.Map.CRS);
        mapControl.Map.RefreshData(fetchInfo);

        return mapControl;
    }

    public static async Task DisplayMapAsync(IMapControl mapControl)
    {
        await mapControl.WaitForLoadingAsync().ConfigureAwait(false);

        // wait for rendering to finish to make the Tests more reliable
        await Task.Delay(300).ConfigureAwait(false);
    }

    public static void ConsoleLog(LogLevel arg1, string arg2, Exception? arg3)
    {
        var message = $@"LogLevel: {arg1} Message: {arg2}";
        if (arg3 != null)
        {
            message += $@" Exception: {arg3}";
        }

        Console.WriteLine(message);
        Console.Out.Flush();
    }
}
