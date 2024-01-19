using Mapsui.Features;
using Mapsui.Layers;
using System.Linq;

namespace Mapsui.Extensions;

public static  class MapExtensions
{
    /// <summary>
    /// Add a layer for <see cref="Marker">
    /// </summary>
    /// <remarks>
    /// This layer should be the topmost <see cref="Layer"> in a <see cref="Map">, so that the <see cref="Callouts">
    /// are always on top.
    /// </remarks>
    /// <param name="map">Map to add this layer too</param>
    /// <param name="name">Name of layer</param>
    /// <returns>Created MemoryLayer</returns>
    public static MemoryLayer AddMarkerLayer(this Map map, string name)
    {
        // Create layer
        var layer = new MemoryLayer(name)
        {
            Style = null,
            IsMapInfoLayer = true
        };

        // Set function for sort order
        layer.SortFeatures = (features) => features.OrderBy((f) => f is Marker && ((Marker)f).HasCallout).ThenBy((f) => f.ZOrder).ThenBy((f) => f.Id);

        // Add handling of touches
        map.Info += (object? sender, MapInfoEventArgs args) =>
        {
            if (args.MapInfo?.Feature == null || args.MapInfo.Feature is not Marker marker) return;

            var hasCallout = marker.HasCallout;

            foreach (var m in layer.Features.Where(f => f is Marker && ((Marker)f).HasCallout))
                ((Marker)m).HideCallout();

            if (!hasCallout)
                marker.ShowCallout();

            layer.DataHasChanged();
        };
        
        // Add layer to map
        map.Layers.Add(layer);

        return layer;
    }
}
