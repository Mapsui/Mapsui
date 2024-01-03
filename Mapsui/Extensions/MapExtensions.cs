using Mapsui.Features;
using Mapsui.Layers;
using System.Linq;

namespace Mapsui.Extensions;

public static  class MapExtensions
{
    public static MemoryLayer AddMarkerLayer(this Map map, string name)
    {
        var layer = new MemoryLayer(name) 
        { 
            Style = null, 
            IsMapInfoLayer = true,
        };

        map.Info += (s, e) =>
        {
            if (e.MapInfo?.Feature == null || e.MapInfo.Feature is not Marker marker) return;

            var hasCallout = marker.HasCallout;

            foreach (var m in layer.Features.Where(f => f is Marker && ((Marker)f).HasCallout))
                ((Marker)m).HideCallout();

            if (!hasCallout)
                marker.ShowCallout();

            layer.DataHasChanged();
        };

        map.Layers.Add(layer);

        return layer;
    }
}
