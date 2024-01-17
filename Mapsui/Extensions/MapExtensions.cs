using Mapsui.Layers;

namespace Mapsui.Extensions;

public static  class MapExtensions
{
    public static MemoryLayer AddMarkerLayer(this Map map, string name)
    {
        // Create layer
        var layer = new MemoryLayer(name)
        {
            Style = null,
            IsMapInfoLayer = true
        };

        // Set function for sort order
        layer.SortFeatures = (features) => features.OrderBy((f) => f.ZOrder).ThenBy((f) => f.Id);

        // Add handling of touches
        map.Info += (object? sender, MapInfoEventArgs args) =>
        {
            if (args.MapInfo?.Feature == null || args.MapInfo.Feature is not Marker marker) return;

            var hasCallout = marker.HasCallout;

            foreach (var m in Features.Where(f => f is Marker && ((Marker)f).HasCallout))
                ((Marker)m).HideCallout();

            if (!hasCallout)
                marker.ShowCallout();

            DataHasChanged();
        };
        
        // Add layer to map
        map.Layers.Add(layer);

        return layer;
    }
}
