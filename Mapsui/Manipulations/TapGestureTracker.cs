using System;
using System.Threading.Tasks;

namespace Mapsui.Manipulations;

public class TapGestureTracker
{
    private readonly double _maxTapDuration = 0.5;
    private DateTime _tapStartTime;
    private MPoint? _tapStartPosition;
    private int _millisecondsToWaitForDoubleTap = 250;
    private bool _waitingForDoubleTap;
    private int _tapCount = 1;

    // This fields was added as a workaround for that in Blazor the touch up does not have a location (or I do not know how to get it).
    public MPoint? LastMovePosition { get; set; }

    public void IfTap(MPoint tapEndPosition, double maxTapDistance, Action<MPoint, int> onTap)
    {
        if (_tapStartPosition == null) return;
        if (tapEndPosition == null) return; // Note, this uses the tapEndPosition parameter.

        var duration = (DateTime.Now - _tapStartTime).TotalSeconds;
        var distance = tapEndPosition.Distance(_tapStartPosition);
        var isTap = duration < _maxTapDuration && distance < maxTapDistance;

        if (_waitingForDoubleTap)
            _tapCount = 2;
        else if (isTap)
            _ = OnTapAfterDelayAsync(onTap, tapEndPosition); // Fire and forget
    }

    public void SetDownPosition(MPoint position)
    {
        _tapStartTime = DateTime.Now;
        _tapStartPosition = position;
    }

    private async Task OnTapAfterDelayAsync(Action<MPoint, int> onTap, MPoint position)
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
