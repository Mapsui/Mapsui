using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class CreatePolygonSample
    {
        public static ILayer AddLayerWithOnePolygon()
        {
            var layer = new Layer("LayerWithPolygon");
            layer.DataSource = new Mapsui.Providers.MemoryProvider(CreatePolygon());
            layer.Styles.Add(new VectorStyle());
            return layer;
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
