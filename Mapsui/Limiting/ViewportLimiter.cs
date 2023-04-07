namespace Mapsui.Limiting;

public class ViewportLimiter : IViewportLimiter
{
    public Viewport Limit(Viewport viewport, MRect? panBounds, MMinMax? zoomBounds)
    {
        return LimitExtent(LimitResolution(viewport, zoomBounds), panBounds);
    }

    private Viewport LimitResolution(Viewport viewport, MMinMax? zoomBounds)
    {
        if (zoomBounds is null) return viewport;

        if (zoomBounds.Min > viewport.Resolution) return viewport with { Resolution = zoomBounds.Min };
        if (zoomBounds.Max < viewport.Resolution) return viewport with { Resolution = zoomBounds.Max };

        return viewport;
    }

    private Viewport LimitExtent(Viewport viewport, MRect? panBounds)
    {
        if (panBounds is null) return viewport;

        var x = viewport.CenterX;

        if (viewport.CenterX < panBounds.Left) x = panBounds.Left;
        if (viewport.CenterX > panBounds.Right) x = panBounds.Right;

        var y = viewport.CenterY;
        if (viewport.CenterY > panBounds.Top) y = panBounds.Top;
        if (viewport.CenterY < panBounds.Bottom) y = panBounds.Bottom;

        return viewport with { CenterX = x, CenterY = y };
    }
}
