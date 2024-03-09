using System;
using System.Threading.Tasks;

namespace Mapsui.Manipulations;

public class TapGestureTracker
{
    private readonly double _maxTapDuration = 0.5;
    private DateTime _tapStartTime;
    private ScreenPosition? _tapStartPosition;
    private int _millisecondsToWaitForDoubleTap = 300;
    private bool _waitingForDoubleTap;
    private int _tapCount = 1;

    // This causes a delay on the single tap. This is not always the desired behavior. When this is set to false
    // The single tap will fire each time before the double tap. This is the default behavior in most systems.
    public bool DoNotFireSingleTapOnDoubleTap { get; set; } = false;

    // This fields was added as a workaround for that in Blazor the touch up does not have a location (or I do not know how to get it).
    public ScreenPosition? LastMovePosition { get; set; }

    public void IfTap(ScreenPosition? tapEndPosition, double maxTapDistance, Action<ScreenPosition, int> onTap)
    {
        if (_tapStartPosition is null) return;
        if (tapEndPosition is null) return; // Note, this uses the tapEndPosition parameter.

        var duration = (DateTime.Now - _tapStartTime).TotalSeconds;
        var distance = tapEndPosition.Value.Distance(_tapStartPosition.Value);
        var isTap = duration < _maxTapDuration && distance < maxTapDistance;

        if (DoNotFireSingleTapOnDoubleTap)
        {
            if (_waitingForDoubleTap)
                _tapCount = 2;
            else if (isTap)
                _ = OnTapAfterDelayAsync(onTap, tapEndPosition.Value); // Fire and forget
        }
        else
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

    private async Task OnTapAfterDelayAsync(Action<ScreenPosition, int> onTap, ScreenPosition position)
    {
        // In the current implementation we always wait for the double tap. This is not 
        // always the desired behavior. Sometimes you want to respond directly. But in that
        // case a double tap will always be preceded by a single tap. Options to resolve this:
        // - Make it configurable
        // - Add an OnSingleTap event (so 3 types, to make it more comprehensible OnDoubleTap should be real event).
        // - Invoke the OnTap also in the OnSingleTap scenario but with different parameters.
        _waitingForDoubleTap = true;
        await Task.Delay(_millisecondsToWaitForDoubleTap);

        onTap(position, _tapCount); // The tap count could be set to 2 during waiting.

        _waitingForDoubleTap = false;
        _tapCount = 1;
    }
}
