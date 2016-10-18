using Mapsui.Geometries;
using Mapsui.Layers;

namespace Mapsui.Tests.Common.Maps
{
    public static class BitmapSymbolWithRotationAndOffsetSample
    {
        public static Map CreateMap()
        {
            var map = new Map {Viewport = {Center = new Point(80, 80), Width = 200, Height = 200, Resolution = 1}};
            var layer = new MemoryLayer
            {
                DataSource = Utilities.CreateProviderWithRotatedBitmapSymbols(),
                Name = "Points with rotated bitmaps",
                Style = null
            };
            map.Layers.Add(layer);
            return map;
        }
    }
}