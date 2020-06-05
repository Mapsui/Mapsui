using Mapsui.Utilities;
using System;

namespace Mapsui.UI.Wpf
{
    public class MouseWheelAnimation
    {
        private int _tickCount;
        private double _toResolution;
        public int Duration { get; set; } = 1000;

        public double GetTargetResolution(int delta, IViewport viewport, IMap map)
        {
            // If the animation has ended then start from the current resolution.
            // The alternative is that use the previous resolution target and add an extra
            // level to that.
            if (!DuringMouseWheelAnimation())
                _toResolution = viewport.Resolution;

            if (delta > Constants.Epsilon)
            {
                _toResolution = ZoomHelper.ZoomIn(map.Resolutions, _toResolution);
            }
            else if (delta < Constants.Epsilon)
            {
                _toResolution = ZoomHelper.ZoomOut(map.Resolutions, _toResolution);
            }

            // TickCount is fast https://stackoverflow.com/a/4075602/85325
            _tickCount = Environment.TickCount;

            return _toResolution;
        }

        public bool DuringMouseWheelAnimation()
        {
            return (Environment.TickCount - _tickCount) < Duration;
        }
    }
}
