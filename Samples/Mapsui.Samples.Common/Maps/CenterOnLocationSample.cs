using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;

namespace Mapsui.Samples.Common.Maps
{
    public static class CenterOnLocationSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());

            // Get the lon lat coordinates from somewhere (Mapsui can not help you there)
            var centerOfLondonOntario = new Point(-81.2497, 42.9837);
            // OSM uses spherical mercator coordinates. So transform the lon lat coordinates to spherical mercator
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfLondonOntario.X, centerOfLondonOntario.Y);
            // Set the center of the viewport to the coordinate. The UI will refresh automatically
            map.Viewport.Center = sphericalMercatorCoordinate;
            // Additionally you might want to set the resolution, this could depend on your specific purpose
            map.Viewport.Resolution = map.Resolutions[9];

            return map;
        }

        public static ILayer CreateLayer()
        {
            return new TileLayer(KnownTileSources.Create()) {Name = "OSM"};
        }
    }
}