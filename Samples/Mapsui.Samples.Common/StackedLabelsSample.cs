using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class StackedLabelsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            var provider = PointsSample.CreateRandomPointsProvider(map.Envelope);
            map.Layers.Add(CreateLabelLayer(provider));
            map.Layers.Add(CreateLayer(provider));
            return map;
        }

        private static ILayer CreateLabelLayer(IProvider provider)
        {
            return new LabelLayer("stacks")
            {
                DataSource = provider,
                UseLabelStacking = true,
                LabelColumn = "Label",
                Style = new LabelStyle(),
            };
        }

        private static ILayer CreateLayer(IProvider dataSource)
        {
            return new Layer("Point Layer")
            {
                DataSource = dataSource,
                Style = new SymbolStyle { SymbolScale = 1, Fill = new Brush(Color.Blue) }
            };
        }
    }
}