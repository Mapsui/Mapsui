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

public class ViewportLimiter : BaseViewportLimiter
{
    /// <summary>
    /// Zoom mode to use, when map is zoomed
    /// </summary>
    public ZoomMode ZoomMode { get; set; } = ZoomMode.KeepWithinResolutions;

    public override ViewportState Limit(ViewportState viewportState)
    {
        var state = LimitResolution(viewportState);
        return LimitExtent(state);
    }

    private ViewportState LimitResolution(ViewportState viewportState)
    {
        if (ZoomMode == ZoomMode.Unlimited) return viewportState;
        if (ZoomLimits is null) return viewportState;

        if (ZoomMode == ZoomMode.KeepWithinResolutions)
        {
            if (ZoomLimits.Min > viewportState.Resolution) return viewportState with { Resolution = ZoomLimits.Min };
            if (ZoomLimits.Max < viewportState.Resolution) return viewportState with { Resolution = ZoomLimits.Max };
        }

        return viewportState;
    }

    private ViewportState LimitExtent(ViewportState viewportState)
    {
        if (PanLimits is null) return viewportState;

        var x = viewportState.CenterX;
        if (viewportState.CenterX < PanLimits.Left) x = PanLimits.Left;
        if (viewportState.CenterX > PanLimits.Right) x = PanLimits.Right;

        var y = viewportState.CenterY;
        if (viewportState.CenterY > PanLimits.Top) y = PanLimits.Top;
        if (viewportState.CenterY < PanLimits.Bottom) y = PanLimits.Bottom;

        return viewportState with { CenterX = x, CenterY = y };
    }
}
