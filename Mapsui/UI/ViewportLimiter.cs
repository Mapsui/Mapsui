using System.Collections.Generic;
using System.Linq;

namespace Mapsui.UI;

public class MinMax
{
    public MinMax(double value1, double value2)
    {
        if (value1 < value2)
        {
            Min = value1;
            Max = value2;
        }
        else
        {
            Min = value2;
            Max = value1;
        }
    }

    public double Min { get; }
    public double Max { get; }
}

public enum ZoomMode
{
    /// <summary>
    /// Restricts zoom in no way
    /// </summary>
    Unlimited,
    /// <summary>
    /// Restricts zoom of the viewport to ZoomLimits and, if ZoomLimits isn't 
    /// set, to minimum and maximum of Resolutions
    /// </summary>
    KeepWithinResolutions,
}

public class ViewportLimiter : IViewportLimiter
{
    /// <summary>
    /// Zoom mode to use, when map is zoomed
    /// </summary>
    public ZoomMode ZoomMode { get; set; } = ZoomMode.KeepWithinResolutions;

    /// <summary>
    /// Sets the limit to which the user can pan the map.
    /// If PanLimits is not set, Map.Extent will be used as restricted extent.
    /// </summary>
    public MRect? PanLimits { get; set; }

    /// <summary>
    /// Pair of the limits for the resolutions (smallest and biggest). If ZoomMode is set 
    /// to anything else than None, resolution is kept between these values.
    /// </summary>
    public MinMax? ZoomLimits { get; set; }

    private MinMax? GetExtremes(IReadOnlyList<double>? resolutions)
    {
        if (resolutions == null || resolutions.Count == 0) return null;
        resolutions = resolutions.OrderByDescending(r => r).ToList();
        var mostZoomedOut = resolutions[0];
        var mostZoomedIn = resolutions[resolutions.Count - 1] * 0.5; // Divide by two to allow one extra level to zoom-in
        return new MinMax(mostZoomedOut, mostZoomedIn);
    }

    public ViewportState Limit(ViewportState viewportState, IReadOnlyList<double> mapResolutions, MRect? mapEnvelope)
    {
        var state = LimitResolution(viewportState, viewportState.Width, viewportState.Height, mapResolutions, mapEnvelope);
        return LimitExtent(state, mapEnvelope);
    }

    public ViewportState LimitResolution(ViewportState viewportState, double screenWidth, double screenHeight,
        IReadOnlyList<double> mapResolutions, MRect? mapEnvelope)
    {
        if (ZoomMode == ZoomMode.Unlimited) return viewportState;

        var resolutionExtremes = ZoomLimits ?? GetExtremes(mapResolutions);
        if (resolutionExtremes == null) return viewportState;

        if (ZoomMode == ZoomMode.KeepWithinResolutions)
        {
            if (resolutionExtremes.Min > viewportState.Resolution) return viewportState with { Resolution = resolutionExtremes.Min };
            if (resolutionExtremes.Max < viewportState.Resolution) return viewportState with { Resolution = resolutionExtremes.Max };
        }

        return viewportState;
    }

    public ViewportState LimitExtent(ViewportState viewportState, MRect? mapEnvelope)
    {
        var maxExtent = PanLimits ?? mapEnvelope;
        if (maxExtent == null)
        {
            // Can be null because both panLimits and Map.Extent can be null. 
            // The Map.Extent can be null if the extent of all layers is null.
            return viewportState;
        }

        var x = viewportState.CenterX;
        if (viewportState.CenterX < maxExtent.Left) x = maxExtent.Left;
        if (viewportState.CenterX > maxExtent.Right) x = maxExtent.Right;

        var y = viewportState.CenterY;
        if (viewportState.CenterY > maxExtent.Top) y = maxExtent.Top;
        if (viewportState.CenterY < maxExtent.Bottom) y = maxExtent.Bottom;

        return viewportState with { CenterX = x, CenterY = y };
    }
}
