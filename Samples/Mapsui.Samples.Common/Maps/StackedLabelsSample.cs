using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps
{
    public static class StackedLabelsSample
    {
        private const string LabelColumn = "Label";

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            var provider = PointsSample.CreateRandomPointsProvider(map.Envelope);
            map.Layers.Add(CreateStackedLabelLayer(provider, LabelColumn));
            map.Layers.Add(CreateLayer(provider));
            return map;
        }

        private static ILayer CreateStackedLabelLayer(IProvider provider, string labelColumn)
        {
            return new MemoryLayer
            {
                DataSource = new StackedLabelProvider(provider, new LabelStyle
                {
                    BackColor = new Brush
                    {
                        Color = new Color(240, 240, 240, 128)
                    },
                    ForeColor = new Color(25, 25, 25),
                    LabelColumn = labelColumn
                }),
                Style = null
            };
        }

        private static ILayer CreateLayer(IProvider dataSource)
        {
            return new Layer("Point Layer")
            {
                DataSource = dataSource,
                Style = new SymbolStyle {SymbolScale = 0.75, Fill = new Brush(Color.Blue)}
            };
        }
    }
}