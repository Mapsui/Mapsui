using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class OpacityStyleSample : ISample
    {
        public string Name => "OpacityStyle";
        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePolygonLayer());
            map.Layers.Add(CreateLineStringLayer());
            return map;
        }

        public static ILayer CreatePolygonLayer()
        {
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider(CreatePolygon()),
                Style = new VectorStyle
                {
                    Fill = new Brush(new Color(150, 150, 30)),
                    Outline = new Pen
                    {
                        Color = Color.Orange,
                        Width = 2,
                        PenStyle = PenStyle.Solid,
                        PenStrokeCap = PenStrokeCap.Round
                    },
                    Opacity = 0.7f,
                }
            };
        }
        public static ILayer CreateLineStringLayer()
        {
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider(CreateLineString()),
                Style = new VectorStyle
                {
                    Line = new Pen
                    {
                        Color = new Color(new Color(30, 150, 150)),
                        PenStrokeCap = PenStrokeCap.Round,
                        PenStyle = PenStyle.Solid,
                        Width = 10,
                    },
                    Opacity = 0.5f,
                }
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

        private static LineString CreateLineString()
        {
            var lineString = new LineString();
            lineString.Vertices.Add(new Point(1000000, 1000000));
            lineString.Vertices.Add(new Point(9000000, 1000000));
            lineString.Vertices.Add(new Point(9000000, 9000000));
            lineString.Vertices.Add(new Point(1000000, 9000000));
            lineString.Vertices.Add(new Point(1000000, 1000000));
            return lineString;
        }
    }
}