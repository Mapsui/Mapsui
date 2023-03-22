namespace Mapsui.Limiting;
public class LimitResult
{
    public LimitResult(ViewportState input, ViewportState output)
    {
        ZoomLimited = input.Resolution != output.Resolution;
        PanXLimited = input.CenterX != output.CenterX;
        PanYLimited = input.CenterY != output.CenterY;
        PanXOrYLimited = PanXLimited || PanYLimited;
        PanLimited = PanXLimited && PanYLimited;
        ViewportStateChanged = input != output;
    }

    public bool ViewportStateChanged { get; }
    public bool ZoomLimited { get; }
    public bool PanLimited { get; }

    // Not sure if I need the properties below
    public bool PanXOrYLimited { get; }
    public bool PanXLimited { get; }
    public bool PanYLimited { get; }
    public bool Limited => ZoomLimited || PanLimited;

}
