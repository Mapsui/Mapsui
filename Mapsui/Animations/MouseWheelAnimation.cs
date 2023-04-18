using System;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.Animations;

public class MouseWheelAnimation
{
    private int _tickCount = int.MinValue;
    private double _destinationResolution;

    public int Duration { get; set; } = 600;
    public Easing Easing { get; set; } = Easing.QuarticOut;

    public double GetResolution(int mouseWheelDelta, double currentResolution, MMinMax? zoomBounds, IReadOnlyList<double> resolutions)
    {
        // If an animation is already running don't start from the current resolution, but from the 
        // destination resolution of the previous animation. This way the consequetive mouse wheel
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
        _tickCount = Environment.TickCount;

        return _destinationResolution;
    }

    private bool IsAnimating()
    {
        var tickProgress = Environment.TickCount - _tickCount;
        return tickProgress >= 0 && tickProgress < Duration;
    }
}
