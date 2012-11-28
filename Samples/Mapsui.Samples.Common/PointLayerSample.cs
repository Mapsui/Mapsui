using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class PointLayerSample
    {
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
    }
}
