using Mapsui.Extensions;
using Mapsui.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Limiting;

/// <summary>
/// This Viewport limiter will always keep the visible map within the zoom and pan limits.
/// It should not be possible to view anything outside the pan limits.
/// An exception is rotation. 
/// </summary>
public class ViewportLimiterKeepWithin : BaseViewportLimiter
{
    // todo: Check validity of the PanLimits and ZoomLimits.
    // It is possible to specify a combination of PanLimits and ZoomLimits that is 
    // impossible to apply. If the lowest allowed resolution does not fill the 
    // screen it can never be kept within the extent. Weird jumpy map behavior will
    // be the result of this. In the history of this file there is a MapWidthSpansViewport
    // method that might be helpful. 

    public override ViewportState Limit(ViewportState viewportState)
    {
        var state = LimitResolution(viewportState);
        return LimitExtent(state);
    }

    private ViewportState LimitResolution(ViewportState viewportState)
    {
        if (ZoomLimits is null) return viewportState;

        if (ZoomLimits.Min > viewportState.Resolution) return viewportState with { Resolution = ZoomLimits.Min };
        if (ZoomLimits.Max < viewportState.Resolution) return viewportState with { Resolution = ZoomLimits.Max };

        return viewportState;
    }

    private static double CalculateResolutionAtWhichMapFillsViewport(double screenWidth, double screenHeight, MRect mapEnvelope)
    {
        return Math.Min(mapEnvelope.Width / screenWidth, mapEnvelope.Height / screenHeight);
    }

    private ViewportState LimitExtent(ViewportState viewport)
    {
        if (PanLimits is null) return viewport;

        // Below we limit the resolution. Why is this part of LimitExtent?
        // This is because it is impossible to limit the map to a certain extent
        // at the more zoomed-out resolutions. If you can see the entire world it is 
        // not possible to limit the extents to an individual country. So here
        // we limit the resolution to a level which could cover the entire extent.
        var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(viewport.Width, viewport.Height, PanLimits);
        if (viewportFillingResolution < ZoomLimits?.Min == true)
        {
            Logger.Log(LogLevel.Error, "Error in limiter configuration. The minimum zoomlevel does not cover the entire extent");
        }
        if (viewportFillingResolution < viewport.Resolution)
            viewport = viewport with { Resolution = viewportFillingResolution };

        var extent = viewport.ToExtent();

        var x = viewport.CenterX;

        if (extent.Left < PanLimits.Left)
            x += PanLimits.Left - extent.Left;
        else if (extent?.Right > PanLimits.Right)
            x += PanLimits.Right - extent.Right;

        var y = viewport.CenterY;

        if (extent?.Top > PanLimits.Top)
            y += PanLimits.Top - extent.Top;
        else if (extent?.Bottom < PanLimits.Bottom)
            y += PanLimits.Bottom - extent.Bottom;

        return viewport with { CenterX = x, CenterY = y };
    }
}
