namespace Mapsui.Limiting;

public class ViewportLimiterWithoutLimits : IViewportLimiter
{
    public Viewport Limit(Viewport viewport, MRect? panBounds, MMinMax? zoomBounds)
    {
        return viewport;
    }
}
