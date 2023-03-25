namespace Mapsui.Limiting;

public class ViewportLimiterWithoutLimits : BaseViewportLimiter
{
    public override ViewportState Limit(ViewportState viewportState, MRect? panExtent, MMinMax? zoomExtremes)
    {
        return viewportState;
    }
}
