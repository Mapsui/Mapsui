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
        private static readonly Random Random = new Random(0);

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
            for (var i = 0; i < count; i++)
            {
                result.Add(new Point(Random.NextDouble() * box.Width + box.Left, Random.NextDouble() * box.Height - box.Top));
            }
            return result;
        }

        public static ILayer CreateRandomPointLayerWithLabel(IProvider dataSource)
        {
            var styleList = new StyleCollection
                {
                    new SymbolStyle {SymbolScale = 1.4, Fill = new Brush(Color.Blue)},
                    new LabelStyle {Text = "TestLabel"}
                };
            return new Layer("pointLayer") {DataSource = dataSource, Style = styleList};
        }

        public static ILayer CreateStackedLabelLayer(IProvider provider)
        {
            return new LabelLayer("stacks")
                {
                    DataSource = provider,
                    UseLabelStacking = true,
                    LabelColumn = "Label",
                    Style = new LabelStyle(),
                };
        }

        public static Point GenerateRandomPoint(BoundingBox box)
        {            
            return new Point(Random.NextDouble() * box.Width + box.Left, Random.NextDouble() * box.Height - box.Top);
        }

        public static ILayer CreateRandomPointLayer(BoundingBox envelope, int count = 25)
        {
            return new Layer("pointLayer")
                {
                    DataSource = new MemoryProvider(GenerateRandomPoints(envelope, count)),
                    Style = new VectorStyle {Fill = new Brush(Color.White)},
                };
        }

        public static ILayer CreateRandomPolygonLayer(BoundingBox envelope, int count = 10)
        {
            return new Layer("pointLayer")
                {
                    DataSource = new MemoryProvider(GenerateRandomPolygons(envelope, count)),
                    Style = new VectorStyle
                        {
                            Fill = new Brush(Color.Orange),
                            Outline = new Pen(Color.Red, 2)
                        }
                };
        }

        private static IEnumerable<IGeometry> GenerateRandomPolygons(BoundingBox envelope, int count)
        {
            var result = new List<IGeometry>();
            for (var i = 0; i < count; i++)
            {
                result.Add(new Polygon(
                    new LinearRing(
                        new List<Point>
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
