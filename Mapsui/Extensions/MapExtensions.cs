using Mapsui.Features;
using Mapsui.Layers;
using System.Linq;

namespace Mapsui.Extensions;

public static  class MapExtensions
{
    public static MemoryLayer AddMarkerLayer(this Map map, string name)
    {
        // Create layer
        var layer = new MarkerLayer(name);
        // Add handling of touches
        map.Info += layer.HandleInfo;
        // Add layer to map
        map.Layers.Add(layer);

        return layer;
    }
}
