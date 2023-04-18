namespace Mapsui.Limiting;

public interface IViewportLimiter
{
    Viewport Limit(Viewport viewport, MRect? panBounds, MMinMax? zoomBounds);
}
