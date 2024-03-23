using System;
using System.Threading.Tasks;

namespace Mapsui.Manipulations;

public enum TapType
{
    Single,
    Double,
    Long
}

public class TapGestureTracker
{
    private readonly double _maxTapDuration = 0.5;
    private readonly double _noTapInterval = 0.25;
    private readonly double _maxLongTapDuration = 3;
    private DateTime _tapStartTime;
    private ScreenPosition? _tapStartPosition;
    private readonly int _millisecondsToWaitForDoubleTap = 300;
    private bool _waitingForDoubleTap;
    private ScreenPosition? _previousTapPosition; // Needed to calculate distance for double tap

    /// <returns>Indicates if the event was handled. If it is handled the caller should not do any further
    /// handling. The implementation of the tap event determines if the event is handled.</returns>
    public bool TapIfNeeded(ScreenPosition? tapEndPosition, double maxTapDistance, Func<ScreenPosition, TapType, bool> onTap)
    {
        if (_tapStartPosition is null) return false;
        if (tapEndPosition is null) return false; // Note, this uses the tapEndPosition parameter.

        var duration = (DateTime.Now - _tapStartTime).TotalSeconds;
        var distance = tapEndPosition.Value.Distance(_tapStartPosition.Value);
        var isTap = duration < _maxTapDuration && distance < maxTapDistance; // This distance check is between start and end position.

        if (isTap)
        {
            if (_waitingForDoubleTap)
            {
                if (_previousTapPosition is null) return false;
                var distanceToPreviousTap = tapEndPosition.Value.Distance(_previousTapPosition.Value);
                _previousTapPosition = null;
                if (duration < _maxTapDuration && distanceToPreviousTap < maxTapDistance) // This distance check is between this and the previous tap.
                    return onTap(tapEndPosition.Value, TapType.Double); // Within wait period so fire.
            }
            else
            {
                _previousTapPosition = tapEndPosition;
                // This is the first tap. Fire right away and start waiting for the second tap.
                // If the second tap is within the wait period we should fire a double tap
                // but not another single tap.
                _ = StartWaitingForSecondTapAsync(); // Fire and forget
                return onTap(tapEndPosition.Value, TapType.Single);
            }
        }
        else
        {
            var minLongTapDuration = _maxTapDuration + _noTapInterval; // Not sure how useful the no-tap interval is, made it up myself.
            var isLongTap =
                duration > minLongTapDuration
                && duration < _maxLongTapDuration
                && distance < maxTapDistance;

            if (isLongTap)
                return onTap(tapEndPosition.Value, TapType.Long);
        }
        return false;
    }

    private async Task StartWaitingForSecondTapAsync()
    {
        _waitingForDoubleTap = true;
        await Task.Delay(_millisecondsToWaitForDoubleTap);
        _waitingForDoubleTap = false;
    }

    public void Restart(ScreenPosition position)
    {
        _tapStartTime = DateTime.Now;
        _tapStartPosition = position;
    }
}
