using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Extensions;

public static class LayerExtensions
{
    /// <summary> Wait for Loading Async </summary>
    /// <param name="layer">layer to wait for</param>
    /// <returns>true if it has waited false otherwise</returns>
    public static async Task<bool> WaitForLoadingAsync(this ILayer layer)
    {
        // The regressions tests are flaky. The line below fixes this. My guess was that layer.Busy
        // was not set yet by the async fetcher, but I am not sure if this actually is the problem.
        // A better soltion would be to set the Layer.Busy = true immediately in Layer.Refresh data,
        // but we need to be sure that it will be set to false in all scenarios, also when there is 
        // no data, or an exception occurs.
        await Task.Delay(100).ConfigureAwait(false);
        bool waited = false;
        while (layer.Busy)
        {
            waited = true;
            await Task.Delay(1).ConfigureAwait(false);
        }

        return waited;
    }
}
