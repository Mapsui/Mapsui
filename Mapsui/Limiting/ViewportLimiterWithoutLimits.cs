namespace Mapsui.Limiting;

public class ViewportLimiterWithoutLimits : BaseViewportLimiter
{
    public override Viewport Limit(Viewport viewport, MRect? panExtent, MMinMax? zoomExtremes)
    {
        return viewport;
    }
}
