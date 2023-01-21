using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.UI;

/// <summary>
/// This Viewport limiter will always keep the map within the zoom and pan limits.
/// An exception is rotation. 
/// </summary>
public class ViewportLimiterKeepWithin : IViewportLimiter
{
    // todo: Check validity of the PanLimits and ZoomLimits.
    // It is possible to specify a combination of PanLimits and ZoomLimits that is 
    // impossible to apply. If the lowest allowed resolution does not fill the 
    // screen it can never be kept within the extent. Weird jumpy map behavior will
    // be the result of this. In the history of this file there is a MapWidthSpansViewport
    // method that might be helpful. 

    /// <summary>
    /// Sets the limit to which the user can pan the map.
    /// If PanLimits is not set, Map.Extent will be used as restricted extent.
    /// </summary>
    public MRect? PanLimits { get; set; }

    /// <summary>
    /// Pair of the limits for the resolutions (smallest and biggest). The resolution is kept 
    /// between these values.
    /// </summary>
    public MinMax? ZoomLimits { get; set; }

    private MinMax? GetExtremes(IReadOnlyList<double>? resolutions)
    {
        if (resolutions == null || resolutions.Count == 0) return null;
        resolutions = resolutions.OrderByDescending(r => r).ToList();
        var mostZoomedOut = resolutions[0];
        var mostZoomedIn = resolutions[resolutions.Count - 1] * 0.5; // divide by two to allow one extra level to zoom-in
        return new MinMax(mostZoomedOut, mostZoomedIn);
    }

    public void Limit(Viewport viewport, IReadOnlyList<double> mapResolutions, MRect? mapEnvelope)
    {
        viewport.SetResolution(LimitResolution(viewport.Resolution, viewport.Width, viewport.Height, mapResolutions, mapEnvelope));
        LimitExtent(viewport, mapEnvelope);
    }

    public double LimitResolution(double resolution, double screenWidth, double screenHeight,
        IReadOnlyList<double> mapResolutions, MRect? mapEnvelope)
    {
        var zoomLimits = ZoomLimits ?? GetExtremes(mapResolutions);
        if (zoomLimits == null) return resolution;

        var panLimit = PanLimits ?? mapEnvelope;
        if (panLimit == null) return resolution;

        if (zoomLimits.Min > resolution) return zoomLimits.Min;

        var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(screenWidth, screenHeight, panLimit);
        if (viewportFillingResolution < zoomLimits.Min) return resolution; // Mission impossible. Can't adhere to both restrictions
        var limit = Math.Min(zoomLimits.Max, viewportFillingResolution);
        if (limit < resolution) return limit;

        return resolution;
    }

    private static double CalculateResolutionAtWhichMapFillsViewport(double screenWidth, double screenHeight, MRect mapEnvelope)
    {
        return Math.Min(mapEnvelope.Width / screenWidth, mapEnvelope.Height / screenHeight);
    }

    public void LimitExtent(Viewport viewport, MRect? mapEnvelope)
    {
        var maxExtent = PanLimits ?? mapEnvelope;
        if (maxExtent == null)
        {
            // Can be null because both panLimits and Map.Extent can be null. 
            // The Map.Extent can be null if the extent of all layers is null
            return;
        }

        var x = viewport.CenterX;

        if (viewport.Extent?.Left < maxExtent.Left)
            x += maxExtent.Left - viewport.Extent.Left;
        else if (viewport.Extent?.Right > maxExtent.Right)
            x += maxExtent.Right - viewport.Extent.Right;

        var y = viewport.CenterY;

        if (viewport.Extent?.Top > maxExtent.Top)
            y += maxExtent.Top - viewport.Extent.Top;
        else if (viewport.Extent?.Bottom < maxExtent.Bottom)
            y += maxExtent.Bottom - viewport.Extent.Bottom;

        viewport.SetCenter(x, y);
    }
}
