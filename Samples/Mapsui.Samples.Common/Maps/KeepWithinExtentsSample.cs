using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class KeepWithinExtentsSample
    {
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