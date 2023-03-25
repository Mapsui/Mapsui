namespace Mapsui.Limiting;

public class ViewportLimiter : BaseViewportLimiter
{
    public override ViewportState Limit(ViewportState viewportState, MRect? panExtent, MMinMax? zoomExtremes)
    {
        var state = LimitResolution(viewportState, zoomExtremes);
        return LimitExtent(state, panExtent);
    }

    private ViewportState LimitResolution(ViewportState viewportState, MMinMax? zoomExtremes)
    {
        if (zoomExtremes is null) return viewportState;

        if (zoomExtremes.Min > viewportState.Resolution) return viewportState with { Resolution = zoomExtremes.Min };
        if (zoomExtremes.Max < viewportState.Resolution) return viewportState with { Resolution = zoomExtremes.Max };

        return viewportState;
    }

    private ViewportState LimitExtent(ViewportState viewportState, MRect? panExtent)
    {
        if (panExtent is null) return viewportState;

        var x = viewportState.CenterX;
        if (viewportState.CenterX < panExtent.Left) x = panExtent.Left;
        if (viewportState.CenterX > panExtent.Right) x = panExtent.Right;

        var y = viewportState.CenterY;
        if (viewportState.CenterY > panExtent.Top) y = panExtent.Top;
        if (viewportState.CenterY < panExtent.Bottom) y = panExtent.Bottom;

        return viewportState with { CenterX = x, CenterY = y };
    }
}
