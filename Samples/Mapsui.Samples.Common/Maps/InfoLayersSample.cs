using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps
{
    public static class InfoLayersSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            var randomPointLayer = CreateRandomPointLayer(PointsSample.CreateRandomPointsProvider(map.Envelope));
            map.Layers.Add(randomPointLayer);
            map.InfoLayers.Add(randomPointLayer);
            return map;
        }

        private static ILayer CreateRandomPointLayer(IProvider dataSource)
        {
            return new Layer("Points with feature info")
            {
                DataSource = dataSource,
                Style = CreateSymbolStyle()
            };
        }

        private static SymbolStyle CreateSymbolStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 1,
                Fill = new Brush(Color.Cyan),
                Outline = {Color = Color.White, Width = 2}
            };
        }
    }
}