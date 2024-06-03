namespace Mapsui.Styles;

public static class SymbolStyles
{
    public static SymbolStyle CreatePinStyle(Color? fillColor = null, Color? strokeColor = null, double symbolScale = 1.0) => new()
    {
        ImageSource = "embedded://Mapsui.Resources.Images.Pin.svg",
        SymbolOffset = new RelativeOffset(0.0, 0.5),
        SymbolScale = symbolScale,
        SvgFillColor = fillColor ?? Color.FromArgb(255, 57, 115, 199),
        SvgStrokeColor = strokeColor ?? Color.FromArgb(210, 245, 245, 245),
    };
}
