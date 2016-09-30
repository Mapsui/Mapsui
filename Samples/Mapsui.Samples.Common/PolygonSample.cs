using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Samples.Common
{
    public static class PolygonSample
    {
        public static ILayer CreateLayer()
        {
            return new Layer("LayerWithPolygon")
            {
                DataSource = new MemoryProvider(CreatePolygon())
            };
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }

        private static Polygon CreatePolygon()
        {
            var polygon = new Polygon();
            polygon.ExteriorRing.Vertices.Add(new Point(0, 0));
            polygon.ExteriorRing.Vertices.Add(new Point(0, 1000000));
            polygon.ExteriorRing.Vertices.Add(new Point(1000000, 1000000));
            polygon.ExteriorRing.Vertices.Add(new Point(1000000, 0));
            polygon.ExteriorRing.Vertices.Add(new Point(0, 0));
            return polygon;
        }
    }
}
