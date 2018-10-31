using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Samples.Common.Maps
{
    public class WidgetSample : ISample
    {
        public string Name => "2 Widgets";
        public string Category => "Widgets";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Widgets.Add(CreateHyperlink("Top Right", VerticalAlignment.Top, HorizontalAlignment.Right));
            map.Widgets.Add(CreateHyperlink("Center Right", VerticalAlignment.Center, HorizontalAlignment.Right));
            map.Widgets.Add(CreateHyperlink("Bottom Right", VerticalAlignment.Bottom, HorizontalAlignment.Right));
            map.Widgets.Add(CreateHyperlink("Bottom Center", VerticalAlignment.Bottom, HorizontalAlignment.Center));

            map.Widgets.Add(CreateHyperlink("Bottom Left", VerticalAlignment.Bottom, HorizontalAlignment.Left));
            map.Widgets.Add(CreateHyperlink("Center Left", VerticalAlignment.Center, HorizontalAlignment.Left));
            map.Widgets.Add(CreateHyperlink("Top Left", VerticalAlignment.Top, HorizontalAlignment.Left));
            map.Widgets.Add(CreateHyperlink("Top Center", VerticalAlignment.Top, HorizontalAlignment.Center));

            map.Widgets.Add(new ZoomInOutWidget { MarginX = 20, MarginY = 20 });
            map.Widgets.Add(new ZoomInOutWidget { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });

            return map;
        }

        private static IWidget CreateHyperlink(string text, VerticalAlignment verticalAlignment, 
            HorizontalAlignment horizontalAlignment)
        {
            return new Hyperlink()
            {
                Text = text,
                Url = "http://www.openstreetmap.org/copyright",
                VerticalAlignment = verticalAlignment,
                HorizontalAlignment = horizontalAlignment,
                MarginX = 10,
                MarginY = 10,
                PaddingX = 4,
                PaddingY = 4,
                BackColor = new Color(255, 192, 203)
            };
        }
    }
}
