using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.CustomWidget;

public class CustomWidget : IWidget
{
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public double MarginX { get; set; } = 20;
    public double MarginY { get; set; } = 20;
    public MRect? Envelope { get; set; }
    public bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        navigator.CenterOn(0, 0);
        return true;
    }

    public Color? Color { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool Enabled { get; set; } = true;
}
