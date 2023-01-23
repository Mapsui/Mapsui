using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Extensions;

public static class EnumerableLayerExtensions
{
    /// <summary> Wait for Loading Async </summary>
    /// <param name="layers">layers to wait for</param>
    /// <returns>true if it has waited false otherwise</returns>
    public static async Task<bool> WaitForLoadingAsync(this IEnumerable<ILayer> layers)
    {
        bool waited = false;
        foreach (var layer in layers)
        {
            if (await layer.WaitForLoadingAsync().ConfigureAwait(false))
                waited = true;
        }

        return waited;
    }
}
