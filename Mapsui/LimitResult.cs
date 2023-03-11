namespace Mapsui;
public class LimitResult
{
    public LimitResult(ViewportState input, ViewportState output)
    {
        ZoomLimited = input.Resolution != output.Resolution;
        PanXLimited = input.CenterX != output.CenterX;
        PanYLimited = input.CenterY != output.CenterY;
        PanXOrYLimited = PanXLimited || PanYLimited;
        PanLimited = PanXLimited && PanYLimited;
    }

    public bool ZoomLimited { get; }
    public bool PanLimited { get; }

    // Not sure if I need to properties below
    public bool PanXOrYLimited { get; }
    public bool PanXLimited { get; }
    public bool PanYLimited { get; }

}
