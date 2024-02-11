using System;

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
}
