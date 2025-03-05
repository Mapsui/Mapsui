namespace Mapsui.Styles;

public interface IPointStyle
{
    Offset Offset { get; set; }
    RelativeOffset RelativeOffset { get; set; }
    bool RotateWithMap { get; set; }
    Offset SymbolOffset { get; set; }
    bool SymbolOffsetRotatesWithMap { get; set; }
    double SymbolRotation { get; set; }
    double SymbolScale { get; set; }
}
