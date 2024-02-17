using Mapsui.Utilities;
using System;
using System.Collections.Generic;

namespace Mapsui;

public class PinchTracker
{
    private double _totalRotationDelta; // We need this to calculate snapping
    private PinchState? _pinchState;
    private PinchState? _previousPinchState;

    /// <summary>
    /// Call this method before the first Pinch call. The Pinch method tracks the start pinch angle which is needed 
    /// to for rotation snapping and the previous pinch state.
    /// </summary>
    public void Restart(List<MPoint> touches) => Restart(GetPinchState(touches));

    public void Update(List<MPoint> touches) => Update(GetPinchState(touches));

    public PinchManipulation? GetPinchManipulation()
    {
        if (_pinchState is null)
            return null;

        if (_previousPinchState is null)
            return null; // There is a touch but no previous touch so no manipulation.

        var scaleChange = _pinchState.GetRadiusChange(_previousPinchState);
        var rotationChange = _pinchState.GetRotationChange(_previousPinchState);

        return new PinchManipulation(_pinchState.Center, _previousPinchState.Center, scaleChange, rotationChange, _totalRotationDelta);
    }

    private static PinchState? GetPinchState(List<MPoint> touches)
    {
        if (touches.Count == 0)
            return null;
        if (touches.Count == 1)
        {
            return new PinchState(touches[0], null, null, touches);
        }

        double centerX = 0;
        double centerY = 0;

        foreach (var location in touches)
        {
            centerX += location.X;
            centerY += location.Y;
        }

        centerX /= touches.Count;
        centerY /= touches.Count;

        var radius = Algorithms.Distance(centerX, centerY, touches[0].X, touches[0].Y);

        var angle = Math.Atan2(touches[1].Y - touches[0].Y, touches[1].X - touches[0].X) * 180.0 / Math.PI;

        return new PinchState(new MPoint(centerX, centerY), radius, angle, touches);
    }

    private void Restart(PinchState? pinchState)
    {
        _totalRotationDelta = 0; // Reset the total. It will incremented in each Pinch call
        _pinchState = pinchState;
        _previousPinchState = null;
    }

    private void Update(PinchState? pinchState)
    {
        _previousPinchState = _pinchState;
        _pinchState = pinchState;

        if (!(pinchState?.Touches.Count == _previousPinchState?.Touches.Count))
        {
            // If the finger count changes this is considered a reset.
            _totalRotationDelta = 0;
            _previousPinchState = null;
            // Note, there is the unlikely change that one finger is lifted exactly when 
            // another is touched down. This should also be ignored, but we can only
            // do that if we had the touch ids. We accept this problem. It will not crash the system.
            return;
        }

        if (pinchState is null)
            _totalRotationDelta = 0;

        if (pinchState is not null && _previousPinchState is not null)
            _totalRotationDelta += pinchState.GetRotationChange(_previousPinchState);
    }

    private record PinchState(MPoint Center, double? Radius, double? Angle, List<MPoint> Touches)
    {
        public PinchManipulation GetPinchManipulation(PinchState previousPinchState, double totalPinchRotation)
            => new PinchManipulation(Center, previousPinchState.Center, GetRadiusChange(previousPinchState), GetRotationChange(previousPinchState), totalPinchRotation);

        public double GetRotationChange(PinchState previousPinchState)
        {
            if (Angle is null)
                return 0;
            if (previousPinchState.Angle is null)
                return 0;
            return Angle.Value - previousPinchState.Angle.Value;
        }

        public double GetRadiusChange(PinchState previousPinchState)
        {
            if (Radius is null)
                return 1;
            if (previousPinchState.Radius is null)
                return 1;
            return Radius.Value / previousPinchState.Radius.Value;
        }
    }
}
