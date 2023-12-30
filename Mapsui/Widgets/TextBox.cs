using Mapsui.Styles;

namespace Mapsui.Widgets;

public class TextBox : Widget
{
    public double PaddingX { get; set; } = 3;
    public double PaddingY { get; set; } = 1;
    public double CornerRadius { get; set; } = 8;
    public string? Text { get; set; }
    public Color BackColor { get; set; } = new(255, 255, 255, 128);
    public Color TextColor { get; set; } = new(0, 0, 0);
}
