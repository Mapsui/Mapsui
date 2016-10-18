using Mapsui.Geometries;
using Mapsui.Layers;

namespace Mapsui.Tests.Common.Maps
{
    public static class LineSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                Viewport =
                {
                    Center = new Point(0, 0),
                    Width = 600,
                    Height = 400,
                    Resolution = 63000
                }
            };

            var layer = new MemoryLayer
            {
                Style = null,
                DataSource = Utilities.CreateLineProvider(),
                Name = "Line"
            };

            map.Layers.Add(layer);
            return map;
        }
    }
}