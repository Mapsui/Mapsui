using System;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.Animations;

public class MouseWheelAnimation
{
    private long _tickCount = long.MinValue;
    private double _destinationResolution;

    public int Duration { get; set; } = 600;
    public Easing Easing { get; set; } = Easing.QuarticOut;

    /// <summary>
    /// When true, each mouse wheel event zooms by a small continuous step instead of snapping
    /// to the next predefined resolution level. This gives a smoother feel, especially on
    /// platforms where the mouse wheel fires many events (e.g. trackpad pinch on a laptop).
    /// </summary>
    public bool UseContinuousMouseWheelZoom { get; set; } = false;

    /// <summary>
    /// The zoom step applied per wheel event when <see cref="UseContinuousMouseWheelZoom"/> is true.
    /// The value is an exponent in base-2: a step of 0.1 scales the resolution by 2^0.1 ≈ 1.07 (7%).
    /// A step of 1.0 would halve or double the resolution in a single event. Default is 0.1.
    /// </summary>
    public double ContinuousMouseWheelZoomStepSize { get; set; } = 0.1;

    public double GetResolution(int mouseWheelDelta, double currentResolution, MMinMax? zoomBounds, IReadOnlyList<double> resolutions)
    {
        // If an animation is already running don't start from the current resolution, but from the 
        // destination resolution of the previous animation. This way the consecutive mouse wheel
        // ticks add up which allows for fast zooming to a detailed level.
        if (IsAnimating())
            currentResolution = _destinationResolution;

        if (mouseWheelDelta > Constants.Epsilon)
        {
            _destinationResolution = ZoomHelper.GetResolutionToZoomIn(resolutions, currentResolution);
            if (zoomBounds is not null)
                _destinationResolution = Math.Max(_destinationResolution, zoomBounds.Min);
        }
        else if (mouseWheelDelta < Constants.Epsilon)
        {
            _destinationResolution = ZoomHelper.GetResolutionToZoomOut(resolutions, currentResolution);
            if (zoomBounds is not null)
                _destinationResolution = Math.Min(_destinationResolution, zoomBounds.Max);
        }

        // TickCount is fast https://stackoverflow.com/a/4075602/85325
        _tickCount = Environment.TickCount64;

        return _destinationResolution;
    }

    private bool IsAnimating()
    {
        var tickProgress = Environment.TickCount64 - _tickCount;
        return tickProgress >= 0 && tickProgress < Duration;
    }
}
