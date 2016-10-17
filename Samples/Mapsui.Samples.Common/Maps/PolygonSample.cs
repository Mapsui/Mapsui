using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Samples.Common.Maps
{
    public static class PolygonSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateLayer());
            return map;
        }

        private static ILayer CreateLayer()
        {
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider(CreatePolygon())
            };
        }

        private static Polygon CreatePolygon()
        {
            var polygon = new Polygon();
            polygon.ExteriorRing.Vertices.Add(new Point(0, 0));
            polygon.ExteriorRing.Vertices.Add(new Point(0, 10000000));
            polygon.ExteriorRing.Vertices.Add(new Point(10000000, 10000000));
            polygon.ExteriorRing.Vertices.Add(new Point(10000000, 0));
            polygon.ExteriorRing.Vertices.Add(new Point(0, 0));
            var linearRing = new LinearRing();
            linearRing.Vertices.Add(new Point(1000000, 1000000));
            linearRing.Vertices.Add(new Point(9000000, 1000000));
            linearRing.Vertices.Add(new Point(9000000, 9000000));
            linearRing.Vertices.Add(new Point(1000000, 9000000));
            linearRing.Vertices.Add(new Point(1000000, 1000000));
            polygon.InteriorRings.Add(linearRing);
            return polygon;
        }
    }
}