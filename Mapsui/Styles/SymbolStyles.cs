namespace Mapsui.Styles;

public static class SymbolStyles
{
    public static SymbolStyle CreatePinStyle(Color? fillColor = null, Color? strokeColor = null, double symbolScale = 1.0) => new()
    {
        Image = new Image
        {
            Source = "embedded://Mapsui.Resources.Images.Pin.svg",
            SvgFillColor = fillColor ?? Color.FromArgb(255, 57, 115, 199),
            SvgStrokeColor = strokeColor ?? Color.FromArgb(210, 245, 245, 245),
        },
        SymbolOffset = new RelativeOffset(0.0, 0.5),
        SymbolScale = symbolScale,
    };
}
