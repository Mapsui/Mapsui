using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class KeepWithinExtentsSample : IDemoSample
    {
        public string Name => "4.4 Keep Within Extents";

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