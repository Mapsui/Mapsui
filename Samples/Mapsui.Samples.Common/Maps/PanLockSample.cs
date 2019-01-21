using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    class PanLockSample : ISample
    {
        public string Name { get; } = "PanLock";
        public string Category { get; } = "Special";
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            mapControl.Map.PanLock = true;
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            return map;
        }
    }
}
