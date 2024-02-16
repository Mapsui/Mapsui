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

    public PinchManipulation GetPinchManipulation()
    {
        ArgumentNullException.ThrowIfNull(_pinchState);

        if (_previousPinchState is null)
            return new PinchManipulation(_pinchState.Center, _pinchState.Center, 1, 0, 0);
        
        var scaleChange = _pinchState.GetRadiusChange(_previousPinchState);
        var rotationChange = _pinchState.GetRotationChange(_previousPinchState);

        return new PinchManipulation(_pinchState.Center, _previousPinchState.Center, scaleChange, rotationChange, _totalRotationDelta);
    }

    private static PinchState? GetPinchState(List<MPoint> locations)
    {
        if (locations.Count == 0)
            return null;
        if (locations.Count == 1)
        {
            return new PinchState(locations[0], null, null);
        }

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
        if (pinchState is null)
            _totalRotationDelta = 0;

        if (pinchState is not null && _previousPinchState is not null)
            _totalRotationDelta += pinchState.GetRotationChange(_previousPinchState);
    }

    private record PinchState(MPoint Center, double? Radius, double? Angle)
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
