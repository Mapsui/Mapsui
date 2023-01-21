using System.Collections.Generic;

namespace Mapsui.UI.Utils;

public class FlingTracker
{
    private const int maxSize = 50;
    private const long maxTicks = 200 * 10000;  // Use only events from the last 200 ms

    private readonly Dictionary<long, Queue<(double x, double y, long time)>> events;

    public FlingTracker()
    {
        events = new Dictionary<long, Queue<(double x, double y, long time)>>();
    }

    public void AddEvent(long id, MPoint location, long ticks)
    {
        // Save event data
        if (!events.ContainsKey(id))
        {
            events.Add(id, new Queue<(double x, double y, long time)>());
        }

        events[id].Enqueue((location.X, location.Y, ticks));

        // Check, if we at the end of array
        if (events[id].Count > 2)
        {
            while (events[id].Count > maxSize || events[id].Peek().time < (ticks - maxTicks))
                events[id].Dequeue();
        }
    }

    // STOP TRACKING THIS ONE
    public void RemoveId(long id)
    {
        if (events.ContainsKey(id))
        {
            events.Remove(id);
        }
    }

    public void Clear()
    {
        events.Clear();
    }

    public (double vx, double vy) CalcVelocity(long id, long now)
    {
        double distanceX = 0;
        double distanceY = 0;

        if (!events.ContainsKey(id) || events[id].Count < 2)
            return (0d, 0d);

        var eventQueue = events[id];
        var eventsArray = eventQueue.ToArray();

        (_, _, var firstTime) = eventsArray[0];

        long finalTime = 0;

        for (var i = 1; i < eventsArray.Length; i++)
        {
            (var lastX, var lastY, var lastTime) = eventsArray[i - 1];
            (var nowX, var nowY, var nowTime) = eventsArray[i];

            // Only calc velocities for last maxTicks ticks
            if (now - lastTime < maxTicks)
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
}
