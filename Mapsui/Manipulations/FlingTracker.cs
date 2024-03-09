#pragma warning disable IDE0005
using System;
using System.Collections.Generic;

namespace Mapsui.Manipulations;

public class FlingTracker
{
    private const int _maxSize = 50;
    private const long _maxTicks = 200 * 10000;  // Use only events from the last 200 ms

    private readonly Queue<(double x, double y, long time)> _events;

    public FlingTracker()
    {
        _events = [];
    }

    public void AddEvent(ScreenPosition position, long ticks)
    {
        _events.Enqueue((position.X, position.Y, ticks));

        // Check, if we at the end of array
        if (_events.Count > 2)
        {
            while (_events.Count > _maxSize || _events.Peek().time < ticks - _maxTicks)
                _events.Dequeue();
        }
    }

    public void Restart()
    {
        _events.Clear();
    }

    private (double vx, double vy) CalcVelocity(long now)
    {
        double distanceX = 0;
        double distanceY = 0;

        var eventQueue = _events;
        var eventsArray = eventQueue.ToArray();

        if (eventsArray.Length == 0)
            return (0, 0);

        (_, _, var firstTime) = eventsArray[0];

        long finalTime = 0;

        for (var i = 1; i < eventsArray.Length; i++)
        {
            (var lastX, var lastY, var lastTime) = eventsArray[i - 1];
            (var nowX, var nowY, var nowTime) = eventsArray[i];

            // Only calc velocities for last maxTicks ticks
            if (now - lastTime < _maxTicks)
            {
                // Calc velocity in pixel per sec
                distanceX += (nowX - lastX) * 10000000;// / (nowTime - lastTime);
                distanceY += (nowY - lastY) * 10000000;// / (nowTime - lastTime);
            }

            finalTime = nowTime;
        }

        var totalTime = finalTime - firstTime;

        return (distanceX / totalTime, distanceY / totalTime);
    }

    public void FlingIfNeeded(Action<double, double> onFling)
    {
        var (velocityX, velocityY) = CalcVelocity(DateTime.Now.Ticks);

        if (Math.Abs(velocityX) <= 200 && Math.Abs(velocityY) <= 200)
            return;

        onFling(velocityX, velocityY);
    }
}
