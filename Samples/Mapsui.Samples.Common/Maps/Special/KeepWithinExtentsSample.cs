using Mapsui.Geometries;
using Mapsui.Projection;
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
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Limiter = new ViewportLimiterKeepWithin
            {
                PanLimits = GetLimitsOfMadagaskar()
            };
            return map;
        }

        private static BoundingBox GetLimitsOfMadagaskar()
        {
            return new BoundingBox(
                SphericalMercator.FromLonLat(41.8, -27.2),
                SphericalMercator.FromLonLat(52.5, -11.6));
        }
    }
}