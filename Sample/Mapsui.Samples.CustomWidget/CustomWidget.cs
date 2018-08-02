using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.CustomWidget
{
    public class CustomWidget : IWidget
    {
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public float MarginX { get; set; } = 20;
        public float MarginY { get; set; } = 20;
        public BoundingBox Envelope { get; set; }
        public void HandleWidgetTouched(INavigator navigator, Point position)
        {
            navigator.NavigateTo(0, 0);
        }

        public Color Color { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
