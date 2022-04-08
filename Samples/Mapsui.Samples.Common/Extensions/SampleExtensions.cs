using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Extensions;

public static class SampleExtensions
{
    public static async Task SetupAsync(this ISample sample, IMapControl mapControl)
    {
        if (sample is IAsyncSample asyncSample)
        {
            await asyncSample.SetupAsync(mapControl);
        }
        else
        {
            sample.Setup(mapControl);
        }
    }    
}