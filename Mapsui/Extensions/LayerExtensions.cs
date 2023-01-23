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
        bool waited = false;
        while (layer.Busy)
        {
            waited = true;
            await Task.Delay(1).ConfigureAwait(false);
        }

        return waited;
    }
}
