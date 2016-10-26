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
                DataSource = new StackedLabelProvider(provider)
                {
                    LabelStyle = new LabelStyle {LabelColumn = labelColumn}
                },
                Style = null
            };
        }

        private static ILayer CreateLayer(IProvider dataSource)
        {
            return new Layer("Point Layer")
            {
                DataSource = dataSource,
                Style = new SymbolStyle {SymbolScale = 1, Fill = new Brush(Color.Blue)}
            };
        }
    }
}