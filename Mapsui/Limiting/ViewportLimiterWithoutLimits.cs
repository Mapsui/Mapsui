namespace Mapsui.Limiting;

public class ViewportLimiterWithoutLimits : IViewportLimiter
{
    public Viewport Limit(Viewport viewport, MRect? panExtent, MMinMax? zoomExtremes)
    {
        return viewport;
    }
}
