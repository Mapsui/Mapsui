using System;
using System.Collections.Generic;

namespace Mapsui;

public class TouchTracker
{
    private double _totalRotationDelta; // We need this to calculate snapping
    private TouchState? _touchState;
    private TouchState? _previousTouchState;

    /// <summary>
    /// Call this method before the first Update call. The Update method tracks the start touch angle which is needed 
    /// to for rotation snapping and the previous touch state.
    /// </summary>
    public void Restart(List<MPoint> touches) => Restart(GetTouchState(touches));

    public void Update(List<MPoint> touches) => Update(GetTouchState(touches));

    public TouchManipulation? GetTouchManipulation()
    {
        if (_touchState is null)
            return null;

        if (_previousTouchState is null)
            return null; // There is a touch but no previous touch so no manipulation.

        var scaleChange = _touchState.GetRadiusChange(_previousTouchState);
        var rotationChange = _touchState.GetRotationChange(_previousTouchState);

        if (_touchState.Equals(_previousTouchState))
            return null; // The default will not change anything so don't return a manipulation.

        return new TouchManipulation(_touchState.Center, _previousTouchState.Center, scaleChange, rotationChange, _totalRotationDelta);
    }

    private static TouchState? GetTouchState(List<MPoint> touches)
    {
        if (touches.Count == 0)
            return null;

        if (touches.Count == 1)
            return new TouchState(touches[0], null, null, touches.Count);
        
        var (centerX, centerY) = GetCenter(touches);
        var radius = Distance(centerX, centerY, touches[0].X, touches[0].Y);
        var angle = Math.Atan2(touches[1].Y - touches[0].Y, touches[1].X - touches[0].X) * 180.0 / Math.PI;

        return new TouchState(new MPoint(centerX, centerY), radius, angle, touches.Count);
    }

    private static double Distance(double x1, double y1, double x2, double y2) 
        => Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));

    private static (double centerX, double centerY) GetCenter(List<MPoint> touches)
    {
        double centerX = 0;
        double centerY = 0;

        foreach (var location in touches)
        {
            centerX += location.X;
            centerY += location.Y;
        }

        centerX /= touches.Count;
        centerY /= touches.Count;

        return (centerX, centerY);
    }

    private void Restart(TouchState? touchState)
    {
        _totalRotationDelta = 0; // Reset the total. It will incremented in each Update call
        _touchState = touchState;
        _previousTouchState = null;
    }

    private void Update(TouchState? touchState)
    {
        _previousTouchState = _touchState;
        _touchState = touchState;

        if (!(touchState?.FingerCount == _previousTouchState?.FingerCount))
        {
            // If the finger count changes this is considered a reset.
            _totalRotationDelta = 0;
            _previousTouchState = null;
            // Note, there is the unlikely change that one finger is lifted exactly when 
            // another is touched down. This should also be ignored, but we can only
            // do that if we had the touch ids. We accept this problem. It will not crash the system.
            return;
        }

        if (touchState is null)
            _totalRotationDelta = 0;

        if (touchState is not null && _previousTouchState is not null)
            _totalRotationDelta += touchState.GetRotationChange(_previousTouchState);
    }

    private record TouchState(MPoint Center, double? Radius, double? Angle, int FingerCount)
    {
        public double GetRotationChange(TouchState previousTouchState)
        {
            if (Angle is null)
                return 0;
            if (previousTouchState.Angle is null)
                return 0;
            return Angle.Value - previousTouchState.Angle.Value;
        }

        public double GetRadiusChange(TouchState previousTouchState)
        {
            if (Radius is null)
                return 1;
            if (previousTouchState.Radius is null)
                return 1;
            return Radius.Value / previousTouchState.Radius.Value;
        }
    }
}

public record TouchManipulation(MPoint Center, MPoint PreviousCenter, double ResolutionChange, double RotationChange, double TotalRotationChange);
