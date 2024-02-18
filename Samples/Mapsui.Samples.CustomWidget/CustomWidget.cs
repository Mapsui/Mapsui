using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.CustomWidget;

public class CustomWidget : IWidget
{
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public MRect Margin { get; set; } = new MRect(20);
    public MRect? Envelope { get; set; }
    public Color? Color { get; set; }
    public MPoint Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool Enabled { get; set; } = true;
    public bool NeedsRedraw { get; set; }
}
