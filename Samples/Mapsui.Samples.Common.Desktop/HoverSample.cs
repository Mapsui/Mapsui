using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Desktop
{
    public class HoverSample
    {
        private const string HoverLayerName = "Hover Layer";

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateHoverLayer(map.Envelope));

            map.HoverLayers.Add(map.Layers.First(l => l.Name == HoverLayerName));

            return map;
        }

        private static ILayer CreateHoverLayer(BoundingBox envelope)
        {
            return new Layer(HoverLayerName)
            {
                DataSource = RandomPointHelper.CreateProviderWithRandomPoints(envelope, 25, 8),
                Style = CreateHoverSymbolStyle()
            };
        }

        private static SymbolStyle CreateHoverSymbolStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.8,
                Fill = new Brush(new Color(251, 236, 215)),
                Outline = { Color = Color.Gray, Width = 1 }
            };
        }
    }
}
