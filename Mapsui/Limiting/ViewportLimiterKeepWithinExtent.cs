using Mapsui.Extensions;
using Mapsui.Logging;
using System;

namespace Mapsui.Limiting;

/// <summary>
/// This Viewport limiter will always keep the visible map within the zoom and pan limits.
/// It should not be possible to view anything outside the pan limits.
/// An exception is rotation. 
/// </summary>
public class ViewportLimiterKeepWithinExtent : BaseViewportLimiter
{
    public override Viewport Limit(Viewport viewport, MRect? panExtent, MMinMax? zoomExtremes)
    {
        var state = LimitResolution(viewport, zoomExtremes);
        return LimitExtent(state, panExtent, zoomExtremes);
    }

    private Viewport LimitResolution(Viewport viewport, MMinMax? zoomExtremes)
    {
        if (zoomExtremes is null) return viewport;

        if (zoomExtremes.Min > viewport.Resolution) return viewport with { Resolution = zoomExtremes.Min };
        if (zoomExtremes.Max < viewport.Resolution) return viewport with { Resolution = zoomExtremes.Max };

        return viewport;
    }

    private static double CalculateResolutionAtWhichMapFillsViewport(double screenWidth, double screenHeight, MRect mapEnvelope)
    {
        return Math.Min(mapEnvelope.Width / screenWidth, mapEnvelope.Height / screenHeight);
    }

    private Viewport LimitExtent(Viewport viewport, MRect? panExtent, MMinMax? zoomExtremes)
    {
        if (panExtent is null) return viewport;

        // Below we limit the resolution. Why is this part of LimitExtent?
        // This is because it is impossible to limit the map to a certain extent
        // at the more zoomed-out resolutions. If you can see the entire world it is 
        // not possible to limit the extents to an individual country. So here
        // we limit the resolution to a level which could cover the entire extent.
        var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(viewport.Width, viewport.Height, panExtent);
        if (viewportFillingResolution < zoomExtremes?.Min == true)
        {
            Logger.Log(LogLevel.Error, "Error in limiter configuration. The minimum zoomlevel does not cover the entire extent");
        }
        if (viewportFillingResolution < viewport.Resolution)
            viewport = viewport with { Resolution = viewportFillingResolution };

        var extent = viewport.ToExtent();

        var x = viewport.CenterX;

        if (extent.Left < panExtent.Left)
            x += panExtent.Left - extent.Left;
        else if (extent?.Right > panExtent.Right)
            x += panExtent.Right - extent.Right;

        var y = viewport.CenterY;

        if (extent?.Top > panExtent.Top)
            y += panExtent.Top - extent.Top;
        else if (extent?.Bottom < panExtent.Bottom)
            y += panExtent.Bottom - extent.Bottom;

        return viewport with { CenterX = x, CenterY = y };
    }
}
