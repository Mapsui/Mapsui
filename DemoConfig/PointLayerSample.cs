using System;
using System.Linq;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Providers;
using SharpMap.Styles;

namespace DemoConfig
{
    public static class PointLayerSample
    {
        public static ILayer Create()
        {
            var layer = new Layer("PointLayer");
            var pointWithDefaultSymbolStyle = new Feature { Geometry = new Point(1000000, 1000000), Style = new SymbolStyle() };
            var pointAsSmallBlackDot = new Feature
                {
                    Geometry = new Point(1000000, 0),
                    Style =
                        new SymbolStyle
                            {
                                SymbolScale = 0.5f,
                                Fill = new Brush { Color = Color.Black },
                                Outline = new Pen { Color = Color.Black}
                            }
                };
            var pointWithlabelStyle = new Feature { Geometry = new Point(0, 1000000), Style = new LabelStyle { Text = "Label" } };

            layer.DataSource = new MemoryProvider(new[] { pointWithlabelStyle, pointWithDefaultSymbolStyle, pointAsSmallBlackDot });
            return layer;
        }
    }
}
