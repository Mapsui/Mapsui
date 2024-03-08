using System;
using System.Threading.Tasks;

namespace Mapsui.Manipulations;

public class TapGestureTracker
{
    private readonly double _maxTapDuration = 0.5;
    private DateTime _tapStartTime;
    private MPoint? _tapStartPosition;
    private MPoint? _tapEndPosition;
    private int _millisecondsToWaitForDoubleTap = 250;
    private bool _waitingForDoubleTap;
    private int _tapCount = 1;

    public void IfTap(MPoint tapEndPosition, double maxTapDistance, Action<MPoint, int> onTap)
    {
        if (_tapStartPosition == null) return;
        if (tapEndPosition == null) return; // Note, this uses the tapEndPosition parameter.

        IfTap(_tapStartPosition, tapEndPosition, maxTapDistance, onTap);
    }

    /// <summary>
    /// Use this method in Blazor or other platforms where the mouse up position is unknown. Use this in combination 
    /// with SetLastMovePosition.
    /// </summary>
    /// <param name="maxTapDistance"></param>
    /// <param name="onTap"></param>
    public void IfTap(double maxTapDistance, Action<MPoint, int> onTap)
    {
        if (_tapStartPosition == null) return;
        if (_tapEndPosition == null) return; // Note, this uses the _tapEndPosition field.

        IfTap(_tapStartPosition, _tapEndPosition, maxTapDistance, onTap);
    }

    private void IfTap(MPoint tapStartPosition, MPoint tapEndPosition, double maxTapDistance, Action<MPoint, int> onTap)
    {
        if (tapStartPosition == null) return;
        if (tapEndPosition == null) return;

        var duration = (DateTime.Now - _tapStartTime).TotalSeconds;
        var distance = tapEndPosition.Distance(tapStartPosition);
        var isTap = duration < _maxTapDuration && distance < maxTapDistance;

        if (_waitingForDoubleTap)
            _tapCount = 2;
        else if (isTap)
            _ = OnTapAfterDelayAsync(onTap, tapEndPosition); // Fire and forget
    }

    /// <summary>
    /// Call this method during move if the platform does not provide the mouse up position.
    /// </summary>
    /// <param name="position"></param>
    public void SetLastMovePosition(MPoint position)
    {
        _tapEndPosition = position;
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
