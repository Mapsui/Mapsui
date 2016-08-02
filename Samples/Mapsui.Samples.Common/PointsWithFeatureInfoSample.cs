using System.Globalization;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class PointsWithFeatureInfoSample
    {
        public static ILayer CreateLayer(BoundingBox envelope)
        {
            var pointLayer = CreateRandomPointLayer(PointsSample.CreateRandomPointsProvider(envelope));
            pointLayer.Name = "Points with feature info";
            pointLayer.Style = new StyleCollection
            {
                new SymbolStyle
                {
                    SymbolScale = 1, Fill = new Brush(Color.Cyan),
                    Outline = { Color = Color.White, Width = 2}
                }
            };
            return pointLayer;
        }

        public static ILayer CreateRandomPointLayer(IProvider dataSource)
        {
            return new Layer("pointLayer")
            {
                DataSource = dataSource,
                Style = new SymbolStyle { SymbolScale = 1, Fill = new Brush(Color.Blue) }
            };
        }
    }
}
