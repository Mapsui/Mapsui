using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class InfoLayersSample
    {
        private const string InfoLayerName = "Points with feature info";

        public static ILayer CreateLayer(BoundingBox envelope)
        {
            var pointLayer = CreateRandomPointLayer(PointsSample.CreateRandomPointsProvider(envelope));
            pointLayer.Name = InfoLayerName;
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

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateLayer(map.Envelope));
            map.InfoLayers.Add(map.Layers.FindLayer(InfoLayerName).First());
            return map;
        }

        public static ILayer CreateRandomPointLayer(IProvider dataSource)
        {
            return new Layer("Point Layer")
            {
                DataSource = dataSource,
                Style = new SymbolStyle { SymbolScale = 1, Fill = new Brush(Color.Blue) }
            };
        }
    }
}
