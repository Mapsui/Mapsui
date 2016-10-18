using Mapsui.Geometries;
using Mapsui.Layers;

namespace Mapsui.Tests.Common.Maps
{
    public static class VectorStyleSample
    {
        public static Map CreateMap()
        {
            var map = new Map {Viewport = {Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1}};
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = Utilities.CreateProviderWithPointsWithVectorStyle(),
                Name = "Points with VectorStyle"
            });
            return map;
        }
    }
}