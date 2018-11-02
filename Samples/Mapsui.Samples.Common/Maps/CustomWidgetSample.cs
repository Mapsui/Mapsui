using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;

namespace Mapsui.Samples.Common.Maps
{
    public class CustomWidgetSample : ISample
    {
        public string Name => "3 Custom Widget";

        public string Category => "Widgets";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new CustomWidget.CustomWidget
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                MarginX = 20,
                MarginY = 20,
                Width = 100,
                Height = 20,
                Color = Color.FromString(Color.KnownColors["goldenrod"])
            });

            return map;
        }
    }
}
