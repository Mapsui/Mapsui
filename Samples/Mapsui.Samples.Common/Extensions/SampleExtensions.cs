using System;
using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Extensions;

public static class SampleExtensions
{
    public static async Task SetupAsync(this ISampleBase sample, IMapControl mapControl)
    {
        if (sample is IAsyncSample asyncSample)
        {
            await asyncSample.SetupAsync(mapControl);
        }

        Setup(sample, mapControl);
    }

    public static void Setup(this ISampleBase sample, IMapControl mapControl)
    {
        if (sample is ISample syncSample)
        {
            syncSample.Setup(mapControl);

            return;
        }

        throw new InvalidOperationException();
    }
}