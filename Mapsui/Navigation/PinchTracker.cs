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
    public void Restart(PinchState pinchState)
    {
        _totalRotationDelta = 0; // Reset the total. It will incremented in each Pinch call
        _pinchState = pinchState;
        _previousPinchState = null;
    }

    public void Update(PinchState pinchState)
    {
        _previousPinchState = _pinchState;
        _pinchState = pinchState;
        if (_previousPinchState is not null)
            _totalRotationDelta += pinchState.Angle - _previousPinchState.Angle;
    }

    public PinchManipulation GetPinchManipulation()
    {
        ArgumentNullException.ThrowIfNull(_pinchState);

        if (_previousPinchState is null)
            return new PinchManipulation(_pinchState.Center, _pinchState.Center, 1, 0, 0);
        
        var rotationChange = _pinchState.Angle - _previousPinchState.Angle;
        var resolutionChange = _pinchState.Radius / _previousPinchState.Radius;

        return new PinchManipulation(_pinchState.Center, _previousPinchState.Center, resolutionChange, rotationChange, _totalRotationDelta);
    }

    public static PinchState GetPinchState(List<MPoint> locations)
    {
        if (locations.Count != 2)
            throw new ArgumentOutOfRangeException(nameof(locations), locations.Count, "Value should be two");

        double centerX = 0;
        double centerY = 0;

        foreach (var location in locations)
        {
            centerX += location.X;
            centerY += location.Y;
        }

        centerX /= locations.Count;
        centerY /= locations.Count;

        var radius = Algorithms.Distance(centerX, centerY, locations[0].X, locations[0].Y);

        var angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;

        return new PinchState(new MPoint(centerX, centerY), radius, angle);
    }
}
