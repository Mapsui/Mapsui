namespace Mapsui.Limiting;

public class ViewportLimiter : IViewportLimiter
{
    public Viewport Limit(Viewport viewport, MRect? panExtent, MMinMax? zoomExtremes)
    {
        return LimitExtent(LimitResolution(viewport, zoomExtremes), panExtent);
    }

    private Viewport LimitResolution(Viewport viewport, MMinMax? zoomExtremes)
    {
        if (zoomExtremes is null) return viewport;

        if (zoomExtremes.Min > viewport.Resolution) return viewport with { Resolution = zoomExtremes.Min };
        if (zoomExtremes.Max < viewport.Resolution) return viewport with { Resolution = zoomExtremes.Max };

        return viewport;
    }

    private Viewport LimitExtent(Viewport viewport, MRect? panExtent)
    {
        if (panExtent is null) return viewport;

        var x = viewport.CenterX;
        if (viewport.CenterX < panExtent.Left) x = panExtent.Left;
        if (viewport.CenterX > panExtent.Right) x = panExtent.Right;

        var y = viewport.CenterY;
        if (viewport.CenterY > panExtent.Top) y = panExtent.Top;
        if (viewport.CenterY < panExtent.Bottom) y = panExtent.Bottom;

        return viewport with { CenterX = x, CenterY = y };
    }
}
