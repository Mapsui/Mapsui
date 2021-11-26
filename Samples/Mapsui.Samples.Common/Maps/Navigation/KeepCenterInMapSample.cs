using Mapsui.Layers.Tiling;
using Mapsui.Projections;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps.Navigation
{
    public class KeepCenterInMapSample : ISample
    {
        public string Name => "Keep Center In Map";
        public string Category => "Navigation";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());

            // This is the default limiter. This limiter ensures that the center 
            // of the viewport always is within the extent. When no PanLimits are
            // specified the Map.Extent is used. In this sample the extent of
            // Madagaskar is used. In such a scenario it makes sense to also limit
            // the top ZoomLimit.

            map.Limiter.PanLimits = GetLimitsOfMadagaskar();
            map.Limiter.ZoomLimits = new MinMax(0.15, 2500);
            map.Home = n => n.NavigateTo(map.Limiter.PanLimits);
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