namespace Mapsui.Limiting;
public class LimitResult
{
    public LimitResult(Viewport input, Viewport output)
    {
        ZoomLimited = input.Resolution != output.Resolution;
        FullyLimited = 
            input.CenterX != output.CenterX &&
            input.CenterY != output.CenterY &&
            ZoomLimited;
    }

    public bool ZoomLimited { get; }
    public bool FullyLimited { get; }

}
