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

    public void IfTap(ScreenPosition? tapEndPosition, double maxTapDistance, Action<ScreenPosition, int> onTap)
    {
        if (_tapStartPosition is null) return;
        if (tapEndPosition is null) return; // Note, this uses the tapEndPosition parameter.

        var duration = (DateTime.Now - _tapStartTime).TotalSeconds;
        var distance = tapEndPosition.Value.Distance(_tapStartPosition.Value);
        var isTap = duration < _maxTapDuration && distance < maxTapDistance;

        if (isTap)
        {
            if (_waitingForDoubleTap)
            {
                onTap(tapEndPosition.Value, 2); // Within wait period so fire.
            }
            else
            {
                // This is the first tap. Fire right away and start waiting for second tap.
                // If the second tap is within the wait period we should fire a double tap
                // but not another single tap.
                onTap(tapEndPosition.Value, 1);
                _ = StartWaitingForSecondTapAsync(); // Fire and forget
            }
        }
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
