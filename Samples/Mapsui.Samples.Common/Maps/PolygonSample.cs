using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class PolygonSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider(CreatePolygon()),
                Style = new VectorStyle
                {
                    Fill = new Brush(new Color(150, 150, 30, 128)),
                    Outline = new Pen
                    {
                        Color = Color.Orange,
                        Width = 2,
                        PenStyle = PenStyle.DashDotDot, //.Solid,
                        PenStrokeCap = PenStrokeCap.Round
                    }
                }
            };
        }

        private static List<Polygon> CreatePolygon()
        {
            var result = new List<Polygon>();

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

            result.Add(polygon);

            polygon = new Polygon();
            polygon.ExteriorRing.Vertices.Add(new Point(-10000000, 0));
            polygon.ExteriorRing.Vertices.Add(new Point(-15000000, 5000000));
            polygon.ExteriorRing.Vertices.Add(new Point(-10000000, 10000000));
            polygon.ExteriorRing.Vertices.Add(new Point(-5000000, 5000000));
            polygon.ExteriorRing.Vertices.Add(new Point(-10000000, 0));
            linearRing = new LinearRing();
            linearRing.Vertices.Add(new Point(-10000000, 1000000));
            linearRing.Vertices.Add(new Point(-6000000, 5000000));
            linearRing.Vertices.Add(new Point(-10000000, 9000000));
            linearRing.Vertices.Add(new Point(-14000000, 5000000));
            linearRing.Vertices.Add(new Point(-10000000, 1000000));
            polygon.InteriorRings.Add(linearRing);

            result.Add(polygon);

            return result;
        }
    }
}