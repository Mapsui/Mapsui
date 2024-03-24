using System;

namespace Mapsui.Manipulations;

public class ManipulationTracker
{
    private double _totalRotationChange; // We need this to calculate snapping
    private ManipulationState? _manipulationState;
    private ManipulationState? _previousManipulationState;

    /// <summary>
    /// Call this method before the first Update call. The Update method tracks the start manipulation angle which is needed 
    /// to for rotation snapping and the previous manipulation state.
    /// </summary>
    public void Restart(ReadOnlySpan<ScreenPosition> positions) => Restart(GetManipulationState(positions));

    public void Manipulate(ReadOnlySpan<ScreenPosition> positions, Action<Manipulation> onManipulation)
        => Manipulate(GetManipulationState(positions), onManipulation);

    private Manipulation? GetManipulation()
    {
        if (_manipulationState is null)
            return null;

        if (_previousManipulationState is null)
            return null; // If there is no previous manipulation there is no way to calculate the new manipulation state.

        var scaleFactor = _manipulationState.GetScaleFactor(_previousManipulationState);
        var rotationChange = _manipulationState.GetRotationChange(_previousManipulationState);

        if (_manipulationState.Equals(_previousManipulationState))
            return null; // The default will not change anything so don't return a manipulation.

        return new Manipulation(_manipulationState.Center, _previousManipulationState.Center, scaleFactor, rotationChange, _totalRotationChange);
    }

    private static ManipulationState? GetManipulationState(ReadOnlySpan<ScreenPosition> positions)
    {
        if (positions.Length == 0)
            return null;

        if (positions.Length == 1)
            return new ManipulationState(positions[0], null, null, positions.Length);

        var (centerX, centerY) = GetCenter(positions);
        var radius = Distance(centerX, centerY, positions[0].X, positions[0].Y);
        var angle = Math.Atan2(positions[1].Y - positions[0].Y, positions[1].X - positions[0].X) * 180.0 / Math.PI;

        return new ManipulationState(new ScreenPosition(centerX, centerY), radius, angle, positions.Length);
    }

    private static double Distance(double x1, double y1, double x2, double y2)
        => Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));

    private static (double centerX, double centerY) GetCenter(ReadOnlySpan<ScreenPosition> positions)
    {
        double centerX = 0;
        double centerY = 0;

        foreach (var location in positions)
        {
            centerX += location.X;
            centerY += location.Y;
        }

        centerX /= positions.Length;
        centerY /= positions.Length;

        return (centerX, centerY);
    }

    private void Restart(ManipulationState? manipulationState)
    {
        _totalRotationChange = 0; // Reset the total. It will incremented in each Update call
        _manipulationState = manipulationState;
        _previousManipulationState = null;
    }

    private void Manipulate(ManipulationState? manipulationState, Action<Manipulation> onManipulation)
    {
        _previousManipulationState = _manipulationState;
        _manipulationState = manipulationState;

        if (!(manipulationState?.LocationsLength == _previousManipulationState?.LocationsLength))
        {
            // If the finger count changes this is considered a reset.
            _totalRotationChange = 0;
            _previousManipulationState = null;
            // Note, there is the unlikely probability that one finger is lifted exactly when 
            // another is touched down. This should also be ignored, but we can only
            // do that if we had the touch ids. We accept this problem. It will not crash the system.
            return;
        }

        if (manipulationState is null)
            _totalRotationChange = 0;

        if (manipulationState is not null && _previousManipulationState is not null)
            _totalRotationChange += manipulationState.GetRotationChange(_previousManipulationState);

        var manipulation = GetManipulation();
        if (manipulation is not null)
            onManipulation(manipulation);
    }

    private record ManipulationState(ScreenPosition Center, double? Radius, double? Angle, int LocationsLength)
    {
        public double GetRotationChange(ManipulationState previousManipulationState)
        {
            if (Angle is null)
                return 0;
            if (previousManipulationState.Angle is null)
                return 0;
            return Angle.Value - previousManipulationState.Angle.Value;
        }

        public double GetScaleFactor(ManipulationState previousManipulationState)
        {
            if (Radius is null)
                return 1;
            if (previousManipulationState.Radius is null)
                return 1;
            return Radius.Value / previousManipulationState.Radius.Value;
        }
    }
}

public record Manipulation(ScreenPosition Center, ScreenPosition PreviousCenter, double ScaleFactor, double RotationChange, double TotalRotationChange);
