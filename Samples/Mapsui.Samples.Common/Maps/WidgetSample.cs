using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.Common.Maps
{
    public static class WidgetSample
    {
        public static Map CreateMap()
        {
            var map = new Map();

            map.Widgets.Add(CreateTextBox("Top Right", VerticalAlignment.Top, HorizontalAlignment.Right));
            map.Widgets.Add(CreateTextBox("Center Right", VerticalAlignment.Center, HorizontalAlignment.Right));
            map.Widgets.Add(CreateTextBox("Bottom Right", VerticalAlignment.Bottom, HorizontalAlignment.Right));
            map.Widgets.Add(CreateTextBox("Bottom Center", VerticalAlignment.Bottom, HorizontalAlignment.Center));

            map.Widgets.Add(CreateTextBox("Bottom Left", VerticalAlignment.Bottom, HorizontalAlignment.Left));
            map.Widgets.Add(CreateTextBox("Center Left", VerticalAlignment.Center, HorizontalAlignment.Left));
            map.Widgets.Add(CreateTextBox("Top Left", VerticalAlignment.Top, HorizontalAlignment.Left));
            map.Widgets.Add(CreateTextBox("Top Center", VerticalAlignment.Top, HorizontalAlignment.Center));

            return map;
        }

        private static TextBox CreateTextBox(string text, VerticalAlignment verticalAlignment, 
            HorizontalAlignment horizontalAlignment)
        {
            return new TextBox
            {
                Text = text,
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
