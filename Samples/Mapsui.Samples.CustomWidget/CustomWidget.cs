using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.CustomWidget;

public class CustomWidget : IWidget
{
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public float MarginX { get; set; } = 20;
    public float MarginY { get; set; } = 20;
    public MRect? Envelope { get; set; }
    public bool HandleWidgetTouched(INavigator navigator, MPoint position)
    {
        navigator.CenterOn(0, 0);
        return true;
    }

    public Color? Color { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Enabled { get; set; } = true;
}
