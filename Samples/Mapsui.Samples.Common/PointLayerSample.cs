using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;

namespace Mapsui.Samples.Common
{
    public static class PointLayerSample
    {
        private static Random random = new Random();

        public static ILayer Create()
        {
            var layer = new Layer("PointLayer");
            var pointWithDefaultSymbolStyle = new Feature { Geometry = new Point(1000000, 1000000)};
            pointWithDefaultSymbolStyle.Styles.Add(new SymbolStyle());
            var pointAsSmallBlackDot = new Feature { Geometry = new Point(1000000, 0)};

            pointAsSmallBlackDot.Styles.Add(new SymbolStyle
                {
                    SymbolScale = 2.0f,
                    Fill = new Brush { Color = null },
                    Outline = new Pen { Color = Color.Green}
                });

            pointAsSmallBlackDot.Styles.Add(new SymbolStyle
                {
                    SymbolScale = 0.5f,
                    Fill = new Brush { Color = Color.Black },
                });

            var pointWithlabelStyle = new Feature { Geometry = new Point(0, 1000000)};
            pointWithDefaultSymbolStyle.Styles.Add(new LabelStyle { Text = "Label" });

            layer.DataSource = new MemoryProvider(new[] { pointWithlabelStyle, pointWithDefaultSymbolStyle, pointAsSmallBlackDot });
            return layer;
        }

        public static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox box, int count = 25)
        {
           var result = new List<IGeometry>();
            for (int i = 0; i < count; i++)
            {
                result.Add(new Mapsui.Geometries.Point(random.NextDouble() * box.Width + box.Left, random.NextDouble() * box.Height - box.Top));
            }
            return result;
        }

        public static Point GenerateRandomPoint(BoundingBox box)
        {            
            return new Mapsui.Geometries.Point(random.NextDouble() * box.Width + box.Left, random.NextDouble() * box.Height - box.Top);
        }

        public static ILayer CreateRandomPointLayer(BoundingBox envelope, int count = 25)
        {
            var pointLayer = new Layer("pointLayer")
                {
                    DataSource = new MemoryProvider(PointLayerSample.GenerateRandomPoints(envelope, count)),
                };
            pointLayer.Styles.Add(new VectorStyle() { Fill = new Brush(Color.Red) });
            return pointLayer;
        }

        public static ILayer CreateRandomPolygonLayer(BoundingBox envelope, int count = 10)
        {
            var pointLayer = new Layer("pointLayer")
                {
                    DataSource = new MemoryProvider(PointLayerSample.GenerateRandomPolygons(envelope, count))
                };

            pointLayer.Styles.Add(new VectorStyle()
                {
                    Fill = new Brush(Color.Orange),
                    Line = new Pen(Color.Black, 2),
                    Outline = new Pen(Color.Red, 1)
                });
            return pointLayer;
        }

        private static IEnumerable<IGeometry> GenerateRandomPolygons(BoundingBox envelope, int count)
        {
            var random = new Random();
            var result = new List<IGeometry>();
            for (int i = 0; i < count; i++)
            {
                result.Add(new Mapsui.Geometries.Polygon(
                    new LinearRing(
                        new List<Point>()
                        {
                            GenerateRandomPoint(envelope),
                            GenerateRandomPoint(envelope),
                            GenerateRandomPoint(envelope)
                        })));
            }
            return result;
        }
    }
}
