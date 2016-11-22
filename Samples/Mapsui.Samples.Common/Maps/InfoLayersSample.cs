using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps
{
    public static class InfoLayersSample
    {
        private const string InfoLayerName = "Info Layer";
        private const string HoverInfoLayerName = "Hover Info Layer";

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateInfoLayer(map.Envelope));
            map.Layers.Add(CreateHoverInfoLayer(map.Envelope));

            map.InfoLayers.Add(map.Layers.First(l => l.Name == InfoLayerName));
            map.HoverInfoLayers.Add(map.Layers.First(l => l.Name == HoverInfoLayerName));

            return map;
        }

        private static ILayer CreateInfoLayer(BoundingBox envelope)
        {
            return new Layer(InfoLayerName)
            {
                DataSource = PointsSample.CreateProviderWithRandomPoints(envelope, 25),
                Style = CreateSymbolStyle()
            };
        }

        private static ILayer CreateHoverInfoLayer(BoundingBox envelope)
        {
            return new Layer(HoverInfoLayerName)
            {
                DataSource = PointsSample.CreateProviderWithRandomPoints(envelope, 25),
                Style = CreateHoverSymbolStyle()
            };
        }

        private static SymbolStyle CreateHoverSymbolStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.8,
                Fill = new Brush(new Color(251, 236, 215)),
                Outline = {Color = Color.Gray, Width = 1}
            };
        }

        private static SymbolStyle CreateSymbolStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.8,
                Fill = new Brush(new Color(213, 234, 194)),
                Outline = {Color = Color.Gray, Width = 1}
            };
        }
    }
}