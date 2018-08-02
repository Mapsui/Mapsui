using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets;

namespace Mapsui.Samples.Common.Maps
{
    public static class CustomWidgetSample
    {
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
