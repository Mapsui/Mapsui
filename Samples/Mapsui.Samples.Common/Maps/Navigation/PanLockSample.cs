using Mapsui.Layers.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps.Navigation
{
    internal class PanLockSample : ISample
    {
        public string Name => "PanLock";
        public string Category => "Navigation";
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
