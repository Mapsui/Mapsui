using Mapsui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.UI;

/// <summary>
/// This Viewport limiter will always keep the map within the zoom and pan limits.
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


    // Todo: GetExtremes should be used to set the PanLimits
    private MinMax? GetExtremes(IReadOnlyList<double>? resolutions)
    {
        if (resolutions == null || resolutions.Count == 0) return null;
        resolutions = resolutions.OrderByDescending(r => r).ToList();
        var mostZoomedOut = resolutions[0];
        var mostZoomedIn = resolutions[resolutions.Count - 1] * 0.5; // divide by two to allow one extra level to zoom-in
        return new MinMax(mostZoomedOut, mostZoomedIn);
    }

    public override ViewportState Limit(ViewportState viewportState)
    {
        var state = LimitResolution(viewportState);
        return LimitExtent(state);
    }

    private ViewportState LimitResolution(ViewportState viewportState)
    {
        if (ZoomLimits is null) return viewportState;
        if (PanLimits is null) return viewportState;

        if (ZoomLimits.Min > viewportState.Resolution) return viewportState with { Resolution = ZoomLimits.Min };

        var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(viewportState.Width, viewportState.Height, PanLimits);
        if (viewportFillingResolution < ZoomLimits.Min) return viewportState; // Mission impossible. Can't adhere to both restrictions
        var limit = Math.Min(ZoomLimits.Max, viewportFillingResolution);
        if (limit < viewportState.Resolution) return viewportState with { Resolution = limit };

        return viewportState;
    }

    private static double CalculateResolutionAtWhichMapFillsViewport(double screenWidth, double screenHeight, MRect mapEnvelope)
    {
        return Math.Min(mapEnvelope.Width / screenWidth, mapEnvelope.Height / screenHeight);
    }

    private ViewportState LimitExtent(ViewportState viewport)
    {
        if (PanLimits is null) return viewport;

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
