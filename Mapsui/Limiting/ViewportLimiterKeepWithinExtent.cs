using Mapsui.Extensions;
using Mapsui.Logging;
using System;

namespace Mapsui.Limiting;

/// <summary>
/// This Viewport limiter will make sure that only the area within the panBounds will be visible in the viewport.
/// It should not be possible to view anything outside of the panBounds.
/// An exception is rotation. 
/// </summary>
public class ViewportLimiterKeepWithinExtent : IViewportLimiter
{
    public Viewport Limit(Viewport viewport, MRect? panBounds, MMinMax? zoomBounds)
    {
        if (viewport.IsRotated())
        {
            Logger.Log(LogLevel.Warning, "ViewportLimiterKeepWithinExtent does not support rotation.");
        }
        return LimitExtent(LimitResolution(viewport, zoomBounds), panBounds, zoomBounds);
    }

    private Viewport LimitResolution(Viewport viewport, MMinMax? zoomBounds)
    {
        if (zoomBounds is null) return viewport;

        if (zoomBounds.Min > viewport.Resolution) return viewport with { Resolution = zoomBounds.Min };
        if (zoomBounds.Max < viewport.Resolution) return viewport with { Resolution = zoomBounds.Max };

        return viewport;
    }

    private Viewport LimitExtent(Viewport viewport, MRect? panBounds, MMinMax? zoomBounds)
    {
        if (panBounds is null) return viewport;

        // Below we limit the resolution. Why is this part of LimitExtent?
        // This is because it is impossible to limit the map to a certain extent
        // at the more zoomed-out resolutions. If you can see the entire world it is 
        // not possible to limit the extents to an individual country. So here
        // we limit the resolution to a level which could cover the entire extent.
        var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(viewport.Width, viewport.Height, panBounds);
        if (viewportFillingResolution < zoomBounds?.Min == true)
        {
            Logger.Log(LogLevel.Error, "Error in limiter configuration. The minimum zoomlevel does not cover the entire extent");
        }
        if (viewportFillingResolution < viewport.Resolution)
            viewport = viewport with { Resolution = viewportFillingResolution };

        var extent = viewport.ToExtent();

        var x = viewport.CenterX;

        if (extent.Left < panBounds.Left)
            x += panBounds.Left - extent.Left;
        else if (extent?.Right > panBounds.Right)
            x += panBounds.Right - extent.Right;

        var y = viewport.CenterY;

        if (extent?.Top > panBounds.Top)
            y += panBounds.Top - extent.Top;
        else if (extent?.Bottom < panBounds.Bottom)
            y += panBounds.Bottom - extent.Bottom;

        return viewport with { CenterX = x, CenterY = y };
    }

    private static double CalculateResolutionAtWhichMapFillsViewport(double screenWidth, double screenHeight, MRect panBounds)
    {
        // This method does not take rotation into account. This is possible and could be implemented at some point.
        return Math.Min(panBounds.Width / screenWidth, panBounds.Height / screenHeight);
    }
}
