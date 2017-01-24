using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class ProjectionSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                Transformation = new MinimalTransformation(),
                CRS = "EPSG:3857",
                BackColor = Color.Gray
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateWgs84PointLayer());
            return map;
        }

        public static Layer CreateWgs84PointLayer()
        {
            return new Layer
            {
                DataSource = new MemoryProvider(new Point(4.643331, 52.433489)) {CRS = "EPSG:4326"},
                Name = "WGS84 Point"
            };
        }
    }
}