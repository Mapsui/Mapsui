using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class KeepWithinExtentsSample : ISample
    {
        public string Name => "Keep Within Extents";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map
            {
                Limiter = new ViewportLimiterKeepWithin()
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            return map;
        }
    }
}