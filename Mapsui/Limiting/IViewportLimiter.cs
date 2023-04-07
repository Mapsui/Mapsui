namespace Mapsui.Limiting;

public interface IViewportLimiter
{
    Viewport Limit(Viewport viewport, MRect? panExtent, MMinMax? zoomExtremes);
}
