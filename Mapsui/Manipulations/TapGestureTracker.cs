using System;
using System.Threading.Tasks;

namespace Mapsui.Manipulations;

public class TapGestureTracker
{
    private readonly double _maxTapDuration = 0.5;
    private DateTime _tapStartTime;
    private ScreenPosition? _tapStartPosition;
    private readonly int _millisecondsToWaitForDoubleTap = 300;
    private bool _waitingForDoubleTap;

    /// <returns>Indicates if the event was handled. If it is handled the called should not do any further
    /// handling. The implementation of the tap event determines if the event is handled.</returns>
    public bool TapIfNeeded(ScreenPosition? tapEndPosition, double maxTapDistance, Func<ScreenPosition, int, bool> onTap)
    {
        if (_tapStartPosition is null) return false;
        if (tapEndPosition is null) return false; // Note, this uses the tapEndPosition parameter.

        var duration = (DateTime.Now - _tapStartTime).TotalSeconds;
        var distance = tapEndPosition.Value.Distance(_tapStartPosition.Value);
        var isTap = duration < _maxTapDuration && distance < maxTapDistance;

        if (isTap)
        {
            if (_waitingForDoubleTap)
            {
                // Todo: For double tap we need to check against the previous tapEndPosition
                return onTap(tapEndPosition.Value, 2); // Within wait period so fire.
            }
            else
            {
                // This is the first tap. Fire right away and start waiting for second tap.
                // If the second tap is within the wait period we should fire a double tap
                // but not another single tap.
                _ = StartWaitingForSecondTapAsync(); // Fire and forget
                return onTap(tapEndPosition.Value, 1);
            }
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
