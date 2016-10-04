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
            map.Layers.Add(CreateLayer(provider));
            map.Layers.Add(PointsSample.CreateRandomPointLayer(provider));
            return map;
        }

        public static ILayer CreateLayer(IProvider provider)
        {
            return new LabelLayer("stacks")
            {
                DataSource = provider,
                UseLabelStacking = true,
                LabelColumn = "Label",
                Style = new LabelStyle(),
            };
        }
    }
}
