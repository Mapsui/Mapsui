using Mapsui.Styles;

namespace Mapsui.Widgets;

public class TextBox : Widget
{
    public int PaddingX { get; set; } = 3;
    public int PaddingY { get; set; } = 1;
    public int CornerRadius { get; set; } = 8;
    public string? Text { get; set; }
    public Color BackColor { get; set; } = new(255, 255, 255, 128);
    public Color TextColor { get; set; } = new(0, 0, 0);
    public int? Width { get; set; }
    public int? Height { get; set; }
    public override bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        // don has any action
        return false;
    }

    public override bool Touchable => false;
}
