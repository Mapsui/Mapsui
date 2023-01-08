using Mapsui.UI;
using System;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Extensions;

public static class SampleExtensions
{
    public static async Task SetupAsync(this ISampleBase sample, IMapControl mapControl)
    {
        if (sample is ISample asyncSample)
        {
            mapControl.Map = await asyncSample.CreateMapAsync();

            return;
        }

        Setup(sample, mapControl);
    }

    private static void Setup(ISampleBase sample, IMapControl mapControl)
    {
        if (sample is IMapControlSample mapControlSample)
        {
            mapControlSample.Setup(mapControl);

            return;
        }

        throw new InvalidOperationException();
    }
}
