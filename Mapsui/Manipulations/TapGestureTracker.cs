using System;

namespace Mapsui.Manipulations;

public class TapGestureTracker
{
    private readonly double _maxTapDuration = 0.5;
    private DateTime _tapStartTime;
    private MPoint? _tapStartPosition;
    private MPoint? _tapEndPosition;

    public void IfTap(Action<MPoint> onTap, double maxTapDistance)
    {
        if (_tapStartPosition == null) return;
        if (_tapEndPosition == null) return;

        var duration = (DateTime.Now - _tapStartTime).TotalSeconds;
        var distance = _tapEndPosition.Distance(_tapStartPosition);
        var isTap = duration < _maxTapDuration && distance < maxTapDistance;

        if (isTap) onTap(_tapEndPosition);
    }

    public void Move(MPoint position)
    {
        _tapEndPosition = position;
    }

    public void Start(MPoint position)
    {
        _tapStartTime = DateTime.Now;
        _tapStartPosition = position;
    }
}
