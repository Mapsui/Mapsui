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
        if (ZoomMode == ZoomMode.Unlimited) return resolution;

        var resolutionExtremes = ZoomLimits ?? GetExtremes(mapResolutions);
        if (resolutionExtremes == null) return resolution;

        if (ZoomMode == ZoomMode.KeepWithinResolutions)
        {
            if (resolutionExtremes.Min > resolution) return resolutionExtremes.Min;
            if (resolutionExtremes.Max < resolution) return resolutionExtremes.Max;
        }

        return resolution;
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
        if (viewport.CenterX < maxExtent.Left) x = maxExtent.Left;
        if (viewport.CenterX > maxExtent.Right) x = maxExtent.Right;

        var y = viewport.CenterY;
        if (viewport.CenterY > maxExtent.Top) y = maxExtent.Top;
        if (viewport.CenterY < maxExtent.Bottom) y = maxExtent.Bottom;

        viewport.CenterX = x;
        viewport.CenterY = y;
    }
}
