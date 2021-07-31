using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class StackedLabelsSample : ISample
    {
        private const string LabelColumn = "Label";

        public string Name => "Stacked labels";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            var provider = RandomPointHelper.CreateProviderWithRandomPoints(map.Envelope);
            map.Layers.Add(CreateStackedLabelLayer(provider, LabelColumn));
            map.Layers.Add(CreateLayer(provider));
            return map;
        }

        private static ILayer CreateStackedLabelLayer(IProvider provider, string labelColumn)
        {
            return new MemoryLayer
            {
                Name = "StackedLabelLayer",
                Style = null,
                DataSource = new StackedLabelProvider(provider, new LabelStyle
                {
                    BackColor = new Brush {Color = new Color(240, 240, 240, 128)},
                    ForeColor = new Color(50, 50, 50),
                    LabelColumn = labelColumn,
                    Font = new Font {  FontFamily = "Cambria", Size = 14}
                })
            };
        }

        private static ILayer CreateLayer(IProvider dataSource)
        {
            return new Layer("Point Layer")
            {
                DataSource = dataSource,
                Style = new SymbolStyle
                {
                    SymbolScale = 0.85,
                    Fill = new Brush(new Color(190, 100, 130, 210)),
                    Outline = new Pen(new Color(140, 50, 100, 210))
                }
            };
        }
    }
}