using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Extensions;

/// <summary>
/// Map Control Extensions
/// </summary>
public static class MapControlExtensions
{
    /// <summary> Wait for Loading Async </summary>
    /// <param name="mapControl">map control</param>
    /// <returns>Task for Waiting for Layers</returns>
    public static async Task WaitForLoadingAsync(this IMapControl mapControl)
    {
        if (mapControl.Map?.Layers != null)
        {
            await mapControl.Map.Layers.WaitForLoadingAsync().ConfigureAwait(false);
        }
    }
}
