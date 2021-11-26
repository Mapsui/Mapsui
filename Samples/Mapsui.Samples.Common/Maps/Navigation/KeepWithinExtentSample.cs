using Mapsui.Layers.Tiling;
using Mapsui.Projections;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps.Navigation
{
    public class KeepWithinExtentSample : ISample
    {
        public string Name => "Keep Within Extent";
        public string Category => "Navigation";

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

        private static MRect GetLimitsOfMadagaskar()
        {
            var (minX, minY) = SphericalMercator.FromLonLat(41.8, -27.2);
            var (maxX, maxY) = SphericalMercator.FromLonLat(52.5, -11.6);
            return new MRect(minX, minY, maxX, maxY);
        }
    }
}