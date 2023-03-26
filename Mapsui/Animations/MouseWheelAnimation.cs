using System;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.Animations;

public class MouseWheelAnimation
{
    private int _tickCount = int.MinValue;
    private double _toResolution;

    public int Duration { get; set; } = 600;
    public Easing Easing { get; set; } = Easing.QuarticOut;

    public double GetResolution(int mouseWheelDelta, Navigator navigator, IReadOnlyList<double> resolutions)
    {
        // If the animation has ended then start from the current resolution.
        // The alternative is to use the previous resolution target and add an extra
        // level to that.
        if (!IsAnimating())
            _toResolution = navigator.Viewport.Resolution;

        if (mouseWheelDelta > Constants.Epsilon)
        {
            _toResolution = ZoomHelper.ZoomIn(resolutions, _toResolution);
            // Todo: Move this to ZoomIn. Make limiting consistent.
            if (navigator.ZoomExtremes is not null)
                _toResolution = Math.Max(_toResolution, navigator.ZoomExtremes.Min);
        }
        else if (mouseWheelDelta < Constants.Epsilon)
        {
            _toResolution = ZoomHelper.ZoomOut(resolutions, _toResolution);
            // Todo: Move this to ZoomOut. Make limiting consistent.
            if (navigator.ZoomExtremes is not null)
                _toResolution = Math.Min(_toResolution, navigator.ZoomExtremes.Max);
        }

        // TickCount is fast https://stackoverflow.com/a/4075602/85325
        _tickCount = Environment.TickCount;

        return _toResolution;
    }

    private bool IsAnimating()
    {
        var tickProgress = Environment.TickCount - _tickCount;
        return tickProgress >= 0 && tickProgress < Duration;
    }
}
