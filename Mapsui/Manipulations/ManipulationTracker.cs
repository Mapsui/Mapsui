using System;

namespace Mapsui.Manipulations;

public class TouchTracker
{
    private double _totalRotationChange; // We need this to calculate snapping
    private TouchState? _touchState;
    private TouchState? _previousTouchState;

    /// <summary>
    /// Call this method before the first Update call. The Update method tracks the start touch angle which is needed 
    /// to for rotation snapping and the previous touch state.
    /// </summary>
    public void Restart(ReadOnlySpan<MPoint> touchLocations) => Restart(GetTouchState(touchLocations));

    public void Manipulate(ReadOnlySpan<MPoint> touchLocations, Action<Manipulation> onManipulation) => Manipulate(GetTouchState(touchLocations), onManipulation);

    private Manipulation? GetManipulation()
    {
        if (_touchState is null)
            return null;

        if (_previousTouchState is null)
            return null; // There is a touch but no previous touch so no manipulation.

        var scaleFactor = _touchState.GetScaleFactor(_previousTouchState);
        var rotationChange = _touchState.GetRotationChange(_previousTouchState);

        if (_touchState.Equals(_previousTouchState))
            return null; // The default will not change anything so don't return a manipulation.

        return new Manipulation(_touchState.Center, _previousTouchState.Center, scaleFactor, rotationChange, _totalRotationChange);
    }

    private static TouchState? GetTouchState(ReadOnlySpan<MPoint> touchLocations)
    {
        if (touchLocations.Length == 0)
            return null;

        if (touchLocations.Length == 1)
            return new TouchState(touchLocations[0], null, null, touchLocations.Length);

        var (centerX, centerY) = GetCenter(touchLocations);
        var radius = Distance(centerX, centerY, touchLocations[0].X, touchLocations[0].Y);
        var angle = Math.Atan2(touchLocations[1].Y - touchLocations[0].Y, touchLocations[1].X - touchLocations[0].X) * 180.0 / Math.PI;

        return new TouchState(new MPoint(centerX, centerY), radius, angle, touchLocations.Length);
    }

    private static double Distance(double x1, double y1, double x2, double y2)
        => Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));

    private static (double centerX, double centerY) GetCenter(ReadOnlySpan<MPoint> touches)
    {
        double centerX = 0;
        double centerY = 0;

        foreach (var location in touches)
        {
            centerX += location.X;
            centerY += location.Y;
        }

        centerX /= touches.Length;
        centerY /= touches.Length;

        return (centerX, centerY);
    }

    private void Restart(TouchState? touchState)
    {
        _totalRotationChange = 0; // Reset the total. It will incremented in each Update call
        _touchState = touchState;
        _previousTouchState = null;
    }

    private void Manipulate(TouchState? touchState, Action<Manipulation> onManipulation)
    {
        _previousTouchState = _touchState;
        _touchState = touchState;

        if (!(touchState?.TouchLocationsLength == _previousTouchState?.TouchLocationsLength))
        {
            // If the finger count changes this is considered a reset.
            _totalRotationChange = 0;
            _previousTouchState = null;
            // Note, there is the unlikely change that one finger is lifted exactly when 
            // another is touched down. This should also be ignored, but we can only
            // do that if we had the touch ids. We accept this problem. It will not crash the system.
            return;
        }

        if (touchState is null)
            _totalRotationChange = 0;

        if (touchState is not null && _previousTouchState is not null)
            _totalRotationChange += touchState.GetRotationChange(_previousTouchState);

        var manipulation = GetManipulation();
        if (manipulation is not null)
            onManipulation(manipulation);
    }

    private record TouchState(MPoint Center, double? Radius, double? Angle, int TouchLocationsLength)
    {
        public double GetRotationChange(TouchState previousTouchState)
        {
            if (Angle is null)
                return 0;
            if (previousTouchState.Angle is null)
                return 0;
            return Angle.Value - previousTouchState.Angle.Value;
        }

        public double GetScaleFactor(TouchState previousTouchState)
        {
            if (Radius is null)
                return 1;
            if (previousTouchState.Radius is null)
                return 1;
            return Radius.Value / previousTouchState.Radius.Value;
        }
    }
}

public record Manipulation(MPoint Center, MPoint PreviousCenter, double ScaleFactor, double RotationChange, double TotalRotationChange);
